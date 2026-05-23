namespace O24OpenAPI.Framework.Models;

/// <summary>
/// The workflow class
/// </summary>
public class Workflow<T>
{
    /// <summary>
    /// Gets or sets the value of the request
    /// </summary>
    public Request<T> Request { get; set; }

    /// <summary>
    /// Gets or sets the value of the response
    /// </summary>
    public Response Response { get; set; }

    /// <summary>
    /// Gets or sets the value of the request
    /// </summary>
    public Request<T> request { get; set; }

    /// <summary>
    /// Gets or sets the value of the response
    /// </summary>
    public Response response { get; set; }
}
