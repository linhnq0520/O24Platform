namespace O24OpenAPI.Client.Workflow;

/// <summary>
/// The workflow execution infor class
/// </summary>
public class WorkflowExecutionInfor
{
    /// <summary>
    /// Gets or sets the value of the execution id
    /// </summary>
    public string? execution_id { get; set; }

    /// <summary>
    /// Gets or sets the value of the input
    /// </summary>
    public object? input { get; set; }

    /// <summary>
    /// Gets or sets the value of the workflow id
    /// </summary>
    public string? workflow_id { get; set; }

    /// <summary>
    /// Gets or sets the value of the status
    /// </summary>
    public string? status { get; set; }

    /// <summary>
    /// Gets or sets the value of the created on
    /// </summary>
    public long created_on { get; set; }

    /// <summary>
    /// Gets or sets the value of the finish on
    /// </summary>
    public long finish_on { get; set; }

    /// <summary>
    /// Gets or sets the value of the is timeout
    /// </summary>
    public string? is_timeout { get; set; }

    /// <summary>
    /// Gets or sets the value of the is processing
    /// </summary>
    public string? is_processing { get; set; }

    /// <summary>
    /// Gets or sets the value of the is success
    /// </summary>
    public string? is_success { get; set; }

    /// <summary>
    /// Gets or sets the value of the workflow type
    /// </summary>
    public string? workflow_type { get; set; }

    /// <summary>
    /// Gets or sets the value of the reversed execution id
    /// </summary>
    public string? reversed_execution_id { get; set; }

    /// <summary>
    /// Gets or sets the value of the reversed by execution id
    /// </summary>
    public string? reversed_by_execution_id { get; set; }

    /// <summary>
    /// Gets or sets the value of the is disputed
    /// </summary>
    public string? is_disputed { get; set; }

    /// <summary>
    /// Gets or sets the value of the archiving time
    /// </summary>
    public long archiving_time { get; set; }

    /// <summary>
    /// Gets or sets the value of the purging time
    /// </summary>
    public long purging_time { get; set; }

    /// <summary>
    /// Gets or sets the value of the approved execution id
    /// </summary>
    public string? approved_execution_id { get; set; }

    /// <summary>
    /// Gets or sets the value of the transaction number
    /// </summary>
    public string? transaction_number { get; set; }

    /// <summary>
    /// Gets or sets the value of the transaction date
    /// </summary>
    public string? transaction_date { get; set; }

    /// <summary>
    /// Gets or sets the value of the value date
    /// </summary>
    public string? value_date { get; set; }

    /// <summary>
    /// Gets or sets the value of the service instances
    /// </summary>
    public WorkflowExecutionInforServiceInstance[]? service_instances { get; set; }

    /// <summary>
    /// The workflow execution infor service instance class
    /// </summary>
    public class WorkflowExecutionInforServiceInstance
    {
        /// <summary>
        /// Gets or sets the value of the service code
        /// </summary>
        public string? service_code { get; set; }

        /// <summary>
        /// Gets or sets the value of the instance id
        /// </summary>
        public string? instance_id { get; set; }
    }
}
