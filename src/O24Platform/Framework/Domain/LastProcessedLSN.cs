using O24OpenAPI.Core.Domain;

namespace O24OpenAPI.Framework.Domain;

/// <summary>
/// The last processed lsn class
/// </summary>
/// <seealso cref="BaseEntity"/>
public partial class LastProcessedLSN : BaseEntity
{
    /// <summary>
    /// /// Gets or sets the value of the table name
    /// </summary>
    public string TableName { get; set; } = default!;

    /// <summary>
    /// Gets or sets the value of the last lsn
    /// </summary>
    public byte[] LastLSN { get; set; } = default!;

    /// <summary>
    /// Gets or sets the value of the last processed time
    /// </summary>
    public DateTime LastProcessedTime { get; set; } = default!;
}
