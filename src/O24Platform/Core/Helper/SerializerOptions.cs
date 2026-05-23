using System.Text.Json;

namespace O24OpenAPI.Core.Helper;

/// <summary>
/// The serializer options class
/// </summary>
public static class SerializerOptions
{
    /// <summary>
    /// The property name case insensitive
    /// </summary>
    public static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        Converters = { new SystemTextJsonConverter() },
        PropertyNamingPolicy = null,
        PropertyNameCaseInsensitive = false,
    };

    /// <summary>
    /// The write indented
    /// </summary>
    public static readonly JsonSerializerOptions SerializeOptions = new() { WriteIndented = false };

    /// <summary>
    /// The write indented
    /// </summary>
    public static readonly JsonSerializerOptions WriteIndentedOptions = new()
    {
        WriteIndented = true,
    };
}
