using System.Text.Json;
using System.Text.Json.Nodes;
using LinKit.Json.Runtime;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace O24OpenAPI.Core.Extensions;

public static class ObjectExtensions
{
    public static bool TryParse<T>(this object s, out T? result)
    {
        result = default;

        if (s == null)
        {
            return false;
        }

        string? str = s.ToString();
        if (string.IsNullOrWhiteSpace(str))
        {
            return false;
        }

        try
        {
            if (typeof(T) == typeof(JObject))
            {
                JObject jObject = JObject.Parse(str);
                result = jObject.ToObject<T>();
                return true;
            }
            else if (typeof(T) == typeof(JArray))
            {
                JArray jArray = JArray.Parse(str);
                result = jArray.ToObject<T>();
                return true;
            }
            else
            {
                result = str.FromJson<T>();
                return true;
            }
        }
        catch
        {
            return false;
        }
    }

    public static string ToSerializeSystemText(this object obj)
    {
        return System.Text.Json.JsonSerializer.Serialize(obj);
    }

    public static string ToSerialize(this object obj)
    {
        if (obj == null)
            return string.Empty;
        return JsonConvert.SerializeObject(obj);
    }

    public static bool ToBoolean(this object obj)
    {
        return obj switch
        {
            bool b => b,
            null => false,
            _ => bool.TryParse(obj.ToString(), out bool result) && result,
        };
    }

    public static int ToInt(this object obj)
    {
        try
        {
            return obj switch
            {
                int i => i,
                null => 0,
                string @string => int.Parse(@string),
                _ => int.TryParse(obj.ToString(), out int result) ? result : 0,
            };
        }
        catch
        {
            return 0;
        }
    }

    public static long ToLong(this object obj)
    {
        return obj switch
        {
            long l => l,
            null => 0,
            _ => long.TryParse(obj.ToString(), out long result) ? result : 0,
        };
    }

    public static double ToDouble(this object obj)
    {
        return obj switch
        {
            double d => d,
            null => 0,
            _ => double.TryParse(obj.ToString(), out double result) ? result : 0,
        };
    }

    public static decimal ToDecimal(this object obj)
    {
        return obj switch
        {
            decimal d => d,
            null => 0,
            _ => decimal.TryParse(obj.ToString(), out decimal result) ? result : 0,
        };
    }

    public static DateTime ToDateTime(this object obj)
    {
        return obj switch
        {
            DateTime dt => dt,
            null => default,
            _ => DateTime.TryParse(obj.ToString(), out DateTime result) ? result : default,
        };
    }

    public static string ToLowerCase(this object obj)
    {
        return obj?.ToString()?.ToLower() ?? string.Empty;
    }

    public static JObject ToJObject(this object obj)
    {
        return obj switch
        {
            null => [],
            string json => JObject.Parse(json),
            JObject jObj => jObj,
            _ => JObject.FromObject(obj),
        };
    }

    public static Dictionary<string, object> ToDictionary(this object model)
    {
        if (model is null)
        {
            return [];
        }

        if (model is Dictionary<string, object> existing)
        {
            return existing;
        }

        if (model is string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<Dictionary<string, object>>(json) ?? [];
            }
            catch
            {
                return new Dictionary<string, object> { { "value", json } };
            }
        }

        try
        {
            JsonSerializerSettings settings = new()
            {
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            };

            string jsonModel = JsonConvert.SerializeObject(model, settings);

            return JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonModel, settings)
                ?? [];
        }
        catch
        {
            return [];
        }
    }

    public static T? ToObject<T>(this object obj, JsonSerializerOptions? options = null)
    {
        if (obj is null)
        {
            return default;
        }

        if (obj is T t)
        {
            return t;
        }

        if (options is not null)
        {
            if (obj is string str)
            {
                return System.Text.Json.JsonSerializer.Deserialize<T>(str, options);
            }
            return System.Text.Json.JsonSerializer.Deserialize<T>(
                System.Text.Json.JsonSerializer.Serialize(obj, options),
                options
            );
        }
        if (obj is string jsonString)
        {
            return JConvert.FromJson<T>(jsonString);
        }
        return JConvert.FromJson<T>(obj.ToJson());
    }

    public static string WriteIndentedJson(
        this object? obj,
        JsonSerializerSettings? settings = null
    )
    {
        if (obj is null)
        {
            return string.Empty;
        }

        settings ??= new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        };

        return JsonConvert.SerializeObject(obj, settings);
    }

    public static JsonNode? ToJsonNode(this object obj)
    {
        if (obj is string json)
        {
            return JsonNode.Parse(json);
        }

        return System.Text.Json.JsonSerializer.SerializeToNode(obj);
    }

    public static List<string> ToListString<T>(this IEnumerable<T> list)
    {
        return [.. list.Select(item => item?.ToString() ?? string.Empty)];
    }
}
