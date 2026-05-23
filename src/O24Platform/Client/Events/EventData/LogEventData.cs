namespace O24OpenAPI.Client.Events.EventData;

/// <summary>
/// The log event data class
/// </summary>
public class LogEventData
{
    /// <summary>
    /// Gets or sets the value of the log type
    /// </summary>
    public string? log_type { get; set; }

    /// <summary>
    /// /// Gets or sets the value of the from service code
    /// </summary>
    public string? from_service_code { get; set; }

    /// <summary>
    /// Gets or sets the value of the to service code
    /// </summary>
    public string? to_service_code { get; set; }

    /// <summary>
    /// Gets or sets the value of the event type
    /// </summary>
    public string? event_type { get; set; }

    /// <summary>
    /// Gets or sets the value of the text data
    /// </summary>
    public string? text_data { get; set; }
}
