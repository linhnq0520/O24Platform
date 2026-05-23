namespace O24OpenAPI.Framework.Models;

/// <summary>
/// The wf request class
/// </summary>
public class WFRequest<T>
{
    /// <summary>
    /// Gets or sets the value of the request header
    /// </summary>
    public WFRequestHeader request_header { get; set; }

    /// <summary>
    /// Gets or sets the value of the request body
    /// </summary>
    public WFRequestBody<T> request_body { get; set; }
}
