namespace O24OpenAPI.Core.Helper;

/// <summary>
/// The 24 helper class
/// </summary>
public class O24Helper
{
    /// <summary>
    /// Gets the o 24 assembly name using the specified value
    /// </summary>
    /// <param name="value">The value</param>
    /// <returns>The string</returns>
    public static string GetO24AssemblyName(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        return value switch
        {
            string v when v.StartsWith("O24OpenAPI.Lib") => "O24OpenAPI",
            string v when v.StartsWith("O24OpenAPI.Client") => "O24OpenAPI.Client",
            string v when v.StartsWith("O24OpenAPI.Core") => "O24OpenAPI.Core",
            string v when v.StartsWith("O24OpenAPI.Data") => "O24OpenAPI.Data",
            _ => GetDefaultAssemblyName(value),
        };
    }

    /// <summary>
    /// Gets the default assembly name using the specified value
    /// </summary>
    /// <param name="value">The value</param>
    /// <returns>The string</returns>
    private static string GetDefaultAssemblyName(string value)
    {
        string[] parts = value.Split('.');
        int takeCount = value.Contains("Web") ? 3 : 2;

        return string.Join('.', parts.Take(takeCount));
    }
}
