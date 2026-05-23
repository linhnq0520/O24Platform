namespace O24OpenAPI.Client.Events.EventData;

/// <summary>
/// The entity event class
/// </summary>
public class EntityEvent
{
    /// <summary>
    /// Gets or sets the value of the table name
    /// </summary>
    public string? table_name { get; set; }

    /// <summary>
    /// Gets or sets the value of the primary key
    /// </summary>
    public string? primary_key { get; set; }

    /// <summary>
    /// Gets or sets the value of the action type
    /// </summary>
    public string? action_type { get; set; }

    /// <summary>
    /// Gets or sets the value of the data
    /// </summary>
    public object? data { get; set; }
}
