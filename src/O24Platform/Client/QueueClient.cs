using LinKit.Json.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using O24OpenAPI.Client.Abstractions;
using O24OpenAPI.Client.EventBus.Abstractions;
using O24OpenAPI.Client.Events;
using O24OpenAPI.Client.Events.EventData;
using O24OpenAPI.Client.Lib;
using O24OpenAPI.Client.Lib.Extensions;
using O24OpenAPI.Client.Scheme.Workflow;
using O24OpenAPI.Contracts.Configuration.Client;
using O24OpenAPI.Contracts.Events;
using O24OpenAPI.Core.Configuration;
using O24OpenAPI.Core.Domain.Logging;
using O24OpenAPI.Core.Extensions;
using O24OpenAPI.Core.Infrastructure;
using O24OpenAPI.Logging.Helpers;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.IO;
using System.Text.Json;

namespace O24OpenAPI.Client;

/// <summary>
/// The queue client class
/// </summary>
/// <seealso cref="MyConsole"/>
public class QueueClient : MyConsole
{
    public delegate Task MessageDeliveringDelegate(string content);

    public delegate Task WorkflowDeliveringDelegate(WFScheme workflow);

    public delegate Task WorkflowExecutionEventDelegate(
        O24OpenAPIEvent<WorkflowExecutionEventData> pWorkflowExecutionEvent
    );
    public delegate Task ServiceToServiceEventDelegate(
        O24OpenAPIEvent<ServiceToServiceEventData> pServiceToServiceEvent
    );

    public delegate Task MethodInvocationEventDelegate(
        O24OpenAPIEvent<MethodInvocationEventData> pMethodInvocationEvent
    );

    public delegate Task LogEventDelegate(O24OpenAPIEvent<LogEventData> pServiceToServiceEvent);

    public delegate Task EntityEventDelegate(O24OpenAPIEvent<EntityEvent> pServiceToServiceEvent);

    public delegate Task LogWorkflowStepDelegate(O24OpenAPIEvent<StepExecutionEvent> stepEvent);

    private bool IsConnected;

    private readonly Lock objLockSetup = new();

    private IConnection? __Connection;

    private IChannel? __CommandChannel;

    private IChannel? __EventChannel;

    private ConnectionFactory? __Factory;

    private readonly object __lockServiceInfoObject = new();

    private ServiceInfo __ServiceInfo = new();

    public string Local_SSL_CERT_PATH = "";

    private AsyncEventingBasicConsumer? CommandQueueConsumer;

    private AsyncEventingBasicConsumer? EventQueueConsumer;

    private static SemaphoreSlim? CommandSemaphore { get; set; }

    private static SemaphoreSlim? EventSemaphore { get; set; }

    public ServiceInfo ServiceInfo
    {
        get
        {
            lock (__lockServiceInfoObject)
            {
                return __ServiceInfo;
            }
        }
        set
        {
            lock (__lockServiceInfoObject)
            {
                __ServiceInfo = value;
            }
        }
    }

    public event MessageDeliveringDelegate? MessageDelivering;
    public event WorkflowDeliveringDelegate? WorkflowDelivering;
    public event WorkflowExecutionEventDelegate? WorkflowExecutionEvent;
    public event ServiceToServiceEventDelegate? ServiceToServiceEvent;
    public event MethodInvocationEventDelegate? MethodInvocationEvent;
    public event LogEventDelegate? LogEvent;
    public event EntityEventDelegate? EntityEvent;
    public event LogWorkflowStepDelegate? StepEvent;
    private readonly ITransactionDispatcher? _transactionDispatcher;

    public QueueClient(ITransactionDispatcher? transactionDispatcher = null)
    {
        O24OpenAPIClientConfiguration o24Config =
            Singleton<O24OpenAPIClientConfiguration>.Instance
            ?? throw new InvalidOperationException(
                "O24OpenAPIClientConfiguration is not configured."
            );
        if (o24Config.YourServiceID.Equals("WFO", StringComparison.OrdinalIgnoreCase))
        {
            ServiceInfoConfiguration? wfoServiceInfo =
                (Singleton<AppSettings>.Instance?.Get<ServiceInfoConfiguration>())
                ?? throw new InvalidOperationException(
                    "ServiceInfoConfiguration for WFO is not configured."
                );
            ServiceInfo = new ServiceInfo(wfoServiceInfo);
        }
        else
        {
            ServiceInfo = new ServiceInfo();
            O24OpenAPIClientConfiguration? o24config =
                Singleton<O24OpenAPIClientConfiguration>.Instance;
            ServiceInfo
                .QueryServiceInfo(
                    o24config.YourServiceID,
                    o24config.YourServiceID,
                    o24config.YourInstanceID
                )
                .GetAwaiter()
                .GetResult();
        }

        Console.WriteLine(
            "QueueClient initialized with ServiceInfo: "
                + JsonConvert.SerializeObject(ServiceInfo, Formatting.Indented)
        );

        if (ServiceInfo.ssl_active?.Equals("Y", StringComparison.OrdinalIgnoreCase) == true)
        {
            Local_SSL_CERT_PATH = Directory.GetCurrentDirectory() + "/queue-client-cert.pfx";
            if (string.IsNullOrWhiteSpace(ServiceInfo.ssl_cert_base64))
            {
                throw new InvalidOperationException("SSL is active but ssl_cert_base64 is empty.");
            }
            byte[] bytes = Convert.FromBase64String(ServiceInfo.ssl_cert_base64);
            File.WriteAllBytes(Local_SSL_CERT_PATH, bytes);
        }
        if (ServiceInfo.concurrent_threads <= 0 || ServiceInfo.concurrent_threads > short.MaxValue)
        {
            ServiceInfo.concurrent_threads = short.MaxValue;
        }
        CommandSemaphore = new SemaphoreSlim(
            (int)ServiceInfo.concurrent_threads,
            (int)ServiceInfo.concurrent_threads
        );
        EventSemaphore = new SemaphoreSlim(
            (int)ServiceInfo.concurrent_threads,
            (int)ServiceInfo.concurrent_threads
        );
        SchedulerService.IntervalInSeconds(
            o24Config.YourInstanceID,
            ServiceInfo.broker_reconnect_interval_in_seconds,
            delegate
            {
                try
                {
                    lock (objLockSetup)
                    {
                        Console.WriteLine("Queue Connection status == " + IsConnected);
                        if (!IsConnected)
                        {
                            WriteLine(
                                $"Reconnect to queue server (every {ServiceInfo.broker_reconnect_interval_in_seconds} seconds) start..."
                            );
                            AutoConfigure().GetAwaiter().GetResult();
                            WriteLine(
                                $"Reconnect to queue server (every {ServiceInfo.broker_reconnect_interval_in_seconds} seconds) ended."
                            );
                        }
                    }
                }
                catch (Exception ex)
                {
                    BusinessLogHelper.Error(ex, ex.Message);
                    WriteLine(ex);
                }
            }
        );

        _transactionDispatcher = transactionDispatcher;
    }

    /// <summary>
    /// Creates the factory
    /// </summary>
    /// <returns>A task containing the bool</returns>
    private async Task<bool> CreateFactory()
    {
        try
        {
            if (__Connection != null)
            {
                await __Connection.CloseAsync();
            }

            __Connection?.Dispose();
        }
        catch { }
        __Factory = new ConnectionFactory
        {
            VirtualHost = ServiceInfo.broker_virtual_host,
            HostName = ServiceInfo.broker_hostname,
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(15.0),
            RequestedHeartbeat = TimeSpan.FromSeconds(30.0),
            ClientProvidedName = "QueueClient:" + ServiceInfo.service_code,
        };
        if (ServiceInfo.ssl_active?.Equals("Y", StringComparison.OrdinalIgnoreCase) == true)
        {
            if (ServiceInfo.ssl_cert_servername == null)
            {
                throw new InvalidOperationException(
                    "SSL is active but ssl_cert_servername is null."
                );
            }
            __Factory.Ssl.Enabled = true;
            __Factory.Ssl.CertPath = Local_SSL_CERT_PATH;
            __Factory.Ssl.CertPassphrase = ServiceInfo.ssl_cert_pass_pharse;
            __Factory.Ssl.ServerName = ServiceInfo.ssl_cert_servername;
            WriteLine("Connect to RabbitMQ server in SSL mode:");
            WriteLine("- Factory.Ssl.CertPath: " + __Factory.Ssl.CertPath);
            WriteLine("- Factory.Endpoint: " + __Factory.Endpoint);
        }
        else
        {
            WriteLine("Connect to RabbitMQ server in Non-SSL mode:");
            WriteLine("- Factory.Endpoint: " + __Factory.Endpoint);
        }
        return true;
    }

    /// <summary>
    /// Auto configure
    /// </summary>
    /// <returns>A task containing the bool</returns>
    public async Task<bool> AutoConfigure()
    {
        await Setup();
        if (MessageDelivering != null || WorkflowDelivering != null)
        {
            await Subscribe();
        }
        WriteLine("AutoConfigure() done.");
        ThreadPool.GetMinThreads(out int workerThreads, out int completionPortThreads);
        WriteLine($"AutoConfigure(): minWorker={workerThreads}; minIOC={completionPortThreads}");
        ThreadPool.SetMinThreads(200, completionPortThreads);
        ThreadPool.GetMaxThreads(out int workerThreads1, out int completionPortThreads1);
        WriteLine($"AutoConfigure(): maxWorker={workerThreads1}; maxIOC={completionPortThreads1}");
        return true;
    }

    /// <summary>
    /// Creates the connection
    /// </summary>
    /// <returns>A task containing the bool</returns>
    private async Task<bool> CreateConnection()
    {
        if (__Factory == null)
        {
            await CreateFactory();
        }
        try
        {
            __CommandChannel?.Dispose();
            __EventChannel?.Dispose();
        }
        catch { }
        try
        {
            __Connection?.CloseAsync();
            __Connection?.Dispose();
        }
        catch { }
        __Factory!.HostName = ServiceInfo.broker_hostname;
        __Factory.Port = (ServiceInfo.broker_port > 0) ? ServiceInfo.broker_port : __Factory.Port;
        __Factory.VirtualHost = ServiceInfo.broker_virtual_host;
        __Factory.UserName = ServiceInfo.broker_user_name;
        __Factory.Password = ServiceInfo.broker_user_password;
        __Connection = await __Factory.CreateConnectionAsync();
        __Connection.ConnectionShutdownAsync += Connection_ConnectionShutdown;
        IsConnected = true;
        return true;
    }

    private async Task Connection_ConnectionShutdown(object sender, ShutdownEventArgs e)
    {
        try
        {
            if (__Connection != null)
                await __Connection.CloseAsync();
            __Connection?.Dispose();
        }
        catch { }
        WriteLine("Event [ConnectionShutdown]: " + e.ToString());
        IsConnected = false;
    }

    private async Task<bool> CreateChannel()
    {
        if (__Factory == null)
        {
            await CreateFactory();
        }
        if (__Connection == null)
        {
            await CreateConnection();
        }
        __CommandChannel = await __Connection!.CreateChannelAsync();
        __EventChannel = await __Connection.CreateChannelAsync();
        return true;
    }

    private async Task CommandMessageComingHandler(object sender, BasicDeliverEventArgs e)
    {
        if (WorkflowDelivering == null)
        {
            return;
        }
        if (CommandSemaphore is not null)
            await CommandSemaphore.WaitAsync();
        try
        {
            WFScheme workflow =
                e.Body.ToArray().FromBytes<WFScheme>()
                ?? throw new InvalidOperationException("Failed to deserialize JSON to WFScheme.");

            if (
                e.BasicProperties.Headers is null
                || !e.BasicProperties.Headers.TryGetValue(
                    "message_type",
                    out object? messageTypeObj
                )
                || messageTypeObj is not byte[] bytes
            )
            {
                Console.WriteLine("Missing or invalid 'message_type' header.");
                throw new InvalidOperationException("Missing or invalid 'message_type' header.");
            }

            if (
                e.BasicProperties.Headers.TryGetValue("work_context", out object? workContextObj)
                && workContextObj is byte[] workContextBytes
            )
            {
                WorkContextTemplate? workContext =
                    System.Text.Json.JsonSerializer.Deserialize<WorkContextTemplate>(
                        workContextBytes
                    );
                EngineContext.Current.ResolveRequired<WorkContext>().SetWorkContext(workContext);
            }
            else
            {
                ILoggerService? loggerService = EngineContext.Current.Resolve<ILoggerService>();
                loggerService?.LogErrorAsync(
                    $"Missing or invalid 'WorkContext' header. Headers: {e.BasicProperties.Headers}"
                );
            }

            string messageType = Encoding.UTF8.GetString(bytes);

            if (messageType.EqualsOrdinalIgnoreCase("workflow"))
            {
                if (ServiceInfo.is_tracking)
                {
                    workflow.request.request_header.tx_context.Add(
                        $"$$$$${workflow.request.request_header.tx_context.Count}_Service_Received_Message_From_Queue_At",
                        Utils.GetCurrentDateAsLongNumber()
                    );
                }
                if (
                    workflow.GetWorkerId() == null
                    || workflow.request.request_header.service_id.Equals(
                        "WFO",
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    Delegate[] invocationList = WorkflowDelivering.GetInvocationList();
                    List<Task> tasks = new(invocationList.Length);

                    foreach (Delegate del in invocationList)
                    {
                        if (del is WorkflowDeliveringDelegate handler)
                        {
                            tasks.Add(handler(workflow));
                        }
                    }

                    CommandSemaphore?.Release();
                    await Task.WhenAll(tasks);
                }
                else
                {
                    if (_transactionDispatcher == null)
                    {
                        throw new Exception(
                            $"Can not execute by worker [{workflow.GetWorkerId()}]"
                        );
                    }
                    _transactionDispatcher.Dispatch(
                        workflow,
                        EngineContext.Current.ResolveRequired<WorkContext>()
                    );
                }
            }
            else
            {
                CommandSemaphore?.Release();
            }
        }
        catch (Exception ex)
        {
            BusinessLogHelper.Error(ex, ex.Message);
            CommandSemaphore?.Release();
            throw;
        }
    }

    private async Task EventMessageComingHandler(object sender, BasicDeliverEventArgs e)
    {
        try
        {
            if (EventSemaphore is not null)
                await EventSemaphore.WaitAsync();
            if (WorkflowExecutionEvent == null)
            {
                return;
            }

            byte[] bodyArray = e.Body.ToArray();
            string messageContent = Encoding.UTF8.GetString(bodyArray);

            O24OpenAPIEvent<object>? eventData = System.Text.Json.JsonSerializer.Deserialize<
                O24OpenAPIEvent<object>
            >(messageContent);
            if (eventData is null)
            {
                return;
            }

            List<Task> tasks = [];

            switch (eventData.EventType)
            {
                case O24OpenAPIWorkflowEventTypeEnum.EventWorkflowRegistered:
                case O24OpenAPIWorkflowEventTypeEnum.EventWorkflowCompleted:
                case O24OpenAPIWorkflowEventTypeEnum.EventWorkflowError:
                case O24OpenAPIWorkflowEventTypeEnum.EventWorkflowTimeout:
                case O24OpenAPIWorkflowEventTypeEnum.EventWorkflowCompensated:
                case O24OpenAPIWorkflowEventTypeEnum.EventWorkflowReversed:
                case O24OpenAPIWorkflowEventTypeEnum.EventWorkflowStepStart:
                case O24OpenAPIWorkflowEventTypeEnum.EventWorkflowStepCompleted:
                case O24OpenAPIWorkflowEventTypeEnum.EventWorkflowStepError:
                case O24OpenAPIWorkflowEventTypeEnum.EventWorkflowStepTimeout:
                case O24OpenAPIWorkflowEventTypeEnum.EventWorkflowStepCompensated:
                    if (WorkflowExecutionEvent != null)
                    {
                        O24OpenAPIEvent<WorkflowExecutionEventData>? executionEvent =
                            System.Text.Json.JsonSerializer.Deserialize<
                                O24OpenAPIEvent<WorkflowExecutionEventData>
                            >(messageContent);
                        if (executionEvent != null)
                        {
                            tasks.Add(WorkflowExecutionEvent(executionEvent));
                        }
                    }
                    break;
                case O24OpenAPIWorkflowEventTypeEnum.EventServiceToService:
                    if (ServiceToServiceEvent != null)
                    {
                        O24OpenAPIEvent<ServiceToServiceEventData>? serviceEvent =
                            System.Text.Json.JsonSerializer.Deserialize<
                                O24OpenAPIEvent<ServiceToServiceEventData>
                            >(messageContent);
                        if (serviceEvent != null)
                        {
                            tasks.Add(ServiceToServiceEvent(serviceEvent));
                        }
                    }
                    break;
                case O24OpenAPIWorkflowEventTypeEnum.EventLog:
                    if (LogEvent != null)
                    {
                        O24OpenAPIEvent<LogEventData>? logEvent =
                            System.Text.Json.JsonSerializer.Deserialize<
                                O24OpenAPIEvent<LogEventData>
                            >(messageContent);
                        if (logEvent != null)
                        {
                            tasks.Add(LogEvent(logEvent));
                        }
                    }
                    break;
                case O24OpenAPIWorkflowEventTypeEnum.EventMethodInvocation:
                    if (MethodInvocationEvent != null)
                    {
                        O24OpenAPIEvent<MethodInvocationEventData>? invocationEvent =
                            System.Text.Json.JsonSerializer.Deserialize<
                                O24OpenAPIEvent<MethodInvocationEventData>
                            >(messageContent);
                        if (invocationEvent != null)
                        {
                            tasks.Add(MethodInvocationEvent(invocationEvent));
                        }
                    }
                    break;
                case O24OpenAPIWorkflowEventTypeEnum.EntityAction:
                    if (EntityEvent != null)
                    {
                        O24OpenAPIEvent<EntityEvent>? entityEvent =
                            System.Text.Json.JsonSerializer.Deserialize<
                                O24OpenAPIEvent<EntityEvent>
                            >(messageContent);
                        if (entityEvent != null)
                        {
                            tasks.Add(EntityEvent(entityEvent));
                        }
                    }
                    break;

                case O24OpenAPIWorkflowEventTypeEnum.EventLogWorkflowStep:
                    if (StepEvent != null)
                    {
                        O24OpenAPIEvent<StepExecutionEvent>? stepEvent =
                            System.Text.Json.JsonSerializer.Deserialize<
                                O24OpenAPIEvent<StepExecutionEvent>
                            >(messageContent);
                        if (stepEvent != null)
                        {
                            tasks.Add(StepEvent(stepEvent));
                        }
                    }
                    break;
            }

            if (tasks.Count > 0)
            {
                await Task.WhenAll(tasks);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Handler error: {ex}");
            BusinessLogHelper.Error(ex, ex.Message);
        }
        finally
        {
            EventSemaphore?.Release();
        }
    }

    private async Task<bool> CreateQueue(string pQueueName, bool autoDelete = false)
    {
        if (__CommandChannel == null)
        {
            await CreateChannel();
        }
        await __CommandChannel!.QueueDeclareAsync(
            pQueueName,
            durable: true,
            exclusive: false,
            autoDelete: autoDelete,
            null
        );
        return true;
    }

    private async Task<bool> DeleteQueue(string pQueueName)
    {
        if (__CommandChannel == null)
        {
            await CreateChannel();
        }
        await __CommandChannel!.QueueDeleteAsync(pQueueName, ifUnused: true, ifEmpty: true);
        return true;
    }

    private async Task Subscribe()
    {
        if (ServiceInfo.broker_queue_name.Length != 0)
        {
            if (__CommandChannel == null || __EventChannel == null)
            {
                await CreateChannel();
            }
            CommandQueueConsumer = new AsyncEventingBasicConsumer(__CommandChannel!);
            CommandQueueConsumer.ReceivedAsync += async (model, ea) =>
            {
                WorkContext workContext = new();
                IServiceScope? previousScope = AsyncScope.Scope;
                WorkContext? previousWorkContext = AsyncScope.WorkContext;
                IServiceScope? commandScope = null;
                try
                {
                    if (
                        ea.BasicProperties.Headers == null
                        || !ea.BasicProperties.Headers.TryGetValue(
                            "message_type",
                            out object? messageTypeObj
                        )
                        || messageTypeObj is not byte[] bytes
                    )
                    {
                        Console.WriteLine("Missing or invalid 'message_type' header.");
                        throw new InvalidOperationException(
                            "Missing or invalid 'message_type' header."
                        );
                    }
                    string messageType = Encoding.UTF8.GetString(bytes);
                    if (!messageType.Equals("workflow", StringComparison.OrdinalIgnoreCase))
                    {
                        ILoggerService? loggerService =
                            EngineContext.Current.Resolve<ILoggerService>();
                        loggerService?.LogErrorAsync(
                            $"Missing or invalid 'message_type' header. MessageType: {messageType}"
                        );
                        await TryAckCommandMessageAsync(ea.DeliveryTag);
                        return;
                    }
                    if (
                        !(
                            ea.BasicProperties.Headers.TryGetValue(
                                "work_context",
                                out object? workContextObj
                            ) && workContextObj is byte[] workContextBytes
                        )
                    )
                    {
                        throw new InvalidOperationException(
                            "Missing or invalid 'WorkContext' header."
                        );
                    }
                    WorkContextTemplate? workContextTemplate =
                        System.Text.Json.JsonSerializer.Deserialize<WorkContextTemplate>(
                            workContextBytes
                        );
                    workContext.SetWorkContext(workContextTemplate);

                    commandScope = EngineContext.Current.ServiceProvider.CreateAsyncScope();

                    WorkContext newWorkContext =
                        commandScope.ServiceProvider.GetRequiredService<WorkContext>();
                    newWorkContext.SetWorkContext(workContext);
                    AsyncScope.Scope = commandScope;
                    AsyncScope.WorkContext = newWorkContext;
                    await RabbitMqLogHelper.LogConsumeAsync(
                        serviceName: Singleton<O24OpenAPIConfiguration>.Instance!.YourServiceID,
                        source: $"Exchange: '{ea.Exchange}', Key: '{ea.RoutingKey}'",
                        body: ea.Body.ToArray(),
                        headers: ea.BasicProperties.Headers,
                        processAction: () => CommandMessageComingHandler(model, ea)
                    );
                    await TryAckCommandMessageAsync(ea.DeliveryTag);
                }
                catch (Exception ex)
                {
                    BusinessLogHelper.Error(ex, ex.Message);
                    RestoreCommandScope(commandScope, workContext);

                    if (await TryReplyWorkflowErrorAsync(ea, ex, workContext))
                    {
                        await TryAckCommandMessageAsync(ea.DeliveryTag);
                    }
                    else
                    {
                        await TryNackCommandMessageAsync(ea.DeliveryTag);
                    }
                }
                finally
                {
                    commandScope?.Dispose();
                    AsyncScope.Scope = previousScope;
                    AsyncScope.WorkContext = previousWorkContext;
                }
            };
            await __CommandChannel!.BasicConsumeAsync(
                ServiceInfo.broker_queue_name,
                autoAck: false,
                CommandQueueConsumer
            );
            EventQueueConsumer = new AsyncEventingBasicConsumer(__EventChannel!);
            EventQueueConsumer.ReceivedAsync += async (model, ea) =>
            {
                WorkContext workContext = new();
                try
                {
                    if (
                        ea.BasicProperties.Headers == null
                        || !ea.BasicProperties.Headers.TryGetValue(
                            "message_type",
                            out object? messageTypeObj
                        )
                        || messageTypeObj is not byte[] bytes
                    )
                    {
                        Console.WriteLine("Missing or invalid 'message_type' header.");
                        throw new InvalidOperationException(
                            "Missing or invalid 'message_type' header."
                        );
                    }
                    string messageType = Encoding.UTF8.GetString(bytes);
                    if (!messageType.Equals("event", StringComparison.OrdinalIgnoreCase))
                    {
                        ILoggerService? loggerService =
                            EngineContext.Current.Resolve<ILoggerService>();
                        loggerService?.LogErrorAsync(
                            $"Missing or invalid 'message_type' header. MessageType: {messageType}"
                        );
                        await __EventChannel!.BasicAckAsync(ea.DeliveryTag, multiple: false);
                    }

                    if (
                        !(
                            ea.BasicProperties.Headers.TryGetValue(
                                "work_context",
                                out object? workContextObj
                            ) && workContextObj is byte[] workContextBytes
                        )
                    )
                    {
                        throw new InvalidOperationException(
                            "Missing or invalid 'WorkContext' header."
                        );
                    }
                    WorkContextTemplate? workContextTemplate =
                        System.Text.Json.JsonSerializer.Deserialize<WorkContextTemplate>(
                            workContextBytes
                        );
                    workContext.SetWorkContext(workContextTemplate);

                    using IServiceScope scope =
                        EngineContext.Current.ServiceProvider.CreateAsyncScope();

                    WorkContext newWorkContext =
                        scope.ServiceProvider.GetRequiredService<WorkContext>();
                    newWorkContext.SetWorkContext(workContext);
                    AsyncScope.Scope = scope;
                    AsyncScope.WorkContext = newWorkContext;
                    await RabbitMqLogHelper.LogConsumeAsync(
                        serviceName: Singleton<O24OpenAPIConfiguration>.Instance!.YourServiceID,
                        source: $"Exchange: '{ea.Exchange}', Key: '{ea.RoutingKey}'",
                        body: ea.Body.ToArray(),
                        headers: ea.BasicProperties.Headers,
                        processAction: () => EventMessageComingHandler(model, ea)
                    );

                    await __EventChannel!.BasicAckAsync(ea.DeliveryTag, multiple: false);
                }
                catch
                {
                    await __EventChannel!.BasicNackAsync(ea.DeliveryTag, false, true);
                }
                finally
                {
                    AsyncScope.Clear();
                }
            };
            await __EventChannel!.BasicConsumeAsync(
                ServiceInfo.event_queue_name,
                autoAck: false,
                EventQueueConsumer
            );
        }
    }

    private static void RestoreCommandScope(IServiceScope? scope, WorkContext workContext)
    {
        if (scope is null)
        {
            return;
        }

        WorkContext scopedWorkContext = scope.ServiceProvider.GetRequiredService<WorkContext>();
        scopedWorkContext.SetWorkContext(workContext);
        AsyncScope.Scope = scope;
        AsyncScope.WorkContext = scopedWorkContext;
    }

    private async Task<bool> TryReplyWorkflowErrorAsync(
        BasicDeliverEventArgs eventArgs,
        Exception exception,
        WorkContext workContext
    )
    {
        try
        {
            WFScheme? workflow = eventArgs.Body.ToArray().FromBytes<WFScheme>();
            if (workflow is null)
            {
                return false;
            }

            workflow.SetWorkContext(workContext);
            workflow.response.status = WFScheme.RESPONSE.EnumResponseStatus.ERROR;
            workflow.response.error_code = "ERROR";
            workflow.response.error_message = exception.Message;
            workflow.response.raw_error = exception.ToString();
            workflow.response.data = System.Text.Json.JsonSerializer.Serialize(
                new { ERROR_MESSAGE = exception.Message }
            );

            await ReplyWorkflow(workflow);
            return true;
        }
        catch (Exception replyException)
        {
            BusinessLogHelper.Error(replyException, replyException.Message);
            return false;
        }
    }

    private async Task<bool> TryAckCommandMessageAsync(ulong deliveryTag)
    {
        try
        {
            if (__CommandChannel is null)
            {
                return false;
            }

            await __CommandChannel.BasicAckAsync(deliveryTag, multiple: false);
            return true;
        }
        catch (Exception ex)
        {
            BusinessLogHelper.Error(ex, ex.Message);
            return false;
        }
    }

    private async Task TryNackCommandMessageAsync(ulong deliveryTag)
    {
        try
        {
            if (__CommandChannel is not null)
            {
                await __CommandChannel.BasicNackAsync(deliveryTag, multiple: false, requeue: true);
            }
        }
        catch (Exception ex)
        {
            BusinessLogHelper.Error(ex, ex.Message);
        }
    }

    /// <summary>
    /// Sends the message using the specified p queue name
    /// </summary>
    /// <param name="pQueueName">The queue name</param>
    /// <param name="pContent">The content</param>
    /// <param name="messageType">The message type</param>
    public async Task SendMessage(
        string pQueueName,
        object pContent,
        string messageType = "workflow"
    )
    {
        await SendMessage(
            pQueueName,
            System.Text.Json.JsonSerializer.Serialize(pContent),
            messageType
        );
    }

    /// <summary>
    /// Sends the message using the specified p queue name
    /// </summary>
    /// <param name="pQueueName">The queue name</param>
    /// <param name="pContent">The content</param>
    /// <param name="messageType">The message type</param>
    public async Task SendMessage(
        string pQueueName,
        string pContent,
        string messageType = "workflow"
    )
    {
        string? jsonWorkContext = EngineContext.Current.Resolve<WorkContext>()?.ToJson();
        BasicProperties basicProperties = new()
        {
            ContentType = "text/plain",
            DeliveryMode = DeliveryModes.Persistent,
            Headers = new Dictionary<string, object?>
            {
                { "message_type", messageType },
                { "work_context", jsonWorkContext },
                {
                    "flow",
                    $"{Singleton<O24OpenAPIConfiguration>.Instance?.YourServiceID} -> {pQueueName}"
                },
            },
        };
        byte[] bytes = Encoding.UTF8.GetBytes(pContent);

        RabbitMqLogHelper.LogPublish(
            serviceName: Singleton<O24OpenAPIConfiguration>.Instance!.YourServiceID,
            destination: $"{pQueueName}", // Mô tả đích đến
            message: pContent,
            headers: basicProperties.Headers
        );

        await __CommandChannel!.BasicPublishAsync("", pQueueName, false, basicProperties, bytes);
    }

    /// <summary>
    /// Sends the workflow using the specified p queue name
    /// </summary>
    /// <param name="pQueueName">The queue name</param>
    /// <param name="pContent">The content</param>
    public async Task SendWorkflow(string pQueueName, string pContent)
    {
        BasicProperties basicProperties = new()
        {
            ContentType = "text/plain",
            DeliveryMode = DeliveryModes.Persistent,
            Headers = new Dictionary<string, object?>
            {
                { "message_type", "workflow" },
                { "work_context", EngineContext.Current.Resolve<WorkContext>()?.ToJson() },
            },
        };

        byte[] bytes = Encoding.UTF8.GetBytes(pContent);

        RabbitMqLogHelper.LogPublish(
            serviceName: Singleton<O24OpenAPIConfiguration>.Instance!.YourServiceID,
            destination: $"{pQueueName}", // Mô tả đích đến
            message: pContent,
            headers: basicProperties.Headers
        );
        await __CommandChannel!.BasicPublishAsync("", pQueueName, false, basicProperties, bytes);
    }

    /// <summary>
    /// Serializes the object with snake case naming using the specified obj
    /// </summary>
    /// <param name="obj">The obj</param>
    /// <returns>The string</returns>
    private static string? SerializeObjectWithSnakeCaseNaming(object obj)
    {
        if (obj == null)
        {
            return null;
        }

        JsonSerializerSettings settings = new()
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy(),
            },
            Formatting = Formatting.None,
        };

        return JsonConvert.SerializeObject(obj, settings);
    }

    /// <summary>
    /// Replies the workflow using the specified workflow
    /// </summary>
    /// <param name="workflow">The workflow</param>
    public async Task ReplyWorkflow(WFScheme workflow)
    {
        workflow.request.request_header.service_instance_id = Singleton<O24OpenAPIConfiguration>
            .Instance!
            .YourInstanceID;
        if (workflow.request.request_header.step_mode.EqualsOrdinalIgnoreCase("TWOWAY") == true)
        {
            workflow.request.request_header.service_id = "WFO";
            if (workflow.response?.data != null)
            {
                string? json = SerializeObjectWithSnakeCaseNaming(workflow.response.data);
                if (string.IsNullOrWhiteSpace(json))
                {
                    throw new InvalidOperationException(
                        "ReplyWorkflow: Serialized response data is null or whitespace."
                    );
                }
                //string json = workflow.response.data.ToJson(o => o.NamingConvention = NamingConvention.SnakeCaseLower);
                workflow.response.data = JsonDocument.Parse(json).RootElement;
                if (ServiceInfo.is_tracking)
                {
                    Dictionary<string, object> txContext = workflow
                        .request
                        .request_header
                        .tx_context;
                    txContext[$"$$$$${txContext.Count}_Service_Sent_Message_To_Queue_At"] =
                        Utils.GetCurrentDateAsLongNumber();
                }
            }

            await SendMessage(
                workflow.request.request_header.from_queue_name,
                System.Text.Json.JsonSerializer.Serialize(workflow)
            );
        }
        else if (!string.IsNullOrEmpty(workflow.request?.request_header?.fanout_exchange))
        {
            if (workflow.response?.data != null)
            {
                string? json = SerializeObjectWithSnakeCaseNaming(workflow.response.data);
                if (string.IsNullOrWhiteSpace(json))
                {
                    BusinessLogHelper.Warning(
                        "ReplyWorkflow: Serialized response data is null or whitespace."
                    );
                    return;
                }
                workflow.response.data = JsonDocument.Parse(json).RootElement;
            }

            IEventBus eventBus = EngineContext.Current.ResolveRequired<IEventBus>();
            WorkflowStepCompletedEvent workflowStepCompletedEvent = new(workflow.ToJson());
            await eventBus.PublishAsync(workflowStepCompletedEvent);
        }
    }

    private async Task FanoutMessage(string exchangeName, string message)
    {
        if (__CommandChannel is null)
        {
            BusinessLogHelper.Info("Command channel is null when fanout.");
            return;
        }
        BasicProperties basicProperties = new()
        {
            ContentType = "text/plain",
            DeliveryMode = DeliveryModes.Persistent,
            Headers = new Dictionary<string, object?>
            {
                { "message_type", "event" },
                { "work_context", EngineContext.Current.Resolve<WorkContext>()?.ToJson() },
            },
        };

        byte[] bytes = Encoding.UTF8.GetBytes(message);
        await __CommandChannel.BasicPublishAsync(exchangeName, "", false, basicProperties, bytes);
    }

    private async Task RegisterFanout()
    {
        O24OpenAPIConfiguration? o24Config = Singleton<O24OpenAPIConfiguration>.Instance;
        if (
            o24Config is not null
            && o24Config.FanoutExchanges != null
            && o24Config.FanoutExchanges.Count > 0
        )
        {
            foreach (string exchange in o24Config.FanoutExchanges)
            {
                await __CommandChannel!.ExchangeDeclareAsync(exchange, "fanout", true, false, null);
                await __CommandChannel.QueueBindAsync(
                    ServiceInfo.event_queue_name,
                    exchange,
                    "",
                    null
                );
            }
        }
    }

    /// <summary>
    /// Setup this instance
    /// </summary>
    /// <returns>A task containing the bool</returns>
    private async Task<bool> Setup()
    {
        await CreateFactory();
        await CreateConnection();
        await CreateChannel();
        O24OpenAPIConfiguration? o24Config = Singleton<O24OpenAPIConfiguration>.Instance;
        if (o24Config == null)
        {
            throw new InvalidOperationException("O24OpenAPIConfiguration instance is null.");
        }
        await CreateQueue(ServiceInfo.broker_queue_name, o24Config.AutoDeleteCommandQueue);
        await CreateQueue(ServiceInfo.event_queue_name, o24Config.AutoDeleteEventQueue);
        await RegisterFanout();
        return true;
    }

    public IConnection GetConnection()
    {
        if (__Connection == null || !__Connection.IsOpen)
        {
            throw new InvalidOperationException("Connection is not established or is closed.");
        }
        return __Connection;
    }

    /// <summary>
    /// Disposes this instance
    /// </summary>
    public void Dispose()
    {
        __CommandChannel?.Dispose();
        __EventChannel?.Dispose();
        __Connection?.Dispose();
    }
}
