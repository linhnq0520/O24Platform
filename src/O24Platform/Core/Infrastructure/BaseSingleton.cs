using System.Collections.Concurrent;

namespace O24OpenAPI.Core.Infrastructure;

/// <summary>
/// The base singleton class
/// </summary>
public class BaseSingleton
{
    /// <summary>
    /// Gets the value of the all singletons
    /// </summary>
    public static ConcurrentDictionary<string, object> AllSingletons { get; } =
        new ConcurrentDictionary<string, object>();
}
