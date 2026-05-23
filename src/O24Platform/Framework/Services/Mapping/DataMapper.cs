using LinKit.Json.Runtime;
using Microsoft.Extensions.Caching.Memory;
using O24OpenAPI.Core.Extensions;
using O24OpenAPI.Framework.Extensions;
using O24OpenAPI.Logging.Helpers;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace O24OpenAPI.Framework.Services.Mapping;

public class DataMapper : IDataMapper
{
    private readonly IMemoryCache _cache;
    private readonly MemoryCacheEntryOptions _cacheOptions;

    public record MappingContext(JsonNode DataSource, string JsonPath);

    // Handler dictionary chuyển thành instance để có thể truy cập instance methods
    private readonly Dictionary<string, Func<MappingContext, object>> _handlers;

    public DataMapper(IMemoryCache cache)
    {
        _cache = cache;
        _cacheOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromHours(24));

        // Khởi tạo handlers trong constructor
        _handlers = new Dictionary<string, Func<MappingContext, object>>
        {
            ["dataS"] = (GetValue<string>),
            ["dataSNull"] = (GetValue<string>),
            ["dataI"] = ctx => GetValue<int>(ctx),
            ["dataINull"] = ctx => GetValue<int?>(ctx),
            ["dataB"] = ctx => GetValue<bool>(ctx),
            ["dataBNull"] = ctx => GetValue<bool?>(ctx),
            ["dataN"] = ctx => GetValue<decimal>(ctx),
            ["dataNNull"] = ctx => GetValue<decimal?>(ctx),
            ["dataDt"] = ctx => GetValue<DateTime>(ctx),
            ["dataDtNull"] = ctx => GetValue<DateTime?>(ctx),
            ["dataD"] = ctx => GetValue<double>(ctx),
            ["dataDNull"] = ctx => GetValue<double?>(ctx),
            ["dataL"] = ctx => GetValue<long>(ctx),
            ["dataLNull"] = ctx => GetValue<long?>(ctx),
            ["dataO"] = (GetValue<JsonObject>),
            ["dataONull"] = (GetValue<JsonObject>),
            ["dataA"] = (GetValue<JsonArray>),
            ["dataANull"] = (GetValue<JsonArray>),
            ["dataUnixToDate"] = GetDateTimeFromUnix,
            ["dataDateToUnix"] = GetUnixFromDateTime,
            ["dataUnixToDateLocal"] = GetDateTimeLocalFromUnix,
            ["dataDateToUnixLocal"] = GetUnixLocalFromDateTime,
            ["dataUnixMilToDateLocal"] = GetDateTimeLocalFromUnixMil,
            ["dataFormatDate"] = GetFormatCustomDate,
            ["dataSet"] = GetDataSet, // Bây giờ là instance method
        };
    }

    #region Public APIs

    public async Task<JsonNode> MapDataAsync(
        JsonNode source,
        JsonNode target,
        Func<string, Task<object>> func = null
    )
    {
        if (target is JsonObject obj)
            return await MapObjectInternal(source, obj, func);
        if (target is JsonArray arr)
            return await MapArrayInternal(source, arr, func);
        return target;
    }

    public async Task<Dictionary<string, object>> MapDataToDictionaryAsync(
        JsonNode source,
        JsonNode target,
        Func<string, Task<object>> func = null
    )
    {
        var mappedNode = await MapDataAsync(source, target.DeepClone(), func);
        return ConvertJsonNodeToDictionary(mappedNode) as Dictionary<string, object> ?? new();
    }

    #endregion

    #region Core Engine

    private async Task<JsonObject> MapObjectInternal(
        JsonNode source,
        JsonObject target,
        Func<string, Task<object>>? func
    )
    {
        foreach (var property in target.ToList())
        {
            if (property.Key == "condition" || property.Value == null)
                continue;

            if (
                property.Value is JsonValue value
                && value.TryGetValue<string>(out var configMapping)
            )
            {
                var trimmedConfig = configMapping.Trim();
                if (string.IsNullOrEmpty(trimmedConfig))
                    continue;

                if (func != null && trimmedConfig.StartsWith("dataFunc"))
                {
                    try
                    {
                        var (path, renameMap) = ExtractDataFuncConfig(trimmedConfig);
                        var data = await func(path);

                        JsonNode? node = JsonSerializer.SerializeToNode(data);

                        if (renameMap?.Count > 0)
                        {
                            node = RenameKeys(node, renameMap);
                        }

                        target[property.Key] = node;
                    }
                    catch (Exception ex)
                    {
                        BusinessLogHelper.Error(
                            ex,
                            "Mapping dataFunc failed. Property={Property}, Config={Config}",
                            property.Key,
                            trimmedConfig
                        );
                        throw;
                    }
                }
                else
                {
                    object mappedValue;
                    try
                    {
                        mappedValue = await MapValueAsync(source, trimmedConfig);
                    }
                    catch (Exception ex)
                    {
                        BusinessLogHelper.Error(
                            ex,
                            "Mapping value failed. Property={Property}, Config={Config}",
                            property.Key,
                            trimmedConfig
                        );
                        throw;
                    }

                    target[property.Key] = mappedValue switch
                    {
                        JsonNode node => node.DeepClone(),
                        _ => JsonValue.Create(mappedValue),
                    };

                    if (IsMissingMappedValue(trimmedConfig, mappedValue))
                    {
                        BusinessLogHelper.Warning(
                            "Mapping value not found. Property={Property}, Config={Config}",
                            property.Key,
                            trimmedConfig
                        );
                    }
                }
            }
            else if (property.Value is JsonObject innerObj)
            {
                await MapObjectInternal(source, innerObj, func);
            }
            else if (property.Value is JsonArray innerArr)
            {
                target[property.Key] = await MapArrayInternal(source, innerArr, func);
            }
        }
        return target;
    }

    private async Task<JsonArray> MapArrayInternal(
        JsonNode source,
        JsonArray target,
        Func<string, Task<object>> func
    )
    {
        for (int i = 0; i < target.Count; i++)
        {
            if (target[i] is JsonObject obj)
                target[i] = await MapObjectInternal(source, obj, func);
            else if (target[i] is JsonArray arr)
                target[i] = await MapArrayInternal(source, arr, func);
        }
        return target;
    }

    private Task<object> MapValueAsync(JsonNode source, string configMapping)
    {
        var (function, path) = ParseConfig(configMapping);

        if (function != null && _handlers.TryGetValue(function, out var handler))
        {
            try
            {
                return Task.FromResult(handler(new MappingContext(source, path)));
            }
            catch (Exception ex)
            {
                BusinessLogHelper.Error(
                    ex,
                    "Mapping handler execution failed. Function={Function}, Path={Path}, Config={Config}",
                    function,
                    path,
                    configMapping
                );
                throw;
            }
        }

        if (!string.IsNullOrEmpty(function))
        {
            BusinessLogHelper.Warning(
                "Mapping function not supported. Function={Function}, Config={Config}",
                function,
                configMapping
            );
        }

        return Task.FromResult<object>(configMapping);
    }

    #endregion

    #region Parsing & Helpers

    private (string Function, string Path) ParseConfig(string config)
    {
        // Sử dụng IMemoryCache thay cho ConcurrentDictionary
        return _cache.GetOrCreate(
            config,
            entry =>
            {
                entry.SetOptions(_cacheOptions);

                int openParen = config.IndexOf('(');
                int closeParen = config.LastIndexOf(')');

                if (openParen > 0 && closeParen > openParen)
                {
                    return (config[..openParen].Trim(), config[(openParen + 1)..closeParen].Trim());
                }
                return (null, config);
            }
        );
    }

    private string ExtractPath(string config) => ParseConfig(config).Path;

    private static bool IsMissingMappedValue(string configMapping, object mappedValue)
    {
        var (function, _) = ParseConfigStatic(configMapping);
        if (string.IsNullOrWhiteSpace(function) || !function.StartsWith("data", StringComparison.Ordinal))
        {
            return false;
        }

        // dataXNull variants allow null by design.
        if (function.EndsWith("Null", StringComparison.Ordinal))
        {
            return false;
        }

        return mappedValue == null;
    }

    private static (string Function, string Path) ParseConfigStatic(string config)
    {
        int openParen = config.IndexOf('(');
        int closeParen = config.LastIndexOf(')');
        if (openParen > 0 && closeParen > openParen)
        {
            return (config[..openParen].Trim(), config[(openParen + 1)..closeParen].Trim());
        }

        return (null, config);
    }

    private static T GetValue<T>(MappingContext context)
    {
        return context.DataSource.TryGetValueByPath(context.JsonPath, out T value)
            ? value
            : default;
    }

    private static object ConvertJsonNodeToDictionary(JsonNode node)
    {
        return node switch
        {
            JsonObject obj => obj.ToDictionary(
                kvp => kvp.Key,
                kvp => ConvertJsonNodeToDictionary(kvp.Value)
            ),
            JsonArray array => array.Select(ConvertJsonNodeToDictionary).ToList(),
            JsonValue value => GetPrimitive(value),
            _ => null,
        };
    }

    private static object GetPrimitive(JsonValue value)
    {
        if (value.TryGetValue(out string s))
            return s;
        if (value.TryGetValue(out int i))
            return i;
        if (value.TryGetValue(out bool b))
            return b;
        if (value.TryGetValue(out long l))
            return l;
        if (value.TryGetValue(out decimal m))
            return m;
        if (value.TryGetValue(out double d))
            return d;
        if (value.TryGetValue(out DateTime dt))
            return dt;
        return value.ToString();
    }

    #endregion

    #region Specialized Handlers

    private static object GetDateTimeFromUnix(MappingContext context)
    {
        var val = GetValue<long>(context);
        return val > 0 ? DateTimeOffset.FromUnixTimeSeconds(val).DateTime : null;
    }

    private static object GetUnixFromDateTime(MappingContext context)
    {
        var val = GetValue<string>(context);
        return DateTime.TryParse(val, out var dt) ? new DateTimeOffset(dt).ToUnixTimeSeconds() : 0;
    }

    private static object GetDateTimeLocalFromUnix(MappingContext context)
    {
        var val = GetValue<long>(context);
        return val > 0 ? DateTimeOffset.FromUnixTimeSeconds(val).UtcDateTime.ToLocalTime() : null;
    }

    private static object GetUnixLocalFromDateTime(MappingContext context)
    {
        var val = GetValue<string>(context);
        return DateTime.TryParse(val, out var dt)
            ? new DateTimeOffset(dt.ToUniversalTime()).ToUnixTimeSeconds()
            : 0;
    }

    private static object GetDateTimeLocalFromUnixMil(MappingContext context)
    {
        var val = GetValue<long>(context);
        return val > 0
            ? DateTimeOffset
                .FromUnixTimeMilliseconds(val)
                .UtcDateTime.ToLocalTime()
                .ToString("dd-MM-yyyy")
            : null;
    }

    private static object GetFormatCustomDate(MappingContext context)
    {
        var token = GetValue<string>(context);
        if (string.IsNullOrEmpty(token))
            return null;

        string dataDate = token.Trim('"');
        if (dataDate.Contains('|'))
        {
            var parts = dataDate.Split('|');
            if (
                DateTime.TryParseExact(
                    parts[0],
                    parts[1],
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var dt
                )
            )
                return dt.ToString("dd/MM/yyyy");
        }
        else if (DateTime.TryParse(dataDate, out var dt))
        {
            return dt.ToString("dd/MM/yyyy");
        }
        return dataDate;
    }

    private object GetDataSet(MappingContext context)
    {
        var result = new List<object>();
        if (context.JsonPath.ToJsonNode() is not JsonObject configMappingItem)
            return result;

        var firstMappingValue = configMappingItem.First().Value?.ToString() ?? string.Empty;
        var (func, pathTemplate) = ParseConfig(firstMappingValue);

        string arrayPath = pathTemplate.Split('[')[0];
        var sourceArray = GetValue<JsonArray>(new MappingContext(context.DataSource, arrayPath));
        if (sourceArray == null)
            return result;

        for (int i = 0; i < sourceArray.Count; i++)
        {
            var row = new Dictionary<string, object>();
            foreach (var kvp in configMappingItem)
            {
                string template = kvp.Value?.ToString() ?? string.Empty;
                string actualPath = string.Format(template, i);
                row[kvp.Key] = MapValueAsync(context.DataSource, actualPath)
                    .GetAwaiter()
                    .GetResult();
            }
            result.Add(row);
        }
        return result;
    }

    private (string Path, Dictionary<string, string>? RenameMap) ExtractDataFuncConfig(string config)
    {
        string raw = ExtractPath(config);

        if (!raw.Contains('|'))
            return (raw, null);

        var parts = raw.Split('|', 2);

        var renameMap = parts[1]
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Split(':', 2))
            .Where(x => x.Length == 2)
            .ToDictionary(x => x[0].Trim(), x => x[1].Trim());

        return (parts[0].Trim(), renameMap);
    }

    private static JsonNode? RenameKeys(JsonNode? node, Dictionary<string, string> map)
    {
        if (node is JsonArray arr)
        {
            var newArr = new JsonArray();

            foreach (var item in arr)
                newArr.Add(RenameKeys(item, map));

            return newArr;
        }

        if (node is JsonObject obj)
        {
            var newObj = new JsonObject();

            foreach (var kvp in obj)
            {
                var newKey = map.TryGetValue(kvp.Key, out var renamed)
                    ? renamed
                    : kvp.Key;

                newObj[newKey] = RenameKeys(kvp.Value, map);
            }

            return newObj;
        }

        return node?.DeepClone();
    }

    #endregion
}
