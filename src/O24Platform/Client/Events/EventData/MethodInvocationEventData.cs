namespace O24OpenAPI.Client.Events.EventData;

/// <summary>
/// The method invocation event data class
/// </summary>
public class MethodInvocationEventData
{
    /// <summary>
    /// Gets or sets the value of the from service code
    /// </summary>
    public string from_service_code { get; set; } = "";

    /// <summary>
    /// Gets or sets the value of the to service code
    /// </summary>
    public string to_service_code { get; set; } = "";

    /// <summary>
    /// Gets or sets the value of the method name
    /// </summary>
    public string method_name { get; set; } = "";

    /// <summary>
    /// Gets or sets the value of the text data
    /// </summary>
    public object[]? text_data { get; set; }
}
