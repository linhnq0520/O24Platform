using O24OpenAPI.Client;
using O24OpenAPI.Client.Events;
using O24OpenAPI.Client.Events.EventData;
using O24OpenAPI.Client.Scheme.Workflow;
using O24OpenAPI.Core.Configuration;
using O24OpenAPI.Core.Events;
using O24OpenAPI.Core.Infrastructure;
using O24OpenAPI.Framework.Services.Events;
using System.Runtime.CompilerServices;

namespace O24OpenAPI.Framework.Services.Queue;

public class QueueContext
{
    [MethodImpl(MethodImplOptions.Synchronized)]
    public static QueueClient GetInstance()
    {
        if (!Singleton<O24OpenAPIConfiguration>.Instance.ConnectToWFO)
        {
            return null;
        }
        try
        {
            if (Singleton<QueueClient>.Instance != null)
            {
                return Singleton<QueueClient>.Instance;
            }
            QueueClient queueClient = new(new TransactionDispatcher());
            queueClient.WorkflowDelivering += WorkflowDelivering;
            queueClient.WorkflowExecutionEvent += WorkflowEvent;
            queueClient.ServiceToServiceEvent += _ServiceToServiceEvent;
            queueClient.LogEvent += _LogEvent;
            queueClient.EntityEvent += _EntityEvent;
            queueClient.StepEvent += LogWorkflowStepEvent;
            queueClient.AutoConfigure().GetAwaiter().GetResult();
            Singleton<QueueClient>.Instance = queueClient;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Queue client initialization failed: " + ex.Message);
            Singleton<QueueClient>.Instance = null;
        }
        return Singleton<QueueClient>.Instance;
    }

    internal static async Task WorkflowDelivering(WFScheme workflow)
    {
        workflow = await EngineContext
            .Current.Resolve<IO24OpenAPIServiceManager>()
            .ConsumeWorkflow(workflow);
        await Singleton<QueueClient>.Instance.ReplyWorkflow(workflow);
    }

    private static async Task WorkflowEvent(O24OpenAPIEvent<WorkflowExecutionEventData> eventData)
    {
        IEventPublisher eventPublisher = EngineContext.Current.Resolve<IEventPublisher>();
        WorkflowEvent evt = new(eventData);
        await eventPublisher.PublishWorkflowEvent(evt);
    }

    private static async Task _ServiceToServiceEvent(
        O24OpenAPIEvent<ServiceToServiceEventData> eventData
    )
    {
        IEventPublisher eventPublisher = EngineContext.Current.Resolve<IEventPublisher>();
        WorkflowServiceToServiceEvent evt = new(eventData);
        await eventPublisher.WorkflowServiceToServiceEventUpdate(evt);
    }

    private static async Task _LogEvent(O24OpenAPIEvent<LogEventData> eventData)
    {
        IEventPublisher eventPublisher = EngineContext.Current.Resolve<IEventPublisher>();
        LogEvent evt = new(eventData);
        await eventPublisher.LogEventUpdate(evt);
    }

    private static async Task _EntityEvent(O24OpenAPIEvent<EntityEvent> e)
    {
        IEventPublisher eventPublisher = EngineContext.Current.Resolve<IEventPublisher>();
        await eventPublisher.EntityEventUpdate(e.EventData.data);
    }

    private static async Task LogWorkflowStepEvent(O24OpenAPIEvent<StepExecutionEvent> eventData)
    {
        IEventPublisher eventPublisher = EngineContext.Current.Resolve<IEventPublisher>();
        await eventPublisher.LogWorkflowStepUpdate(eventData.EventData.data);
    }
}
