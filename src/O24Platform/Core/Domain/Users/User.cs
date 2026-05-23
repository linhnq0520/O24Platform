using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace O24OpenAPI.Core.Domain.Users;

/// <summary>
/// The user class
/// </summary>
/// <seealso cref="BaseEntity"/>
public partial class User : BaseEntity
{
    /// <summary>
    /// Gets or sets the value of the user code
    /// </summary>
    [JsonProperty("usercode")]
    [JsonPropertyName("usercode")]
    public string? UserCode { get; set; }

    /// <summary>
    /// Gets or sets the value of the login name
    /// </summary>
    [JsonProperty("loginname")]
    [JsonPropertyName("loginname")]
    public string? LoginName { get; set; }

    /// <summary>
    /// Gets or sets the value of the username
    /// </summary>
    [JsonProperty("username")]
    [JsonPropertyName("username")]
    public string? Username { get; set; }

    /// <summary>
    /// Gets or sets the value of the branch code
    /// </summary>
    [JsonProperty("branchcode")]
    [JsonPropertyName("branchcode")]
    public string? BranchCode { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonProperty("deviceid")]
    [JsonPropertyName("deviceid")]
    public string DeviceId { get; set; } = string.Empty;
}
