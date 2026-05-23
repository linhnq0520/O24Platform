using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace O24OpenAPI.Core.Domain.ValueObjects;

public class LanguageValueObject
{
    [JsonProperty("en")]
    [JsonPropertyName("en")]
    public string English { get; set; } = string.Empty;

    [JsonProperty("vi")]
    [JsonPropertyName("vi")]
    public string Vietnamese { get; set; } = string.Empty;

    [JsonProperty("lo")]
    [JsonPropertyName("lo")]
    public string Lao { get; set; } = string.Empty;

    [JsonProperty("zh")]
    [JsonPropertyName("zh")]
    public string Chinese { get; set; } = string.Empty;
}
