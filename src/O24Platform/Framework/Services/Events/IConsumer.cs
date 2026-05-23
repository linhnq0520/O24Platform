namespace O24OpenAPI.Framework.Services.Events;

/// <summary>Consumer interface</summary>
/// <typeparam name="T"></typeparam>
public interface IConsumer<T>
{
    /// <summary>Handle event</summary>
    /// <param name="eventMessage">Event</param>
    /// <returns></returns>
    Task HandleEvent(T eventMessage);
}
