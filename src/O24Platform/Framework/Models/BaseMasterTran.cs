namespace O24OpenAPI.Framework.Models;

public abstract class BaseMasterTran : BaseTrans
{
    public string CrossBranchCode { get; set; } = string.Empty;

    public string CrossCurrencyCode { get; set; } = string.Empty;

    public decimal BaseCurrencyAmount { get; set; } = 0;
}
