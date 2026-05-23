using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace O24OpenAPI.Core.Domain;

public class LocalizedString
{
    [JsonPropertyName("vi")]
    [JsonProperty("vi")]
    public string? Vi { get; set; }

    [JsonPropertyName("en")]
    [JsonProperty("en")]
    public string? En { get; set; }
}
