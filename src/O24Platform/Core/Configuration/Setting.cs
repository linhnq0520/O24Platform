using O24OpenAPI.Core.Caching;

namespace O24OpenAPI.Core.Configuration;

/// <summary>
/// The setting class
/// </summary>
/// <seealso cref="BaseEntity"/>
public partial class Setting : BaseEntity
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Setting"/> class
    /// </summary>
    public Setting() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Setting"/> class
    /// </summary>
    /// <param name="name">The name</param>
    /// <param name="value">The value</param>
    /// <param name="organizationId">The organization id</param>
    public Setting(string name, string value, int organizationId = 0)
    {
        this.Name = name;
        this.Value = value;
        this.OrganizationId = organizationId;
    }

    /// <summary>
    /// Gets or sets the value of the name
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the value of the value
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// Gets or sets the value of the organization id
    /// </summary>
    public int OrganizationId { get; set; }

    /// <summary>
    /// Returns the string
    /// </summary>
    /// <returns>The string</returns>
    public override string? ToString() => this.Name;

    public static CacheKey GetCacheKeyAll() => CachingKey.EntityKeyWithService<Setting>("all");
}
