using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace O24OpenAPI.Framework.Models;

public class O24OpenAPIServiceSearchResponse : BaseO24OpenAPIModel
{
    [JsonProperty("id")]
    [JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the value of the step code
    /// /// </summary>
    [JsonProperty("step_code")]
    [JsonPropertyName("step_code")]
    public string StepCode { get; set; }

    /// <summary>
    /// Gets or sets the value of the full class name
    /// </summary>
    [JsonProperty("full_class_name")]
    [JsonPropertyName("full_class_name")]
    public string FullClassName { get; set; }

    /// <summary>
    /// Gets or sets the value of the method name
    /// </summary>
    [JsonProperty("method_name")]
    [JsonPropertyName("method_name")]
    public string MethodName { get; set; }

    /// <summary>
    /// Gets or sets the value of the should await
    /// </summary>
    [JsonProperty("should_await")]
    [JsonPropertyName("should_await")]
    public bool ShouldAwait { get; set; } = false;

    /// <summary>
    /// Gets or sets the value of the is inquiry
    /// </summary>
    [JsonProperty("is_inquiry")]
    [JsonPropertyName("is_inquiry")]
    public bool IsInquiry { get; set; } = false;
    public string MediatorKey { get; set; } = string.Empty;
    public bool IsModuleExecute { get; set; } = false;
    public bool? IsAutoReverse { get; set; } = true;
}
