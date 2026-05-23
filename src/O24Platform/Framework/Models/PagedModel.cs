namespace O24OpenAPI.Framework.Models;

/// <summary>
/// The paged model class
/// </summary>
/// <seealso cref="BaseO24OpenAPIModel"/>
public abstract class PagedModel : BaseO24OpenAPIModel
{
    /// <summary>
    /// Gets or sets the value of the page index
    /// </summary>
    public int PageIndex { get; set; }

    /// <summary>
    /// Gets or sets the value of the page size
    /// </summary>
    public int PageSize { get; set; }
}
