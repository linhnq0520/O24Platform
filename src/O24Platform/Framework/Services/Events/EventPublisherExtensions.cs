using System.Text.Json;
using O24OpenAPI.Client.Events.EventData;
using O24OpenAPI.Core.Configuration;
using O24OpenAPI.Core.Domain.Logging;
using O24OpenAPI.Core.Events;
using O24OpenAPI.Core.Infrastructure;
using O24OpenAPI.Framework.Services.Logging;

namespace O24OpenAPI.Framework.Services.Events;

/// <summary>
/// The event publisher extensions class
/// </summary>
public static class EventPublisherExtensions
{
    /// <summary>
    /// Workflows the event update using the specified event publisher
    /// </summary>
    /// <param name="eventPublisher">The event publisher</param>
    /// <param name="evt">The evt</param>
    public static async Task WorkflowEventUpdate(
        this IEventPublisher eventPublisher,
        WorkflowEvent evt
    )
    {
        ILogger logger = EngineContext.Current.Resolve<ILogger>();
        AppSettings appSettings = EngineContext.Current.Resolve<AppSettings>();
        if (logger != null && appSettings.Get<O24OpenAPIConfiguration>().LogEventMessage)
        {
            await logger.Insert(
                LogLevel.Information,
                $"Receive {evt.EventName} from portal - {evt.WorkflowId} - {evt.ExecutionId}",
                JsonSerializer.Serialize(evt.EventData) ?? ""
            );
        }
        await eventPublisher.Publish(evt);
    }

    // public static async Task WorkflowFinishEventUpdate(
    //     this IEventPublisher eventPublisher,
    //     WorkflowFinishEvent evt
    // )
    // {
    //     await eventPublisher.Publish(evt);
    // }

    /// <summary>
    /// Workflows the service to service event update using the specified event publisher
    /// </summary>
    /// <param name="eventPublisher">The event publisher</param>
    /// <param name="evt">The evt</param>
    public static async Task WorkflowServiceToServiceEventUpdate(
        this IEventPublisher eventPublisher,
        WorkflowServiceToServiceEvent evt
    )
    {
        ILogger logger = EngineContext.Current.Resolve<ILogger>();
        AppSettings appSettings = EngineContext.Current.Resolve<AppSettings>();
        if (logger != null && appSettings.Get<O24OpenAPIConfiguration>().LogEventMessage)
        {
            await logger.Insert(
                LogLevel.Information,
                $"Receive {evt.EventName} from portal - {evt.FromService} - {evt.ToService}",
                evt.TextData ?? ""
            );
        }
        await eventPublisher.Publish(evt);
    }

    /// <summary>
    /// Logs the event update using the specified event publisher
    /// </summary>
    /// <param name="eventPublisher">The event publisher</param>
    /// <param name="evt">The evt</param>
    public static async Task LogEventUpdate(this IEventPublisher eventPublisher, LogEvent evt)
    {
        if (Singleton<O24OpenAPIConfiguration>.Instance.LogEventMessage)
        {
            ILogger logger = EngineContext.Current.Resolve<ILogger>();
            if (logger != null)
            {
                await logger.Insert(
                    LogLevel.Information,
                    $"Receive {evt.EventName} from portal - {evt.FromService} - {evt.ToService}",
                    evt.TextData ?? ""
                );
            }
        }
        await eventPublisher.Publish(evt);
    }

    /// <summary>
    /// Entities the event update using the specified event publisher
    /// </summary>
    /// <param name="eventPublisher">The event publisher</param>
    /// <param name="evt">The evt</param>
    public static async Task EntityEventUpdate(this IEventPublisher eventPublisher, EntityEvent evt)
    {
        await eventPublisher.Publish(evt);
    }

    /// <summary>
    /// Cdcs the event update using the specified event publisher
    /// </summary>
    /// <param name="eventPublisher">The event publisher</param>
    /// <param name="evt">The evt</param>
    public static async Task CDCEventUpdate(this IEventPublisher eventPublisher, CDCEvent evt)
    {
        await eventPublisher.Publish(evt);
    }

    public static async Task LogWorkflowStepUpdate(
        this IEventPublisher eventPublisher,
        StepExecutionEvent evt
    )
    {
        await eventPublisher.Publish(evt);
    }

    public static async Task PublishWorkflowEvent(
        this IEventPublisher eventPublisher,
        WorkflowEvent evt
    )
    {
        await eventPublisher.Publish(evt);
    }
}
