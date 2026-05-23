using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace O24OpenAPI.Core.Abstractions;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public abstract class BaseO24OpenAPIModel { }
