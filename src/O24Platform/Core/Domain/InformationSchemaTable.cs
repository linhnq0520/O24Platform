namespace O24OpenAPI.Core.Domain;

/// <summary>
/// The information schema class
/// </summary>
public class INFORMATION_SCHEMA
{
    /// <summary>
    /// Gets or sets the value of the table catalog
    /// </summary>
    public string? TABLE_CATALOG { get; set; }

    /// <summary>
    /// Gets or sets the value of the table schema
    /// </summary>
    public string? TABLE_SCHEMA { get; set; }

    /// <summary>
    /// Gets or sets the value of the table name
    /// </summary>
    public string? TABLE_NAME { get; set; }

    /// <summary>
    /// Gets or sets the value of the table type
    /// </summary>
    public string? TABLE_TYPE { get; set; }
}
