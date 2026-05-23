using System.Text.Json;

namespace O24OpenAPI.Client.Events.EventData;

/// <summary>
/// The service to service event data class
/// </summary>
public class ServiceToServiceEventData
{
    /// <summary>
    /// Gets or sets the value of the from service code
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

    /// <summary>
    /// Deserializes this instance
    /// </summary>
    /// <typeparam name="TSeserializedObject">The seserialized object</typeparam>
    /// <returns>The seserialized object</returns>
    public TSeserializedObject Deserialize<TSeserializedObject>()
    {
        return JsonSerializer.Deserialize<TSeserializedObject>(text_data);
    }
}
