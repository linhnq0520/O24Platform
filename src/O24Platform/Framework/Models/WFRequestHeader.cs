using O24OpenAPI.Client.Enums;

namespace O24OpenAPI.Framework.Models;

/// <summary>
/// The wf request header class
/// </summary>
public class WFRequestHeader
{
    /// <summary>
    /// Gets or sets the value of the service id
    /// </summary>
    public string service_id { get; set; } = "";

    /// <summary>
    /// Gets or sets the value of the server ip
    /// </summary>
    public string server_ip { get; set; } = "";

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
    public string execution_id { get; set; } = "";

    /// <summary>
    /// Gets or sets the value of the step execution id
    /// </summary>
    public string step_execution_id { get; set; } = "";

    /// <summary>
    /// Gets or sets the value of the from queue name
    /// </summary>
    public string from_queue_name { get; set; } = "";

    /// <summary>
    /// Gets or sets the value of the to queue name
    /// </summary>
    public string to_queue_name { get; set; } = "";

    /// <summary>
    /// Gets or sets the value of the is compensated
    /// </summary>
    public string is_compensated { get; set; } = "N";

    /// <summary>
    /// Gets or sets the value of the step mode
    /// </summary>
    public string step_mode { get; set; } = "TWOWAY";

    /// <summary>
    /// Gets or sets the value of the step code
    /// </summary>
    public string step_code { get; set; } = "";

    /// <summary>
    /// Gets or sets the value of the step order
    /// </summary>
    public int step_order { get; set; }

    /// <summary>
    /// Gets or sets the value of the cache execution id
    /// </summary>
    public string cache_execution_id { get; set; }

    /// <summary>
    /// Gets or sets the value of the user id
    /// </summary>
    public string user_id { get; set; }

    /// <summary>
    /// Gets or sets the value of the organization id
    /// </summary>
    public string organization_id { get; set; }

    /// <summary>
    /// Gets or sets the value of the workflow type
    /// </summary>
    public Client.Scheme.Workflow.WFScheme.REQUEST.REQUESTHEADER.EnumWorkflowType workflow_type { get; set; }

    /// <summary>
    /// Gets or sets the value of the reversal execution id
    /// </summary>
    public string reversal_execution_id { get; set; }

    /// <summary>
    /// Gets or sets the value of the approval execution id
    /// </summary>
    public string approval_execution_id { get; set; }

    /// <summary>
    /// Gets or sets the value of the sending condition passed
    /// </summary>
    public bool sending_condition_passed { get; set; } = true;

    /// <summary>
    /// Gets or sets the value of the channel id
    /// </summary>
    public string channel_id { get; set; } = "";

    /// <summary>
    /// Gets or sets the value of the service instance id
    /// </summary>
    public string service_instance_id { get; set; } = "";

    /// <summary>
    /// Gets or sets the value of the client device id
    /// </summary>
    public string client_device_id { get; set; } = "";

    /// <summary>
    /// Gets or sets the value of the tx context
    /// </summary>
    public Dictionary<string, object> tx_context { get; set; } = [];

    /// <summary>
    /// Gets or sets the value of the processing version
    /// </summary>
    public ProcessNumber processing_version { get; set; }
}
