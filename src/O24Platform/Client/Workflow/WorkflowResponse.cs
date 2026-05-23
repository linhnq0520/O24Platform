namespace O24OpenAPI.Client.Workflow;

/// <summary>
/// The workflow response class
/// </summary>
public class WorkflowResponse<T>
{
    /// <summary>
    /// Gets or sets the value of the time in miliseconds
    /// </summary>
    public long time_in_miliseconds { get; set; }

    /// <summary>
    /// Gets or sets the value of the status
    /// </summary>
    public string? status { get; set; }

    /// <summary>
    /// Gets or sets the value of the description
    /// </summary>
    public string? description { get; set; }

    /// <summary>
    /// Gets or sets the value of the execution id
    /// </summary>
    public string? execution_id { get; set; }

    /// <summary>
    /// Gets or sets the value of the data
    /// </summary>
    public T? data { get; set; }
}

/// <summary>
/// The workflow response class
/// </summary>
public class WorkflowResponse
{
    public bool success { get; set; } = true;

    /// <summary>
    /// Gets or sets the value of the time in milliseconds
    /// </summary>
    public long time_in_milliseconds { get; set; }

    /// <summary>
    /// Gets or sets the value of the status
    /// </summary>
    public string? status { get; set; }

    /// <summary>
    /// Gets or sets the value of the description
    /// </summary>
    public string? description { get; set; }

    /// <summary>
    /// Gets or sets the value of the error code
    /// </summary>
    public string? error_code { get; set; }

    /// <summary>
    /// Gets or sets the value of the error message
    /// </summary>
    public string? error_message { get; set; }

    /// <summary>
    /// /// Gets or sets the value of the execution id
    /// </summary>
    public string? execution_id { get; set; }
    public string? error_next_action { get; set; }

    /// <summary>
    /// Gets or sets the value of the transaction number
    /// </summary>
    public object? transaction_number { get; set; }

    /// <summary>
    /// Gets or sets the value of the transaction date
    /// </summary>
    public object? transaction_date { get; set; }

    /// <summary>
    /// Gets or sets the value of the value date
    /// </summary>
    public object? value_date { get; set; }

    /// <summary>
    /// Gets or sets the value of the data
    /// </summary>
    public Dictionary<string, object> data { get; set; } = new();

    /// <summary>
    /// When true, the workflow stopped at a human-approval gate; use approval_request_id with approval APIs.
    /// </summary>
    public bool awaiting_approval { get; set; }

    /// <summary>
    /// Pending approval opaque id issued when <see cref="awaiting_approval"/> is true.
    /// </summary>
    public string? approval_request_id { get; set; }
}
