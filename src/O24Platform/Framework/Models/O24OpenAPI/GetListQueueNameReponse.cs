using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace O24OpenAPI.Framework.Models.O24OpenAPI;

public class GetQueueNameResponse : BaseO24OpenAPIModel
{
    [JsonProperty("full_class_name")]
    [JsonPropertyName("full_class_name")]
    public string FullClassName { get; set; } = string.Empty;
}
