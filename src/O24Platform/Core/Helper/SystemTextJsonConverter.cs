using System.Text.Json;
using System.Text.Json.Serialization;

namespace O24OpenAPI.Core.Helper;

/// <summary>
/// The system text json converter class
/// </summary>
/// <seealso cref="JsonConverter{object}"/>
public class SystemTextJsonConverter : JsonConverter<object>
{
    /// <summary>
    /// Reads the reader
    /// </summary>
    /// <param name="reader">The reader</param>
    /// <param name="typeToConvert">The type to convert</param>
    /// <param name="options">The options</param>
    /// <returns>The object</returns>
    public override object Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
        {
            return ConvertJsonElementToObject(doc.RootElement);
        }
    }

    /// <summary>
    /// Writes the writer
    /// </summary>
    /// <param name="writer">The writer</param>
    /// <param name="value">The value</param>
    /// <param name="options">The options</param>
    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        var json = JsonSerializer.Serialize(value, options);
        writer.WriteRawValue(json);
    }

    /// <summary>
    /// Converts the json element to object using the specified element
    /// </summary>
    /// <param name="element">The element</param>
    /// <returns>The object</returns>
    private object ConvertJsonElementToObject(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Object => ConvertJsonObject(element),
            JsonValueKind.Array => ConvertJsonArray(element),
            _ => ConvertSimpleJsonValue(element),
        };
    }

    /// <summary>
    /// Converts the json object using the specified element
    /// </summary>
    /// <param name="element">The element</param>
    /// <returns>The dict</returns>
    private Dictionary<string, object> ConvertJsonObject(JsonElement element)
    {
        var dict = new Dictionary<string, object>();
        foreach (var prop in element.EnumerateObject())
        {
            dict[prop.Name] = ConvertJsonElementToObject(prop.Value);
        }
        return dict;
    }

    /// <summary>
    /// Converts the json array using the specified element
    /// </summary>
    /// <param name="element">The element</param>
    /// <returns>The list</returns>
    private List<object> ConvertJsonArray(JsonElement element)
    {
        var list = new List<object>();
        foreach (var item in element.EnumerateArray())
        {
            list.Add(ConvertJsonElementToObject(item));
        }
        return list;
    }

    /// <summary>
    /// Converts the simple json value using the specified element
    /// </summary>
    /// <param name="element">The element</param>
    /// <returns>The object</returns>
    private static object? ConvertSimpleJsonValue(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt64(out long l)
                ? l
                : RemoveTrailingZero(element.GetDouble()),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => element.ToString(),
        };
    }

    private static object RemoveTrailingZero(double number)
    {
        return number % 1 == 0 ? (object)(long)number : number; // Nếu là số nguyên, cast về long để bỏ .0
    }
}
