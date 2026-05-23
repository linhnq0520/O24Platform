using System.Text.Json.Serialization;

namespace O24OpenAPI.Client.Infisical;

/// <summary>
/// The infisical response class
/// </summary>
public class InfisicalResponse
{
    /// <summary>
    /// Gets or sets the value of the secrets
    /// </summary>
    [JsonPropertyName("secrets")]
    public List<InfisicalSecret> Secrets { get; set; } = [];
}
