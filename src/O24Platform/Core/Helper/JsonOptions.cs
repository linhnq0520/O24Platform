using System.Text.Json;
using System.Text.Json.Serialization;

namespace O24OpenAPI.Core.Helper;

public class JsonOptions
{
    /// <summary>
    /// The write indented
    /// </summary>
    public static readonly JsonSerializerOptions IgnoreCycles = new()
    {
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        WriteIndented = false,
    };

    public static readonly JsonSerializerOptions IgnoreCyclesAndWriteIndented = new()
    {
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        WriteIndented = true,
    };

    public static readonly JsonSerializerOptions Unwrapped = new()
    {
        Converters = { new SystemTextJsonConverter() },
        PropertyNamingPolicy = null,
        PropertyNameCaseInsensitive = false,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
    };
}
