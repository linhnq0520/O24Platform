using System.Reflection;
using O24OpenAPI.Core.Attributes;
using O24OpenAPI.Core.Domain.Localization;

namespace O24OpenAPI.Core.Helper;

public static class ResourceReflectionHelper
{
    public static List<LocaleStringResource> Extract(Type resourceType)
    {
        ArgumentNullException.ThrowIfNull(resourceType);

        var result = new List<LocaleStringResource>();

        ExtractInternal(resourceType, result);

        return result;
    }

    private static void ExtractInternal(Type resourceType, List<LocaleStringResource> result)
    {
        var fields = resourceType.GetFields(
            BindingFlags.Public
                | BindingFlags.NonPublic
                | BindingFlags.Static
                | BindingFlags.FlattenHierarchy
        );

        foreach (var field in fields)
        {
            if (!field.IsLiteral || field.FieldType != typeof(string))
                continue;

            var resourceName = field.GetRawConstantValue()?.ToString();
            if (string.IsNullOrWhiteSpace(resourceName))
                continue;

            var localizedAttributes = field
                .GetCustomAttributes(typeof(LocalizedAttribute), false)
                .Cast<LocalizedAttribute>();

            foreach (var attr in localizedAttributes)
            {
                result.Add(
                    new LocaleStringResource
                    {
                        Language = attr.Lang,
                        ResourceName = resourceName,
                        ResourceValue = attr.Value,
                    }
                );
            }
        }

        var nestedTypes = resourceType.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic);

        foreach (var nested in nestedTypes)
        {
            ExtractInternal(nested, result);
        }
    }

    public static List<LocaleStringResource> ExtractField(Type resourceType, string fieldName)
    {
        ArgumentNullException.ThrowIfNull(resourceType);

        if (string.IsNullOrWhiteSpace(fieldName))
            throw new ArgumentException("Field name is required", nameof(fieldName));

        var field =
            resourceType.GetField(
                fieldName,
                BindingFlags.Public
                    | BindingFlags.NonPublic
                    | BindingFlags.Static
                    | BindingFlags.FlattenHierarchy
            )
            ?? throw new InvalidOperationException(
                $"Field '{fieldName}' not found in type '{resourceType.FullName}'"
            );
        if (!field.IsLiteral || field.FieldType != typeof(string))
            throw new InvalidOperationException($"Field '{fieldName}' must be const string");

        var resourceName = field.GetRawConstantValue()?.ToString();

        var localizedAttributes = field
            .GetCustomAttributes(typeof(LocalizedAttribute), false)
            .Cast<LocalizedAttribute>()
            .ToList();

        var result = new List<LocaleStringResource>();

        foreach (var attr in localizedAttributes)
        {
            result.Add(
                new LocaleStringResource
                {
                    Language = attr.Lang,
                    ResourceName = resourceName,
                    ResourceValue = attr.Value,
                }
            );
        }

        return result;
    }
}
