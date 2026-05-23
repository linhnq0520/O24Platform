namespace O24OpenAPI.Framework.Models;

/// <summary>
/// The request class
/// </summary>
public class Request<T>
{
    /// <summary>
    /// Gets or sets the value of the request header
    /// </summary>
    public RequestHeader RequestHeader { get; set; }

    /// <summary>
    /// Gets or sets the value of the request body
    /// </summary>
    public RequestBody<T> RequestBody { get; set; }

    /// <summary>
    /// Gets or sets the value of the request header
    /// </summary>
    public RequestHeader request_header { get; set; }

    /// <summary>
    /// Gets or sets the value of the request body
    /// </summary>
    public RequestBody<T> request_body { get; set; }
}
