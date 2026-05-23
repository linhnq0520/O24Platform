namespace O24OpenAPI.Framework.Models;

/// <summary>
/// The request body class
/// </summary>
public class RequestBody<T>
{
    /// <summary>
    /// Gets or sets the value of the workflow input
    /// </summary>
    public WorkflowInput WorkflowInput { get; set; }

    /// <summary>
    /// Gets or sets the value of the data
    /// </summary>
    public T Data { get; set; }

    /// <summary>
    /// Gets or sets the value of the workflow input
    /// </summary>
    public WorkflowInput workflow_input { get; set; }

    /// <summary>
    /// Gets or sets the value of the data
    /// </summary>
    public T data { get; set; }
}
