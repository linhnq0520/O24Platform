namespace O24OpenAPI.Core.Events;

/// <summary>
/// The event publisher interface
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// Publishes the event
    /// </summary>
    /// <typeparam name="TEvent">The event</typeparam>
    /// <param name="@event">The event</param>
    Task Publish<TEvent>(TEvent @event);
}
