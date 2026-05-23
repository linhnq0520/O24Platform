using System.Text.Json.Serialization;
using O24OpenAPI.Core.Configuration;
using O24OpenAPI.Core.Domain;
using O24OpenAPI.Core.Domain.Logging;
using O24OpenAPI.Core.Infrastructure;

namespace O24OpenAPI.Framework.Models.Logging;

/// <summary>
/// The call log request class
/// </summary>
/// <seealso cref="BaseO24OpenAPIModel"/>
public class CallLogRequest : BaseO24OpenAPIModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CallLogRequest"/> class
    /// </summary>
    public CallLogRequest()
    {
        ServiceId = Singleton<O24OpenAPIConfiguration>.Instance.YourServiceID;
        var workContext = EngineContext.Current.Resolve<WorkContext>();
        ExecutionLogId = workContext?.ExecutionLogId;
        ChannelId = workContext?.CurrentChannel;
        UserId = workContext?.UserContext.UserId;
        Reference = workContext?.ExecutionId;
    }

    [JsonPropertyName("execution_log_id")]
    public string ExecutionLogId { get; set; }

    /// <summary>
    /// Gets or sets the value of the log level id
    /// </summary>
    [JsonPropertyName("log_level_id")]
    public int LogLevelId { get; set; }

    /// <summary>
    /// Gets or sets the value of the channel id
    /// </summary>
    [JsonPropertyName("channel_id")]
    public string ChannelId { get; set; }

    /// <summary>
    /// Gets or sets the value of the service id
    /// </summary>
    [JsonPropertyName("service_id")]
    public string ServiceId { get; set; }

    /// <summary>
    /// Gets or sets the value of the status
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("code")]
    public string Code { get; set; }

    /// <summary>
    /// Gets or sets the value of the short message
    /// </summary>
    [JsonPropertyName("short_message")]
    public string ShortMessage { get; set; }

    /// <summary>
    /// Gets or sets the value of the full message
    /// </summary>
    [JsonPropertyName("full_message")]
    public string FullMessage { get; set; }

    /// <summary>
    /// Gets or sets the value of the data
    /// </summary>
    [JsonPropertyName("data")]
    public string Data { get; set; }

    /// <summary>
    /// Gets or sets the value of the user id
    /// </summary>
    [JsonPropertyName("user_id")]
    public string UserId { get; set; }

    /// <summary>
    /// Gets or sets the value of the reference
    /// </summary>
    [JsonPropertyName("reference")]
    public string Reference { get; set; }

    /// <summary>
    /// Gets or sets the value of the ip address
    /// </summary>
    [JsonPropertyName("ip_address")]
    public string IpAddress { get; set; }

    /// <summary>
    /// Gets or sets the value of the user agent
    /// </summary>
    [JsonPropertyName("user_agent")]
    public string UserAgent { get; set; }

    /// <summary>
    /// Gets or sets the value of the log level
    /// </summary>
    public LogLevel LogLevel
    {
        get => (LogLevel)LogLevelId;
        set => LogLevelId = (int)value;
    }
}
