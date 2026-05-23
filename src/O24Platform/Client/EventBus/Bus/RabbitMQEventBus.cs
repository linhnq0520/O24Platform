using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using O24OpenAPI.Client.EventBus.Abstractions;
using O24OpenAPI.Contracts.Events;
using O24OpenAPI.Core.Configuration;
using O24OpenAPI.Core.Infrastructure;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace O24OpenAPI.Client.EventBus.Bus;

public sealed class RabbitMQEventBus(
    IServiceProvider serviceProvider,
    IOptions<EventBusOptions> options,
    IOptions<EventBusSubscriptionInfo> subscriptionOptions
) : IEventBus, IDisposable, IHostedService
{
    private const string ExchangeName = "o24_event_bus";

    private readonly ResiliencePipeline _pipeline = CreateResiliencePipeline(
        options.Value.RetryCount
    );

    private readonly string _queueName =
        $"{ExchangeName}_queue_{Singleton<O24OpenAPIConfiguration>.Instance!.YourServiceID}";

    private readonly EventBusSubscriptionInfo _subscriptionInfo = subscriptionOptions.Value;

    private IConnection? _rabbitMQConnection;
    private IChannel? _consumerChannel;

    private Task? _executingTask;
    private CancellationTokenSource? _stoppingCts;

    private IConnection EnsureConnection()
    {
        if (_rabbitMQConnection == null || !_rabbitMQConnection.IsOpen)
        {
            var newConn = Singleton<QueueClient>.Instance?.GetConnection();

            if (newConn == null)
            {
                Console.WriteLine("RabbitMQ connection is null (QueueClient not ready).");
                throw new InvalidOperationException("RabbitMQ connection not available.");
            }

            _rabbitMQConnection = newConn;
            Console.WriteLine("RabbitMQ connection (re)established.");
        }

        return _rabbitMQConnection;
    }

    private async Task<IChannel> EnsureConsumerChannel(
        CancellationToken cancellationToken = default
    )
    {
        if (_consumerChannel != null && _consumerChannel.IsOpen)
        {
            return _consumerChannel;
        }

        var connection = EnsureConnection();
        _consumerChannel = await connection.CreateChannelAsync(
            cancellationToken: cancellationToken
        );

        _consumerChannel.CallbackExceptionAsync += async (sender, ea) =>
        {
            Console.WriteLine(ea.Exception.ToString());
            Console.WriteLine("RabbitMQ consumer channel exception. Recreating... ");
            _consumerChannel?.Dispose();
            _consumerChannel = null;
            await EnsureConsumerChannel(cancellationToken);
        };

        // Declare exchange + queue
        await _consumerChannel.ExchangeDeclareAsync(exchange: ExchangeName, type: "direct");
        await _consumerChannel.QueueDeclareAsync(
            queue: _queueName,
            durable: true,
            exclusive: false,
            autoDelete: true,
            arguments: null
        );

        var consumer = new AsyncEventingBasicConsumer(_consumerChannel);
        consumer.ReceivedAsync += OnMessageReceived;

        await _consumerChannel.BasicConsumeAsync(
            queue: _queueName,
            autoAck: false,
            consumer: consumer
        );

        foreach (var (eventName, _) in _subscriptionInfo.EventTypes)
        {
            await _consumerChannel.QueueBindAsync(
                queue: _queueName,
                exchange: ExchangeName,
                routingKey: eventName
            );
        }

        Console.WriteLine("RabbitMQ consumer channel started.");
        return _consumerChannel;
    }

    /// <summary>
    /// Publish an Integration Event to RabbitMQ
    /// </summary>
    /// <param name="event"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task PublishAsync(
        IntegrationEvent @event,
        CancellationToken cancellationToken = default
    )
    {
        var connection = EnsureConnection();
        var routingKey = @event.GetType().Name;

        var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
        await using (channel)
        {
            await channel.ExchangeDeclareAsync(
                exchange: ExchangeName,
                type: "direct",
                cancellationToken: cancellationToken
            );

            var body = SerializeMessage(@event);

            Console.WriteLine(
                $"Preparing to publish event. Exchange: {ExchangeName}, RoutingKey: {routingKey}, BodySize: {body?.Length ?? 0} bytes"
            );

            await _pipeline.ExecuteAsync(
                async ct =>
                {
                    try
                    {
                        var properties = new BasicProperties
                        {
                            DeliveryMode = DeliveryModes.Persistent,
                        };

                        await channel.BasicPublishAsync(
                            exchange: ExchangeName,
                            routingKey: routingKey,
                            mandatory: true,
                            basicProperties: properties,
                            body: body,
                            cancellationToken: ct
                        );

                        Console.WriteLine(
                            $"Event published successfully. RoutingKey: {routingKey}, EventType: {@event.GetType().Name}"
                        );
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            $"Failed to publish event. Exchange: {ExchangeName}, RoutingKey: {routingKey}, EventType: {@event.GetType().Name}. Exception: {ex}"
                        );
                        throw;
                    }
                },
                cancellationToken
            );
        }
    }

    /// <summary>
    /// On Message Received from RabbitMQ
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="eventArgs"></param>
    /// <returns></returns>
    private async Task OnMessageReceived(object sender, BasicDeliverEventArgs eventArgs)
    {
        Console.WriteLine(
            $"Received message with RoutingKey: {eventArgs.RoutingKey}, DeliveryTag: {eventArgs.DeliveryTag}"
        );
        var eventName = eventArgs.RoutingKey;
        var message = Encoding.UTF8.GetString(eventArgs.Body.Span);

        try
        {
            await ProcessEvent(eventName, message);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error processing message {Message}", message);
            Console.WriteLine(
                $"Error processing message with RoutingKey: {eventArgs.RoutingKey}, DeliveryTag: {eventArgs.DeliveryTag}. Exception: {ex}"
            );
        }

        if (_consumerChannel != null)
        {
            await _consumerChannel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false);
        }
    }

    private async Task ProcessEvent(string eventName, string message)
    {
        await using var scope = serviceProvider.CreateAsyncScope();

        if (!_subscriptionInfo.EventTypes.TryGetValue(eventName, out var eventType))
        {
            Console.WriteLine("Unknown event type {EventName}", eventName);
            return;
        }

        var integrationEvent = DeserializeMessage(message, eventType);

        foreach (
            var handler in scope.ServiceProvider.GetKeyedServices<IIntegrationEventHandler>(
                eventType
            )
        )
        {
            await handler.Handle(integrationEvent);
        }
    }

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        _stoppingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _executingTask = Task.Run(() => ExecuteAsync(_stoppingCts.Token), cancellationToken);

        return Task.CompletedTask;
    }

    private async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await EnsureConsumerChannel(cancellationToken);

                // giữ vòng lặp "ngủ" nhưng kiểm tra định kỳ channel
                while (
                    _consumerChannel?.IsOpen == true && !cancellationToken.IsCancellationRequested
                )
                {
                    await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
                }
            }
            catch (Exception)
            {
                Console.WriteLine("RabbitMQ consumer loop error. Retrying in 5s...");
                await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
            }
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (_executingTask == null)
        {
            return;
        }

        _stoppingCts?.Cancel();
        await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
    }

    public void Dispose()
    {
        try
        {
            _stoppingCts?.Cancel();
            _consumerChannel?.Dispose();
            _rabbitMQConnection?.Dispose();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    private static IntegrationEvent DeserializeMessage(string message, Type eventType) =>
        (IntegrationEvent)(
            JsonSerializer.Deserialize(message, eventType)
            ?? throw new InvalidOperationException(
                $"Unable to deserialize message to {eventType.FullName}"
            )
        );

    private static byte[] SerializeMessage(IntegrationEvent @event) =>
        JsonSerializer.SerializeToUtf8Bytes(@event, @event.GetType());

    private static ResiliencePipeline CreateResiliencePipeline(int retryCount)
    {
        var retryOptions = new RetryStrategyOptions
        {
            ShouldHandle = new PredicateBuilder()
                .Handle<BrokerUnreachableException>()
                .Handle<SocketException>(),
            MaxRetryAttempts = retryCount,
            DelayGenerator = (context) =>
                ValueTask.FromResult(GenerateDelay(context.AttemptNumber)),
        };

        return new ResiliencePipelineBuilder().AddRetry(retryOptions).Build();

        static TimeSpan? GenerateDelay(int attempt)
        {
            return TimeSpan.FromSeconds(Math.Pow(2, attempt));
        }
    }
}
