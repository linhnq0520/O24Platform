namespace O24OpenAPI.Framework.Models;

/// <summary>
///
/// </summary>
public class AdvanceSearchParamModel : BaseTransactionModel
{
    /// <summary>
    ///
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    ///
    /// </summary>
    public int PageIndex { get; set; } = 0;

    /// <summary>
    ///
    /// </summary>
    public int PageSize { get; set; } = int.MaxValue;
}
