namespace O24OpenAPI.Core.Domain.Localization;

/// <summary>
/// The language class
/// </summary>
/// <seealso cref="BaseEntity"/>
public partial class Language : BaseEntity
{
    /// <summary>
    /// Gets or sets the value of the name
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the value of the language culture
    /// </summary>
    public string? LanguageCulture { get; set; }

    /// <summary>
    /// Gets or sets the value of the unique seo code
    /// </summary>
    public string? UniqueSeoCode { get; set; }

    /// <summary>
    /// Gets or sets the value of the display order
    /// </summary>
    public int DisplayOrder { get; set; }
}
