namespace O24OpenAPI.Client.Workflow;

/// <summary>
/// The workflow execution status response class
/// </summary>
public class WorkflowExecutionStatusResponse
{
    /// <summary>
    /// Gets or sets the value of the execution id
    /// </summary>
    public string? execution_id { get; set; }

    /// <summary>
    /// Gets or sets the value of the status
    /// </summary>
    public string? status { get; set; }

    /// <summary>
    /// Gets or sets the value of the description
    /// </summary>
    public string? description { get; set; }
}
