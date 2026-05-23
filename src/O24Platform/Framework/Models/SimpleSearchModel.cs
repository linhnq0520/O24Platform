namespace O24OpenAPI.Framework.Models;

/// <summary>
/// The simple search model class
/// </summary>
/// <seealso cref="BaseTransactionModel"/>
public class SimpleSearchModel : BaseSearch
{
    /// <summary>Search query</summary>
    public string SearchText { get; set; } = "";
}
