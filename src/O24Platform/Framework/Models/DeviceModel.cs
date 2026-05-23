using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace O24OpenAPI.Framework.Models;

public class DeviceModel : BaseO24OpenAPIModel
{
    [JsonProperty("ip_address")]
    [JsonPropertyName("ip_address")]
    public string IpAddress { get; set; } = "::1";

    [JsonProperty("os_version")]
    [JsonPropertyName("os_version")]
    public string OsVersion { get; set; } = "unknown";

    [JsonProperty("user_agent")]
    [JsonPropertyName("user_agent")]
    public string UserAgent { get; set; } = string.Empty;

    [JsonProperty("device_id")]
    [JsonPropertyName("device_id")]
    public string DeviceId { get; set; } = Guid.NewGuid().ToString("N");

    [JsonProperty("device_type")]
    [JsonPropertyName("device_type")]
    public string DeviceType { get; set; } = "unknown";

    [JsonProperty("app_version")]
    [JsonPropertyName("app_version")]
    public string AppVersion { get; set; } = "1.0.0";

    [JsonProperty("device_name")]
    [JsonPropertyName("device_name")]
    public string DeviceName { get; set; } = "Generic Device";

    [JsonProperty("brand")]
    [JsonPropertyName("brand")]
    public string Brand { get; set; } = "Unknown";

    [JsonProperty("is_emulator")]
    [JsonPropertyName("is_emulator")]
    public bool IsEmulator { get; set; } = false;

    [JsonProperty("is_rooted_or_jailbroken")]
    [JsonPropertyName("is_rooted_or_jailbroken")]
    public bool IsRootedOrJailbroken { get; set; } = false;

    [JsonProperty("network")]
    [JsonPropertyName("network")]
    public string Network { get; set; } = string.Empty;

    [JsonProperty("memory")]
    [JsonPropertyName("memory")]
    public string Memory { get; set; } = string.Empty;
}
