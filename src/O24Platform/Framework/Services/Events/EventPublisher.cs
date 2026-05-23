using O24OpenAPI.Core.Events;
using O24OpenAPI.Core.Infrastructure;
using O24OpenAPI.Framework.Services.Logging;

namespace O24OpenAPI.Framework.Services.Events;

/// <summary>
/// The event publisher class
/// </summary>
/// <seealso cref="IEventPublisher"/>
public class EventPublisher : IEventPublisher
{
    /// <summary>
    /// Publishes the event
    /// </summary>
    /// <typeparam name="TEvent">The event</typeparam>
    /// <param name="@event">The event</param>
    public virtual async Task Publish<TEvent>(TEvent @event)
    {
        var consumers = EngineContext.Current.ResolveAll<IConsumer<TEvent>>().ToList();
        if (consumers.Count == 0)
        {
            return;
        }

        var logger = EngineContext.Current.Resolve<ILogger>();

        await Parallel.ForEachAsync(
            consumers,
            async (consumer, cancellationToken) =>
            {
                try
                {
                    await consumer.HandleEvent(@event);
                }
                catch (Exception ex)
                {
                    if (logger != null)
                    {
                        await logger.Error(
                            $"Error in consumer {consumer.GetType().Name}: {ex.Message}",
                            ex
                        );
                    }
                }
            }
        );
    }
}
