namespace O24OpenAPI.Framework.Models;

public class CodeListGroupAndNameRequestModel : BaseTransactionModel
{
    public string CodeGroup { get; set; }

    public string CodeName { get; set; }
    public int PageIndex { get; set; } = 0;
    public int PageSize { get; set; } = int.MaxValue;
}
