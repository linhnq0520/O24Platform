using System.Text.Json.Nodes;

namespace O24OpenAPI.Framework.Services.Mapping;

public interface IDataMapper
{
    Task<Dictionary<string, object>> MapDataToDictionaryAsync(
        JsonNode source,
        JsonNode target,
        Func<string, Task<object>> func = null
    );
    Task<JsonNode> MapDataAsync(
        JsonNode source,
        JsonNode target,
        Func<string, Task<object>> func = null
    );
}
