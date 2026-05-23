namespace O24OpenAPI.Core.Configuration;

/// <summary>
/// The cache config class
/// </summary>
/// <seealso cref="IConfig"/>
public class CacheConfig : IConfig
{
    /// <summary>
    /// Gets or sets the value of the neptune cache time
    /// </summary>
    public int NeptuneCacheTime { get; set; } = 0;

    /// <summary>
    /// Gets or sets the value of the default cache time
    /// </summary>
    public int DefaultCacheTime { get; set; } = 60;

    /// <summary>
    /// Gets or sets the value of the short term cache time
    /// </summary>
    public int ShortTermCacheTime { get; set; } = 3;

    /// <summary>
    /// Gets or sets the value of the bundled files cache time
    /// </summary>
    public int BundledFilesCacheTime { get; set; } = 120;
}
