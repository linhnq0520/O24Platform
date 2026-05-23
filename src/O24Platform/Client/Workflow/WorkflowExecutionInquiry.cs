namespace O24OpenAPI.Client.Workflow;

/// <summary>
/// The workflow execution inquiry class
/// </summary>
public class WorkflowExecutionInquiry
{
    /// <summary>
    /// Gets or sets the value of the execution
    /// </summary>
    public WorkflowExecutionInfor? execution { get; set; }

    /// <summary>
    /// Gets or sets the value of the execution steps
    /// </summary>
    public List<WorkflowStepInfor>? execution_steps { get; set; }

    /// <summary>
    /// Gets or sets the value of the error message
    /// </summary>
    public string? error_message { get; set; }
}
