using System.Text.Json;
using O24OpenAPI.Core.Json;

namespace O24OpenAPI.Framework.Models.O24OpenAPI;

/// <summary>
/// The 24 utils class
/// </summary>
public class O24Utils
{
    /// <summary>
    /// The number converter
    /// </summary>
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        Converters = { new NumberConverter() },
    };

    /// <summary>
    /// Deserializes the workflow using the specified value
    /// </summary>
    /// <typeparam name="T">The </typeparam>
    /// <param name="value">The value</param>
    /// <param name="jsonSerializerOptions">The json serializer options</param>
    /// <exception cref="InvalidOperationException">Failed to deserialize workflow.</exception>
    /// <returns>A wf of t</returns>
    public static WF<T> DeserializeWorkflow<T>(
        string value,
        JsonSerializerOptions jsonSerializerOptions = null
    )
        where T : BaseTransactionModel
    {
        return JsonSerializer.Deserialize<WF<T>>(
                value,
                jsonSerializerOptions ?? _jsonSerializerOptions
            ) ?? throw new InvalidOperationException("Failed to deserialize workflow.");
    }
}
