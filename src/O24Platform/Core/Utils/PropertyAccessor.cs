using System.Linq.Expressions;

namespace O24OpenAPI.Core.Utils;

/// <summary>
/// The property accessor class
/// </summary>
public class PropertyAccessor<T, TResult>
{
    /// <summary>
    /// Gets the property value using the specified obj
    /// </summary>
    /// <param name="obj">The obj</param>
    /// <param name="propertyName">The property name</param>
    /// <returns>The result</returns>
    public static TResult GetPropertyValue(T obj, string propertyName)
    {
        var getter = CreateGetter(propertyName);
        return getter(obj);
    }

    /// <summary>
    /// Creates the getter using the specified property name
    /// </summary>
    /// <param name="propertyName">The property name</param>
    /// <returns>A func of t and t result</returns>
    private static Func<T, TResult> CreateGetter(string propertyName)
    {
        var param = Expression.Parameter(typeof(T), "x");
        var property = Expression.Property(param, propertyName);
        var convert = Expression.Convert(property, typeof(TResult));
        var lambda = Expression.Lambda<Func<T, TResult>>(convert, param);
        return lambda.Compile();
    }
}
