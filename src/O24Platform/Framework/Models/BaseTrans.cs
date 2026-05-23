using O24OpenAPI.Core.Domain;

namespace O24OpenAPI.Framework.Models;

public abstract partial class BaseTrans : BaseEntity
{
    public string TransId { get; set; }

    public string TransactionNumber { get; set; }

    public string TransCode { get; set; }

    public string TransactionStatus { get; set; }

    public decimal Amount { get; set; }

    public bool GLPopulated { get; set; }

    public int GLGroup { get; set; } = 1;
}
