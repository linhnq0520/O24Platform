using System.Dynamic;
using LinKit.Core.Abstractions;
using Newtonsoft.Json.Linq;

namespace O24OpenAPI.Framework.Services.Mapping;

public partial interface IDataMappingService
{
    /// <summary>
    /// Maps the data using the specified source
    /// </summary>
    /// <param name="source">The source</param>
    /// <param name="target">The target</param>
    /// <param name="func">The func</param>
    /// <returns>A task containing the object</returns>
    Task<JObject> MapDataAsync(
        JObject source,
        JObject target,
        Func<string, Task<object>> func = null
    );

    /// <summary>
    /// Maps the data to dictionary using the specified source
    /// </summary>
    /// <param name="source">The source</param>
    /// <param name="target">The target</param>
    /// <param name="func">The func</param>
    /// <returns>A task containing a dictionary of string and object</returns>
    Task<Dictionary<string, object>> MapDataToDictionaryAsync(
        JObject source,
        JObject target,
        Func<string, Task<object>> func = null
    );
}

/// <summary>
/// The data mapping service class
/// </summary>
/// <seealso cref="IDataMappingService"/>
[RegisterService(Lifetime.Scoped)]
public partial class DataMappingService : IDataMappingService
{
    /// <summary>
    /// The mapping context
    /// </summary>
    public record MappingContext(JObject DataSource, string JsonPath);

    /// <summary>
    /// The get unix local from date time
    /// </summary>
    private static readonly Dictionary<string, Delegate> _handlers = new()
    {
        ["dataS"] = (GetValue<string>),
        ["dataSNull"] = (GetValueNullable<string>),
        ["dataI"] = (GetValue<int>),
        ["dataINull"] = (GetValueNullable<int>),
        ["dataB"] = (GetValue<bool>),
        ["dataBNull"] = (GetValueNullable<bool>),
        ["dataN"] = (GetValue<decimal>),
        ["dataNNull"] = (GetValueNullable<decimal>),
        ["dataDt"] = (GetValue<DateTime>),
        ["dataDtNull"] = (GetValueNullable<DateTime>),
        ["dataD"] = (GetValue<double>),
        ["dataDNull"] = (GetValueNullable<double>),
        ["dataL"] = (GetValue<long>),
        ["dataLNull"] = (GetValueNullable<long>),
        ["dataO"] = (GetValue<ExpandoObject>),
        ["dataONull"] = (GetValueNullable<ExpandoObject>),
        ["dataUnixToDate"] = GetDateTimeFromUnix,
        ["dataDateToUnix"] = GetUnixFromDateTime,
        ["dataUnixToDateLocal"] = GetDateTimeLocalFromUnix,
        ["dataDateToUnixLocal"] = GetUnixLocalFromDateTime,
    };

    /// <summary>
    /// Maps the data using the specified source
    /// </summary>
    /// <param name="source">The source</param>
    /// <param name="target">The target</param>
    /// <param name="func">The func</param>
    /// <returns>The target</returns>
    public async Task<JObject> MapDataAsync(
        JObject source,
        JObject target,
        Func<string, Task<object>> func = null
    )
    {
        try
        {
            foreach (var property in target)
            {
                if (property.Key == "condition" || property.Value == null)
                {
                    continue;
                }

                string configMapping =
                    property.Value.Type == JTokenType.String
                        ? property.Value.ToString().Trim()
                        : null;

                if (
                    !string.IsNullOrEmpty(configMapping)
                    && func != null
                    && configMapping.StartsWith("dataFunc")
                )
                {
                    string jsonPath = ExtractJsonPath(configMapping);
                    target[property.Key] = JToken.FromObject(await func(jsonPath));
                    continue;
                }

                if (!string.IsNullOrEmpty(configMapping))
                {
                    target[property.Key] = JToken.FromObject(await Map(source, configMapping));
                    continue;
                }

                switch (property.Value.Type)
                {
                    case JTokenType.Array:
                        target[property.Key] = await MapArrayDataAsync(
                            source,
                            (JArray)property.Value
                        );
                        break;
                    case JTokenType.Object:
                        target[property.Key] = await MapDataAsync(
                            source,
                            (JObject)property.Value,
                            func
                        );
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in MapDataAsync: {ex.Message}");
        }
        return target;
    }

    /// <summary>
    /// Maps the data to dictionary using the specified source
    /// </summary>
    /// <param name="source">The source</param>
    /// <param name="target">The target</param>
    /// <param name="func">The func</param>
    /// <returns>The result</returns>
    public async Task<Dictionary<string, object>> MapDataToDictionaryAsync(
        JObject source,
        JObject target,
        Func<string, Task<object>> func = null
    )
    {
        var result = new Dictionary<string, object>();
        try
        {
            foreach (var property in target)
            {
                if (property.Key == "condition" || property.Value == null)
                {
                    continue;
                }

                string configMapping =
                    property.Value.Type == JTokenType.String
                        ? property.Value.ToString().Trim()
                        : null;

                if (
                    !string.IsNullOrEmpty(configMapping)
                    && func != null
                    && configMapping.StartsWith("dataFunc")
                )
                {
                    string jsonPath = ExtractJsonPath(configMapping);
                    result[property.Key] = await func(jsonPath);
                    continue;
                }

                if (!string.IsNullOrEmpty(configMapping))
                {
                    try
                    {
                        var x = await Map(source, configMapping);
                        result[property.Key] = x;
                        continue;
                    }
                    catch (Exception) { }
                }

                switch (property.Value.Type)
                {
                    case JTokenType.Array:
                        result[property.Key] = await MapArrayDataAsync(
                            source,
                            (JArray)property.Value
                        );
                        break;
                    case JTokenType.Object:
                        result[property.Key] = await MapDataAsync(
                            source,
                            (JObject)property.Value,
                            func
                        );
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in MapDataToDictionaryAsync: {ex.Message}");
        }
        return result;
    }

    /// <summary>
    /// Extracts the json path using the specified config mapping
    /// </summary>
    /// <param name="configMapping">The config mapping</param>
    /// <returns>The string</returns>
    private static string ExtractJsonPath(string configMapping)
    {
        int startIndex = configMapping.IndexOf('(') + 1;
        int endIndex = configMapping.IndexOf(')');
        return startIndex > 0 && endIndex > startIndex
            ? configMapping.Substring(startIndex, endIndex - startIndex).Trim()
            : string.Empty;
    }

    /// <summary>
    /// Maps the source
    /// </summary>
    /// <param name="source">The source</param>
    /// <param name="configMapping">The config mapping</param>
    /// <returns>A task containing the object</returns>
    public static async Task<object> Map(JObject source, string configMapping)
    {
        if (string.IsNullOrWhiteSpace(configMapping))
        {
            return null;
        }

        var lastIndexOfFunc = configMapping.IndexOf('(');
        if (lastIndexOfFunc >= 0)
        {
            string function = configMapping[..lastIndexOfFunc].Trim();
            string jsonPath = ExtractJsonPath(configMapping);

            if (_handlers.TryGetValue(function, out var handler))
            {
                var context = new MappingContext(source, jsonPath);

                if (handler is Func<MappingContext, object> syncHandler)
                {
                    return syncHandler(context);
                }

                if (handler is Func<MappingContext, Task<object>> asyncHandler)
                {
                    return await asyncHandler(context);
                }
            }

            Console.WriteLine($"Unknown function: {function}");
            return null;
        }
        return configMapping;
    }

    /// <summary>
    /// Maps the array data using the specified source
    /// </summary>
    /// <param name="source">The source</param>
    /// <param name="target">The target</param>
    /// <returns>The target</returns>
    private async Task<JArray> MapArrayDataAsync(JObject source, JArray target)
    {
        var tasks = target.Select(
            async (item, i) =>
            {
                target[i] = item switch
                {
                    JObject obj => await MapDataAsync(source, obj),
                    JArray array => await MapArrayDataAsync(source, array),
                    _ => item,
                };
            }
        );

        await Task.WhenAll(tasks);
        return target;
    }

    /// <summary>
    /// Gets the value using the specified context
    /// </summary>
    /// <typeparam name="T">The </typeparam>
    /// <param name="context">The context</param>
    /// <returns>The object</returns>
    private static object GetValue<T>(MappingContext context)
    {
        if (
            context.DataSource.TryGetValue(context.JsonPath, out JToken token)
            && token.Type != JTokenType.Null
        )
        {
            return token.ToObject<T>();
        }
        return default(T);
    }

    /// <summary>
    /// Gets the value nullable using the specified context
    /// </summary>
    /// <typeparam name="T">The </typeparam>
    /// <param name="context">The context</param>
    /// <returns>The object</returns>
    private static object GetValueNullable<T>(MappingContext context)
    {
        if (
            context.DataSource.TryGetValue(context.JsonPath, out JToken token)
            && !string.IsNullOrWhiteSpace(token?.ToString())
        )
        {
            return token.ToObject<T>();
        }
        return null;
    }

    /// <summary>
    /// Gets the date time from unix using the specified context
    /// </summary>
    /// <param name="context">The context</param>
    /// <returns>The object</returns>
    private static object GetDateTimeFromUnix(MappingContext context)
    {
        var token = context.DataSource.SelectToken(context.JsonPath);
        return token != null && long.TryParse(token.ToString(), out long unixTimestamp)
            ? DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).DateTime
            : DateTime.MinValue;
    }

    /// <summary>
    /// Gets the unix from date time using the specified context
    /// </summary>
    /// <param name="context">The context</param>
    /// <returns>The object</returns>
    private static object GetUnixFromDateTime(MappingContext context)
    {
        var token = context.DataSource.SelectToken(context.JsonPath);
        return token != null && DateTime.TryParse(token.ToString(), out DateTime dateTime)
            ? new DateTimeOffset(dateTime).ToUnixTimeSeconds()
            : 0;
    }

    /// <summary>
    /// Gets the date time local from unix using the specified context
    /// </summary>
    /// <param name="context">The context</param>
    /// <returns>The object</returns>
    private static object GetDateTimeLocalFromUnix(MappingContext context)
    {
        var token = context.DataSource.SelectToken(context.JsonPath);
        return token != null && long.TryParse(token.ToString(), out long unixTimestamp)
            ? DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).UtcDateTime.ToLocalTime()
            : DateTime.MinValue;
    }

    /// <summary>
    /// Gets the unix local from date time using the specified context
    /// </summary>
    /// <param name="context">The context</param>
    /// <returns>The object</returns>
    private static object GetUnixLocalFromDateTime(MappingContext context)
    {
        var token = context.DataSource.SelectToken(context.JsonPath);
        return token != null && DateTime.TryParse(token.ToString(), out DateTime localDateTime)
            ? new DateTimeOffset(localDateTime.ToUniversalTime()).ToUnixTimeSeconds()
            : 0;
    }
}
