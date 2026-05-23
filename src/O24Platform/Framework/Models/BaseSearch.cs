namespace O24OpenAPI.Framework.Models;

/// <summary>
/// The base search class
/// </summary>
/// <seealso cref="BaseTransactionModel"/>
public class BaseSearch : BaseTransactionModel
{
    /// <summary>Page size</summary>
    public int PageSize { get; set; } = int.MaxValue;

    /// <summary>Page index</summary>
    public int PageIndex { get; set; } = 0;
}
