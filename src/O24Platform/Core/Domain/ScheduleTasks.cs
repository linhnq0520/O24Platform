using System.ComponentModel.DataAnnotations.Schema;

namespace O24OpenAPI.Core.Domain;

/// <summary>
/// The schedule task class
/// </summary>
/// <seealso cref="BaseEntity"/>
public partial class ScheduleTask : BaseEntity
{
    /// <summary>
    /// Gets or sets the value of the name
    /// </summary>
    public string Name { get; set; } = default!;

    public string CorrelationId { get; set; } = default!;

    /// <summary>
    /// Gets or sets the value of the seconds
    /// </summary>
    [Column]
    public int Seconds { get; set; }

    /// <summary>
    /// Gets or sets the value of the type
    /// </summary>
    public string Type { get; set; } = default!;

    /// <summary>
    /// Gets or sets the value of the last enabled utc
    /// </summary>
    public DateTime? LastEnabledUtc { get; set; }

    /// <summary>
    /// Gets or sets the value of the enabled
    /// </summary>
    [Column]
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the value of the stop on error
    /// </summary>
    public bool StopOnError { get; set; }

    /// <summary>
    /// Gets or sets the value of the last start utc
    /// </summary>
    public DateTime? LastStartUtc { get; set; }

    /// <summary>
    /// Gets or sets the value of the last end utc
    /// </summary>
    public DateTime? LastEndUtc { get; set; }

    /// <summary>
    /// Gets or sets the value of the last success utc
    /// </summary>
    public DateTime? LastSuccessUtc { get; set; }
}
