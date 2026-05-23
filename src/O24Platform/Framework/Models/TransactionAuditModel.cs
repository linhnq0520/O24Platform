namespace O24OpenAPI.Framework.Models;

/// <summary>
/// The transaction audit model class
/// </summary>
/// <seealso cref="BaseO24OpenAPIModel"/>
public class TransactionAuditModel : BaseO24OpenAPIModel
{
    /// <summary>
    /// Gets or sets the value of the transaction code
    /// </summary>
    public string TransactionCode { get; set; }

    /// <summary>
    /// Gets or sets the value of the sub code
    /// </summary>
    public string SubCode { get; set; }

    /// <summary>
    /// Gets or sets the value of the transaction date time
    /// </summary>
    public DateTime TransactionDateTime { get; set; }

    /// <summary>
    /// Gets or sets the value of the transaction date
    /// </summary>
    public string TransactionDate { get; set; }

    /// <summary>
    /// Gets or sets the value of the transaction number
    /// </summary>
    public string TransactionNumber { get; set; }

    /// <summary>
    /// Gets or sets the value of the ref id
    /// </summary>
    public string RefId { get; set; }

    /// <summary>
    /// Gets or sets the value of the user code
    /// </summary>
    public string UserCode { get; set; }

    /// <summary>
    /// Gets or sets the value of the user name
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// Gets or sets the value of the status
    /// </summary>
    public string Status { get; set; }

    /// <summary>
    /// Gets or sets the value of the is reverse
    /// </summary>
    public bool IsReverse { get; set; }

    /// <summary>
    /// Gets or sets the value of the description
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the value of the details
    /// </summary>
    public List<TransactionDetailAuditModel> Details { get; set; }
}
