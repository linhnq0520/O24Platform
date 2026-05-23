namespace O24OpenAPI.Client.Workflow;

/// <summary>
/// The workflow step infor class
/// </summary>
public class WorkflowStepInfor
{
    /// <summary>
    /// Gets or sets the value of the step execution id
    /// </summary>
    public string? step_execution_id { get; set; }

    /// <summary>
    /// Gets or sets the value of the execution id
    /// </summary>
    public string? execution_id { get; set; }

    /// <summary>
    /// Gets or sets the value of the workflow id
    /// </summary>
    public string? workflow_id { get; set; }

    /// <summary>
    /// Gets or sets the value of the step group
    /// </summary>
    public int step_group { get; set; }

    /// <summary>
    /// Gets or sets the value of the step order
    /// </summary>
    public int step_order { get; set; }

    /// <summary>
    /// Gets or sets the value of the step code
    /// </summary>
    public string? step_code { get; set; }

    /// <summary>
    /// Gets or sets the value of the request id
    /// </summary>
    public string? request_id { get; set; }

    /// <summary>
    /// Gets or sets the value of the sending condition
    /// </summary>
    public object? sending_condition { get; set; }

    /// <summary>
    /// Gets or sets the value of the p1 start
    /// </summary>
    public long p1_start { get; set; }

    /// <summary>
    /// Gets or sets the value of the p1 finish
    /// </summary>
    public long p1_finish { get; set; }

    /// <summary>
    /// Gets or sets the value of the p1 status
    /// </summary>
    public string? p1_status { get; set; }

    /// <summary>
    /// Gets or sets the value of the p1 error
    /// </summary>
    public string? p1_error { get; set; }

    /// <summary>
    /// Gets or sets the value of the p1 content
    /// </summary>
    public object? p1_content { get; set; }

    /// <summary>
    /// Gets or sets the value of the p2 request id
    /// </summary>
    public string? p2_request_id { get; set; }

    /// <summary>
    /// Gets or sets the value of the p2 start
    /// </summary>
    public long p2_start { get; set; }

    /// <summary>
    /// Gets or sets the value of the p2 finish
    /// </summary>
    public long p2_finish { get; set; }

    /// <summary>
    /// Gets or sets the value of the p2 status
    /// </summary>
    public string? p2_status { get; set; }

    /// <summary>
    /// Gets or sets the value of the p2 error
    /// </summary>
    public string? p2_error { get; set; }

    /// <summary>
    /// Gets or sets the value of the p2 error code
    /// </summary>
    public string? p2_error_code { get; set; }

    /// <summary>
    /// Gets or sets the value of the p2 content
    /// </summary>
    public object? p2_content { get; set; }

    /// <summary>
    /// Gets or sets the value of the is success
    /// </summary>
    public string? is_success { get; set; }

    /// <summary>
    /// Gets or sets the value of the is timeout
    /// </summary>
    public string? is_timeout { get; set; }

    /// <summary>
    /// Gets or sets the value of the p3 start
    /// </summary>
    public long p3_start { get; set; }

    /// <summary>
    /// Gets or sets the value of the p3 finish
    /// </summary>
    public long p3_finish { get; set; }

    /// <summary>
    /// Gets or sets the value of the p3 status
    /// </summary>
    public string? p3_status { get; set; }

    /// <summary>
    /// Gets or sets the value of the p3 error
    /// </summary>
    public string? p3_error { get; set; }

    /// <summary>
    /// Gets or sets the value of the p3 content
    /// </summary>
    public object? p3_content { get; set; }

    /// <summary>
    /// Gets or sets the value of the is disputed
    /// </summary>
    public string? is_disputed { get; set; }

    /// <summary>
    /// Gets or sets the value of the p4 status
    /// </summary>
    public string? p4_status { get; set; }

    /// <summary>
    /// Gets or sets the value of the p4 content
    /// </summary>
    public object? p4_content { get; set; }
}
