using O24OpenAPI.Core.Configuration;
using O24OpenAPI.Core.Infrastructure;

namespace O24OpenAPI.Core.Caching;

/// <summary>
/// The prefix key class
/// </summary>
public class PrefixKey
{
    /// <summary>
    /// Gets the value of the key
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PrefixKey"/> class
    /// </summary>
    /// <param name="prefix">The prefix</param>
    public PrefixKey(string prefix)
    {
        this.Key = Singleton<O24OpenAPIConfiguration>.Instance?.YourServiceID + "." + prefix;
    }
}
