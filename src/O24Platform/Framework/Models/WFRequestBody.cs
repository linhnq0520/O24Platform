namespace O24OpenAPI.Framework.Models;

/// <summary>
/// The wf request body class
/// </summary>
public class WFRequestBody<T>
{
    /// <summary>
    /// Gets or sets the value of the workflow input
    /// </summary>
    public WFInput workflow_input { get; set; }

    /// <summary>
    /// Gets or sets the value of the data
    /// </summary>
    public T data { get; set; }
}
