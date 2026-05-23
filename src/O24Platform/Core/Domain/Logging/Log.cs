namespace O24OpenAPI.Core.Domain.Logging;

/// <summary>
/// The log class
/// </summary>
/// <seealso cref="BaseEntity"/>
public partial class Log : BaseEntity
{
    /// <summary>
    /// Gets or sets the value of the log level id
    /// </summary>
    public int LogLevelId { get; set; }

    /// <summary>
    /// Gets or sets the value of the short message
    /// </summary>
    public string? ShortMessage { get; set; }

    /// <summary>
    /// Gets or sets the value of the full message
    /// </summary>
    public string? FullMessage { get; set; }

    /// <summary>
    /// Gets or sets the value of the ip address
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// Gets or sets the value of the user id
    /// </summary>
    public int? UserId { get; set; }

    /// <summary>
    /// Gets or sets the value of the page url
    /// </summary>
    public string? PageUrl { get; set; }

    /// <summary>
    /// Gets or sets the value of the referred url
    /// </summary>
    public string? ReferredUrl { get; set; }

    /// <summary>
    /// Gets or sets the value of the log level
    /// </summary>
    public LogLevel LogLevel
    {
        get => (LogLevel)this.LogLevelId;
        set => this.LogLevelId = (int)value;
    }
}
