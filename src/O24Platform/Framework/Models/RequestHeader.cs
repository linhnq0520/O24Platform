namespace O24OpenAPI.Framework.Models;

/// <summary>
/// The request header class
/// </summary>
public class RequestHeader
{
    /// <summary>
    /// Gets or sets the value of the service id
    /// </summary>
    public string ServiceId { get; set; }

    /// <summary>
    /// Gets or sets the value of the server ip
    /// </summary>
    public string serverIp { get; set; }

    /// <summary>
    /// Gets or sets the value of the utc send time
    /// </summary>
    public long UtcSendTime { get; set; }

    /// <summary>
    /// Gets or sets the value of the step timeout
    /// </summary>
    public long StepTimeout { get; set; }

    /// <summary>
    /// Gets or sets the value of the execution id
    /// </summary>
    public string ExecutionId { get; set; }

    /// <summary>
    /// Gets or sets the value of the step execution id
    /// </summary>
    public string StepExecutionId { get; set; }

    /// <summary>
    /// Gets or sets the value of the step code
    /// </summary>
    public string StepCode { get; set; }

    /// <summary>
    /// Gets or sets the value of the is compensated
    /// </summary>
    public string IsCompensated { get; set; }

    /// <summary>
    /// Gets or sets the value of the sending condition passed
    /// </summary>
    public bool SendingConditionPassed { get; set; }

    /// <summary>
    /// Gets or sets the value of the service id
    /// </summary>
    public string service_id { get; set; }

    /// <summary>
    /// Gets or sets the value of the server ip
    /// </summary>
    public string server_ip { get; set; }

    /// <summary>
    /// Gets or sets the value of the utc send time
    /// </summary>
    public long utc_send_time { get; set; }

    /// <summary>
    /// Gets or sets the value of the step timeout
    /// </summary>
    public long step_timeout { get; set; }

    /// <summary>
    /// Gets or sets the value of the execution id
    /// </summary>
    public string execution_id { get; set; }

    /// <summary>
    /// Gets or sets the value of the step execution id
    /// </summary>
    public string step_execution_id { get; set; }

    /// <summary>
    /// Gets or sets the value of the step code
    /// </summary>
    public string step_code { get; set; }

    /// <summary>
    /// Gets or sets the value of the to queue name
    /// </summary>
    public string to_queue_name { get; set; }

    /// <summary>
    /// Gets or sets the value of the is compensated
    /// </summary>
    public string is_compensated { get; set; }

    /// <summary>
    /// Gets or sets the value of the step mode
    /// </summary>
    public string step_mode { get; set; } = "TWOWAY";

    /// <summary>
    /// Gets or sets the value of the organization id
    /// </summary>
    public string organization_id { get; set; }

    /// <summary>
    /// Gets or sets the value of the sending condition passed
    /// </summary>
    public bool sending_condition_passed { get; set; }
}
