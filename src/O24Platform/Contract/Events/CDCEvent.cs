namespace O24OpenAPI.Contracts.Events;

public class CDCEvent : IntegrationEvent
{
    /// <summary>
    /// Gets or sets the value of the table name
    /// </summary>
    public string table_name { get; set; } = default!;

    /// <summary>
    /// Gets or sets the value of the action
    /// </summary>
    public string action { get; set; } = default!;

    /// <summary>
    /// Gets or sets the value of the source
    /// </summary>
    public string source { get; set; } = default!;

    /// <summary>
    /// /// Gets or sets the value of the sql
    /// </summary>
    public string sql { get; set; } = default!;
}
