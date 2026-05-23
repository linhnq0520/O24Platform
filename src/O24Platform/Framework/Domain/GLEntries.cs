using O24OpenAPI.Core.Domain;

namespace O24OpenAPI.Framework.Domain;

/// <summary>
/// The gl entries class
/// </summary>
/// <seealso cref="BaseEntity"/>
public partial class GLEntries : BaseEntity
{
    /// <summary>
    /// Gets or sets the value of the transaction number
    /// </summary>
    public string TransactionNumber { get; set; }

    /// <summary>
    /// Gets or sets the value of the trans table name
    /// </summary>
    public string TransTableName { get; set; }

    /// <summary>
    /// Gets or sets the value of the trans id
    /// </summary>
    public string TransId { get; set; }

    /// <summary>
    /// Gets or sets the value of the sys account name
    /// </summary>
    public string SysAccountName { get; set; }

    /// <summary>
    /// Gets or sets the value of the gl account
    /// </summary>
    public string GLAccount { get; set; }

    /// <summary>
    /// Gets or sets the value of the dor c
    /// </summary>
    public string DorC { get; set; }

    /// <summary>
    /// Gets or sets the value of the transaction status
    /// </summary>
    public string TransactionStatus { get; set; }

    /// <summary>
    /// Gets or sets the value of the amount
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the value of the branch code
    /// </summary>
    public string BranchCode { get; set; }

    /// <summary>
    /// Gets or sets the value of the currency code
    /// </summary>
    public string CurrencyCode { get; set; }

    /// <summary>
    /// Gets or sets the value of the value date
    /// </summary>
    public DateTime ValueDate { get; set; }

    /// <summary>
    /// Gets or sets the value of the posted
    /// </summary>
    public bool Posted { get; set; } = false;

    /// <summary>
    /// Gets or sets the value of the accounting group
    /// </summary>
    public int AccountingGroup { get; set; } = 1;

    /// <summary>
    /// Gets or sets the value of the cross branch code
    /// </summary>
    public string CrossBranchCode { get; set; }

    /// <summary>
    /// Gets or sets the value of the cross currency code
    /// </summary>
    public string CrossCurrencyCode { get; set; }

    /// <summary>
    /// Gets or sets the value of the base currency amount
    /// </summary>
    public decimal BaseCurrencyAmount { get; set; }
}
