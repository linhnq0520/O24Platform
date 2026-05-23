namespace O24OpenAPI.Framework.Models;

/// <summary>
/// The cdc data class
/// </summary>
public class CDCData
{
    /// <summary>
    /// Gets or sets the value of the table name
    /// /// </summary>
    public string TableName { get; set; }

    /// <summary>
    /// Gets or sets the value of the lsn
    /// </summary>
    public string LSN { get; set; }

    /// <summary>
    /// Gets or sets the value of the operation
    /// </summary>
    public int Operation { get; set; }

    /// <summary>
    /// Gets or sets the value of the data
    /// </summary>
    public string Data { get; set; }
}
