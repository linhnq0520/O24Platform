using System.Text.Json;
using System.Text.Json.Serialization;

namespace O24OpenAPI.Core.Json;

public class DictionaryStringObjectJsonConverter : JsonConverter<Dictionary<string, object>>
{
    public override Dictionary<string, object> Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        return ConvertJsonObject(doc.RootElement);
    }

    public override void Write(
        Utf8JsonWriter writer,
        Dictionary<string, object> value,
        JsonSerializerOptions options
    )
    {
        JsonSerializer.Serialize(writer, value, typeof(object), options);
    }

    private Dictionary<string, object> ConvertJsonObject(JsonElement element)
    {
        var dict = new Dictionary<string, object>();
        foreach (var prop in element.EnumerateObject())
        {
            var @object = ConvertJsonElementToObject(prop.Value);
            if (@object != null)
            {
                dict[prop.Name] = @object;
            }
        }
        return dict;
    }

    private object? ConvertJsonElementToObject(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Object => ConvertJsonObject(element),
            JsonValueKind.Array => ConvertJsonArray(element),
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt64(out long l) ? l : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => null,
        };
    }

    private List<object> ConvertJsonArray(JsonElement element)
    {
        var list = new List<object>();
        foreach (var item in element.EnumerateArray())
        {
            var @object = ConvertJsonElementToObject(item);
            if (@object != null)
            {
                list.Add(@object);
            }
        }
        return list;
    }
}
