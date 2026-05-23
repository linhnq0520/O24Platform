namespace O24OpenAPI.Core.Domain.Localization;

/// <summary>
/// The locale string resource class
/// </summary>
/// <seealso cref="BaseEntity"/>
public partial class LocaleStringResource : BaseEntity
{
    /// <summary>
    /// Gets or sets the value of the language
    /// </summary>
    public string Language { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value of the resource name
    /// </summary>
    public string ResourceName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value of the resource value
    /// </summary>
    public string ResourceValue { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value of the resource value
    /// </summary>
    public string? ResourceCode { get; set; }
}
