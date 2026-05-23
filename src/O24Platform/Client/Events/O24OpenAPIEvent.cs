using O24OpenAPI.Client.Lib;

namespace O24OpenAPI.Client.Events;

/// <summary>
/// The 24 open api event class
/// </summary>
public sealed class O24OpenAPIEvent<TO24OpenAPIEventData>
{
    /// <summary>
    /// Gets or sets the value of the event type
    /// </summary>
    public O24OpenAPIWorkflowEventTypeEnum EventType { get; set; }

    /// <summary>
    /// Gets the value of the event type name
    /// </summary>
    public string EventTypeName => EventType.ToString();

    /// <summary>
    /// Gets or sets the value of the event data
    /// </summary>
    public GenericEventData<TO24OpenAPIEventData> EventData { get; set; } =
        new GenericEventData<TO24OpenAPIEventData>();

    /// <summary>
    /// Gets the value of the raised on utc
    /// </summary>
    public long raised_on_utc { get; } = Utils.GetCurrentDateAsLongNumber();

    /// <summary>
    /// Initializes a new instance of the <see cref="O24OpenAPIEvent{TO24OpenAPIEventData}"/> class
    /// </summary>
    public O24OpenAPIEvent() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="O24OpenAPIEvent{TO24OpenAPIEventData}"/> class
    /// </summary>
    /// <param name="pEventType">The event type</param>
    public O24OpenAPIEvent(O24OpenAPIWorkflowEventTypeEnum pEventType)
    {
        EventType = pEventType;
    }
}
