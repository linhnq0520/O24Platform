using O24OpenAPI.Core.Configuration;
using O24OpenAPI.Core.Domain;
using O24OpenAPI.Core.Infrastructure;

namespace O24OpenAPI.Framework.Domain.Logging;

/// <summary>
/// The http log class
/// </summary>
/// <seealso cref="BaseEntity"/>
public partial class HttpLog : BaseEntity
{
    /// <summary>
    /// Gets or sets the value of the request id
    /// </summary>
    public string RequestId { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Gets or sets the value of the http method
    /// </summary>
    public string HttpMethod { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value of the request url
    /// </summary>
    public string RequestUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value of the request headers
    /// </summary>
    public string RequestHeaders { get; set; }

    /// <summary>
    /// Gets or sets the value of the request body
    /// </summary>
    public string RequestBody { get; set; }

    /// <summary>
    /// Gets or sets the value of the client ip
    /// </summary>
    public string ClientIp { get; set; }

    /// <summary>
    /// Gets or sets the value of the user agent
    /// </summary>
    public string UserAgent { get; set; }

    /// <summary>
    /// Gets or sets the value of the response status code
    /// </summary>
    public int ResponseStatusCode { get; set; } = 0;

    /// <summary>
    /// Gets or sets the value of the response headers
    /// </summary>
    public string ResponseHeaders { get; set; }

    /// <summary>
    /// Gets or sets the value of the response body
    /// </summary>
    public string ResponseBody { get; set; }

    /// <summary>
    /// Gets or sets the value of the service id
    /// </summary>
    public string ServiceId { get; set; } =
        Singleton<O24OpenAPIConfiguration>.Instance.YourServiceID;

    /// <summary>
    /// Gets or sets the value of the reference
    /// </summary>
    public string Reference { get; set; }

    /// <summary>
    /// Gets or sets the value of the exception message
    /// </summary>
    public string ExceptionMessage { get; set; }

    /// <summary>
    /// Gets or sets the value of the stack trace
    /// </summary>
    public string StackTrace { get; set; }

    /// <summary>
    /// Gets or sets the value of the begin on utc
    /// </summary>
    public DateTime BeginOnUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the value of the finish on utc
    /// </summary>
    public DateTime FinishOnUtc { get; set; }
}
