using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using O24OpenAPI.Core.Infrastructure;
using O24OpenAPI.Framework.Extensions;
using O24OpenAPI.Framework.Helpers;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace O24OpenAPI.Framework.Utils;

/// <summary>
/// The swagger filter utils class
/// </summary>
public static class SwaggerFilterUtils
{
    /// <summary>
    /// The operation filter attribute
    /// </summary>
    private static readonly Dictionary<Type, string> FilterTypeMappings = new()
    {
        { typeof(DocumentFilterAttribute), "DocumentFilter" },
        { typeof(SchemaFilterAttribute), "SchemaFilter" },
        { typeof(OperationFilterAttribute), "OperationFilter" },
    };

    /// <summary>
    /// Applies the all filters using the specified options
    /// </summary>
    /// <param name="options">The options</param>
    public static void ApplyAllFilters(SwaggerGenOptions options)
    {
        Singleton<ITypeFinder>
            .Instance.FindClassesOfType<ISwaggerFilter>()
            .ToList()
            .ForEach(type =>
            {
                var filterType = FilterTypeMappings
                    .FirstOrDefault(kv =>
                        type.GetCustomAttributes(false).Any(a => a.GetType() == kv.Key)
                    )
                    .Value;
                if (filterType != null)
                {
                    RegisterFilter(options, filterType, type);
                }
            });
    }

    /// <summary>
    /// Registers the filter using the specified options
    /// </summary>
    /// <param name="options">The options</param>
    /// <param name="filterType">The filter type</param>
    /// <param name="type">The type</param>
    private static void RegisterFilter(SwaggerGenOptions options, string filterType, Type type)
    {
        try
        {
            var methodInfo = typeof(SwaggerGenOptionsExtensions).GetMethod(
                filterType,
                BindingFlags.Public | BindingFlags.Static
            );
            if (methodInfo != null)
            {
                var genericMethod = methodInfo.MakeGenericMethod(type);
                genericMethod.Invoke(null, new object[] { options, Array.Empty<object>() });
                Console.WriteLine($"Successfully registered {filterType} for {type.FullName}");
            }
            else
            {
                Console.WriteLine(
                    $"Failed to find {filterType} method in SwaggerGenOptionsExtensions for {type.FullName}"
                );
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error registering {filterType} for {type.FullName}: {ex.Message}");
        }
    }
}
