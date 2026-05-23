namespace O24OpenAPI.Core.Utils;

public class ValidationUtils
{
    public static void CheckRequired(params (string name, object value)[] fields)
    {
        foreach ((string? name, object? value) in fields)
        {
            if (value == null || (value is string str && string.IsNullOrWhiteSpace(str)))
            {
                throw new ArgumentException($"[{name}] is required");
            }
            if (value is int @int && @int == 0)
            {
                throw new ArgumentException($"[{name}] must greater than 0");
            }
        }
    }
    public static void RangeValue<T>(
        string name,
        T value,
        IEnumerable<T> allowedValues)
    {
        if (allowedValues == null || !allowedValues.Any())
        {
            throw new ArgumentException($"[{name}] allowed values is empty");
        }

        if (!allowedValues.Contains(value))
        {
            throw new ArgumentException(
                $"[{name}] must be one of: {string.Join(", ", allowedValues)}");
        }
    }
}
