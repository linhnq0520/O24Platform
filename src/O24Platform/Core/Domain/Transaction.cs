namespace O24OpenAPI.Core.Domain;

/// <summary>
/// The transaction class
/// </summary>
/// <seealso cref="BaseEntity"/>
public partial class Transaction : BaseEntity
{
    /// <summary>
    /// Gets or sets the value of the transaction code
    /// </summary>
    public string? TransactionCode { get; set; }

    /// <summary>
    /// Gets or sets the value of the sub code
    /// </summary>
    public string? SubCode { get; set; }

    /// <summary>
    /// Gets or sets the value of the transaction date
    /// </summary>
    public DateTime TransactionDate { get; set; }

    /// <summary>
    /// Gets or sets the value of the value date
    /// </summary>
    public DateTime ValueDate { get; set; }

    /// <summary>
    /// Gets or sets the value of the service sys date
    /// </summary>
    public DateTime? ServiceSysDate { get; set; }

    /// <summary>
    /// Gets or sets the value of the reference id
    /// </summary>
    public string? ReferenceId { get; set; }

    /// <summary>
    /// Gets or sets the value of the reference code
    /// </summary>
    public string? ReferenceCode { get; set; }

    /// <summary>
    /// Gets or sets the value of the business code
    /// </summary>
    public string? BusinessCode { get; set; }

    /// <summary>
    /// Gets or sets the value of the transaction number
    /// </summary>
    public string? TransactionNumber { get; set; }

    /// <summary>
    /// Gets or sets the value of the trans id
    /// </summary>
    public string? TransId { get; set; }

    /// <summary>
    /// Gets or sets the value of the channel id
    /// </summary>
    public string? ChannelId { get; set; }

    /// <summary>
    /// Gets or sets the value of the ref id
    /// </summary>
    public string? RefId { get; set; }

    /// <summary>
    /// Gets or sets the value of the user code
    /// </summary>
    public string? UserCode { get; set; }

    /// <summary>
    /// Gets or sets the value of the user name
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// Gets or sets the value of the login name
    /// </summary>
    public string? LoginName { get; set; }

    /// <summary>
    /// Gets or sets the value of the branch code
    /// </summary>
    public string? BranchCode { get; set; }

    /// <summary>
    /// Gets or sets the value of the request body
    /// </summary>
    public string? RequestBody { get; set; }

    /// <summary>
    /// Gets or sets the value of the response body
    /// </summary>
    public string? ResponseBody { get; set; }

    /// <summary>
    /// Gets or sets the value of the status
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Gets or sets the value of the is reverse
    /// </summary>
    public bool IsReverse { get; set; }

    /// <summary>
    /// Gets or sets the value of the amount 1
    /// </summary>
    public Decimal Amount1 { get; set; }

    /// <summary>
    /// Gets or sets the value of the description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the value of the start time
    /// </summary>
    public long StartTime { get; set; }

    /// <summary>
    /// Gets or sets the value of the duration
    /// </summary>
    public long Duration { get; set; }
}
