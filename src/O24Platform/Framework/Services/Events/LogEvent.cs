using O24OpenAPI.Client.Events;
using O24OpenAPI.Client.Events.EventData;

namespace O24OpenAPI.Framework.Services.Events;

/// <summary>
/// The log event class
/// </summary>
public class LogEvent
{
    /// <summary>
    /// Gets or sets the value of the log type
    /// </summary>
    public string LogType { get; set; }

    /// <summary>
    /// Gets or sets the value of the event name
    /// </summary>
    public string EventName { get; set; }

    /// <summary>
    /// Gets or sets the value of the event type
    /// </summary>
    public string EventType { get; set; }

    /// <summary>
    /// Gets or sets the value of the text data
    /// </summary>
    public string TextData { get; set; }

    /// <summary>
    /// Gets or sets the value of the from service
    /// </summary>
    public string FromService { get; set; }

    /// <summary>
    /// Gets or sets the value of the to service
    /// </summary>
    public string ToService { get; set; }

    /// <summary>
    /// Gets or sets the value of the event data
    /// </summary>
    public LogEventData EventData { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LogEvent"/> class
    /// </summary>
    /// <param name="e">The </param>
    public LogEvent(O24OpenAPIEvent<LogEventData> e)
    {
        EventName = e.EventTypeName;
        EventData = e.EventData.data;
        TextData = e.EventData.data.text_data;
        FromService = e.EventData.data.from_service_code;
        ToService = e.EventData.data.to_service_code;
        EventType = e.EventData.data.event_type;
        LogType = e.EventData.data.log_type;
    }
}
