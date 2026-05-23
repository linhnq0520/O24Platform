using Newtonsoft.Json.Linq;

namespace O24OpenAPI.Core.Extensions;

/// <summary>
/// The object extensions class
/// </summary>
public static class JObjectExtensions
{
    /// <summary>
    /// Merges the with replace array using the specified target
    /// </summary>
    /// <param name="target">The target</param>
    /// <param name="source">The source</param>
    /// <returns>The target</returns>
    public static JObject MergeWithReplaceArray(this JObject target, JObject source)
    {
        target.Merge(
            source,
            new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Replace }
        );
        return target;
    }

    /// <summary>
    /// Merges the with concat array using the specified target
    /// </summary>
    /// <param name="target">The target</param>
    /// <param name="source">The source</param>
    /// <returns>The target</returns>
    public static JObject MergeWithConcatArray(this JObject target, JObject source)
    {
        target.Merge(
            source,
            new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Concat }
        );
        return target;
    }

    /// <summary>
    /// Merges the with union array using the specified target
    /// </summary>
    /// <param name="target">The target</param>
    /// <param name="source">The source</param>
    /// <returns>The target</returns>
    public static JObject MergeWithUnionArray(this JObject target, JObject source)
    {
        target.Merge(
            source,
            new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Union }
        );
        return target;
    }

    /// <summary>
    /// Merges the with merge array using the specified target
    /// </summary>
    /// <param name="target">The target</param>
    /// <param name="source">The source</param>
    /// <returns>The target</returns>
    public static JObject MergeWithMergeArray(this JObject target, JObject source)
    {
        target.Merge(
            source,
            new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Merge }
        );
        return target;
    }
}
