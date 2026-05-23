using System.Text.Json;
using System.Text.Json.Nodes;

namespace O24OpenAPI.Core.Extensions;

/// <summary>
/// The dictionary extensions class
/// </summary>
public static class DictionaryExtensions
{
    /// <summary>
    /// Gets the string value using the specified dic
    /// </summary>
    /// <param name="dic">The dic</param>
    /// <param name="key">The key</param>
    /// <param name="defaultValue">The default value</param>
    /// <returns>The default value</returns>
    public static string GetStringValue(
        this Dictionary<string, object> dic,
        string key,
        string defaultValue = ""
    )
    {
        return dic.TryGetValue(key, out var value)
            ? value?.ToString() ?? defaultValue
            : defaultValue;
    }

    public static JsonNode? DictionaryToJsonNode(this Dictionary<string, object> dictionary)
    {
        if (dictionary == null)
        {
            return null;
        }

        JsonObject jsonObject = [];
        foreach (KeyValuePair<string, object> kvp in dictionary)
        {
            jsonObject[kvp.Key] = ConvertToJsonNode(kvp.Value);
        }
        return jsonObject;
    }

    private static JsonNode? ConvertToJsonNode(object value)
    {
        switch (value)
        {
            case null:
                return null;
            case string str:
                return JsonValue.Create(str);
            case bool b:
                return JsonValue.Create(b);
            case int i:
                return JsonValue.Create(i);
            case long l:
                return JsonValue.Create(l);
            case double d:
                return JsonValue.Create(d);
            case decimal dec:
                return JsonValue.Create(dec);
            case DateTime dt:
                return JsonValue.Create(dt);
            case Dictionary<string, object> dict:
                return DictionaryToJsonNode(dict);
            case IEnumerable<object> list:
                JsonArray jsonArray = [];
                foreach (object item in list)
                {
                    jsonArray.Add(ConvertToJsonNode(item));
                }
                return jsonArray;
            default:
                return JsonNode.Parse(JsonSerializer.Serialize(value));
        }
    }
}
