using Humanizer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace O24OpenAPI.Framework.Extensions;

/// <summary>
/// The snake case converter class
/// </summary>
public static class SnakeCaseConverter
{
    /// <summary>
    /// Converts the keys to snake case using the specified token
    /// </summary>
    /// <param name="token">The token</param>
    /// <returns>The token</returns>
    public static JToken ConvertKeysToSnakeCase(this JToken token)
    {
        if (token == null)
        {
            return token;
        }
        if (token.Type == JTokenType.Object)
        {
            JObject jObject = [];
            foreach (JProperty item in token.Children<JProperty>())
            {
                jObject[item.Name.Underscore()] = item.Value;
            }
            return jObject;
        }
        if (token.Type == JTokenType.Array)
        {
            JArray jArray = [];
            foreach (JToken item2 in token.Children())
            {
                jArray.Add(ConvertKeysToSnakeCase(item2));
            }
            return jArray;
        }
        return token;
    }

    /// <summary>
    /// Returns the dictionary using the specified obj
    /// </summary>
    /// <typeparam name="TValue">The value</typeparam>
    /// <param name="obj">The obj</param>
    /// <returns>A dictionary of string and t value</returns>
    public static Dictionary<string, TValue> ToDictionary<TValue>(this object obj)
    {
        string value = JsonConvert.SerializeObject(obj);
        return JsonConvert.DeserializeObject<Dictionary<string, TValue>>(value);
    }
}
