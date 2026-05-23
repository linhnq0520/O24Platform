namespace O24OpenAPI.Core.Extensions;

/// <summary>
/// The list extensions class
/// </summary>
public static class ListExtensions
{
    /// <summary>
    /// Has the value using the specified list
    /// </summary>
    /// <typeparam name="T">The </typeparam>
    /// <param name="list">The list</param>
    /// <returns>The bool</returns>
    public static bool HasValue<T>(this IList<T> list)
    {
        return list.Count > 0;
    }
}
