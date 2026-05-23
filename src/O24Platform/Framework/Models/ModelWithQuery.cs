namespace O24OpenAPI.Framework.Models;

/// <summary>
/// The model with query class
/// </summary>
/// <seealso cref="BaseTransactionModel"/>
public class ModelWithQuery : BaseTransactionModel
{
    /// <summary>
    /// Gets or sets the value of the command name
    /// </summary>
    public string CommandName { get; set; }

    /// <summary>
    /// Gets or sets the value of the parameters
    /// </summary>
    public Dictionary<string, object> Parameters { get; set; }

    /// <summary>
    /// Gets or sets the value of the is search
    /// </summary>
    public bool IsSearch { get; set; } = false;

    /// <summary>
    /// Gets or sets the value of the page index
    /// /// </summary>
    public int? PageIndex { get; set; }

    /// <summary>
    /// Gets or sets the value of the page size
    /// </summary>
    public int? PageSize { get; set; }
}
