namespace O24OpenAPI.Client.Events.EventData;

/// <summary>
/// The cdc event class
/// </summary>
public class CDCEvent
{
    /// <summary>
    /// Gets or sets the value of the table name
    /// </summary>
    public string? table_name { get; set; }

    /// <summary>
    /// Gets or sets the value of the action
    /// </summary>
    public string? action { get; set; }

    /// <summary>
    /// Gets or sets the value of the source
    /// </summary>
    public string? source { get; set; }

    /// <summary>
    /// Gets or sets the value of the sql
    /// </summary>
    public string? sql { get; set; }
}
