using System.Text.Json;
using O24OpenAPI.Client.Enums;
using O24OpenAPI.Client.Log;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace O24OpenAPI.Client.Scheme.Workflow;

/// <summary>
/// The workflow scheme class
/// </summary>
/// <seealso cref="IValidScheme"/>
public class WorkflowScheme : IValidScheme
{
    /// <summary>
    /// Gets or sets the value of the request
    /// </summary>
    public REQUEST Request { get; set; } = new REQUEST();

    /// <summary>
    /// Gets or sets the value of the response
    /// </summary>
    public RESPONSE Response { get; set; } = new RESPONSE();

    /// <summary>
    /// Gets or sets the value of the request
    /// </summary>
    public REQUEST request { get; set; } = new REQUEST();

    /// <summary>
    /// Gets or sets the value of the response
    /// </summary>
    public RESPONSE response { get; set; } = new RESPONSE();

    /// <summary>
    /// Ises the valid using the specified error
    /// </summary>
    /// <param name="error">The error</param>
    /// <returns>The bool</returns>
    public bool IsValid(out string error)
    {
        string error1 = "";
        int num = this.Request.IsValid(out error1) ? 1 : 0;
        string error2 = "";
        bool flag = this.Response.IsValid(out error2);
        object obj = (object)
            new
            {
                Request = new { IsValid = num != 0, Error = error1 },
                Response = new { IsValid = flag, Error = error2 },
            };
        error = JsonSerializer.Serialize(obj);
        return (num & (flag ? 1 : 0)) != 0;
    }

    /// <summary>
    /// Ises the reversal
    /// </summary>
    /// <returns>The bool</returns>
    public bool IsReversal() => this.Request.RequestHeader.IsReversal();

    /// <summary>
    /// Ises the compensated
    /// </summary>
    /// <returns>The bool</returns>
    public bool IsCompensated() => this.Request.RequestHeader.IsCompensated();

    /// <summary>
    /// Ises the success
    /// </summary>
    /// <returns>The bool</returns>
    public bool IsSuccess()
    {
        return this.Response.Status == WorkflowScheme.RESPONSE.EnumResponseStatus.SUCCESS;
    }

    /// <summary>
    /// Ises the error
    /// </summary>
    /// <returns>The bool</returns>
    public bool IsError()
    {
        return this.Response.Status == WorkflowScheme.RESPONSE.EnumResponseStatus.ERROR;
    }

    /// <summary>
    /// Creates the centralized log using the specified subject
    /// </summary>
    /// <param name="subject">The subject</param>
    /// <param name="log_text">The log text</param>
    /// <returns>The centralized log</returns>
    public static CentralizedLog CreateCentralizedLog(string subject, string log_text)
    {
        return new CentralizedLog() { subject = subject, log_text = log_text };
    }

    /// <summary>
    /// The input class
    /// </summary>
    /// <seealso cref="Dictionary{string, object}"/>
    public class INPUT : Dictionary<string, object> { }

    /// <summary>
    /// The request class
    /// </summary>
    /// <seealso cref="IValidScheme"/>
    public class REQUEST : IValidScheme
    {
        /// <summary>
        /// Gets or sets the value of the request header
        /// </summary>
        public REQUESTHEADER RequestHeader { get; set; } = new REQUESTHEADER();

        /// <summary>
        /// Gets or sets the value of the request body
        /// </summary>
        public REQUESTBODY RequestBody { get; set; } = new REQUESTBODY();

        /// <summary>
        /// Gets or sets the value of the request header
        /// </summary>
        public REQUESTHEADER request_header { get; set; } = new REQUESTHEADER();

        /// <summary>
        /// Gets or sets the value of the request body
        /// </summary>
        public REQUESTBODY request_body { get; set; } = new REQUESTBODY();

        /// <summary>
        /// Ises the valid using the specified error
        /// </summary>
        /// <param name="error">The error</param>
        /// <returns>The bool</returns>
        public bool IsValid(out string error)
        {
            string error1 = "";
            int num = this.RequestHeader.IsValid(out error1) ? 1 : 0;
            string error2 = "";
            bool flag = this.RequestHeader.IsValid(out error2);
            object obj = (object)
                new
                {
                    RequestHeader = new { IsValid = num != 0, Error = error1 },
                    RequestBody = new { IsValid = flag, Error = error2 },
                };
            error = JsonSerializer.Serialize(obj);
            return (num & (flag ? 1 : 0)) != 0;
        }

        /// <summary>
        /// The requestheader class
        /// </summary>
        /// <seealso cref="IValidScheme"/>
        public class REQUESTHEADER : IValidScheme
        {
            /// <summary>
            /// Gets or sets the value of the channel
            /// </summary>
            public string Channel { get; set; } = "";

            /// <summary>
            /// Gets or sets the value of the service id
            /// </summary>
            public string ServiceId { get; set; } = "";

            /// <summary>
            /// Gets or sets the value of the server ip
            /// </summary>
            public string ServerIp { get; set; } = "";

            /// <summary>
            /// Gets or sets the value of the utc send time
            /// </summary>
            public long UtcSendTime { get; set; }

            /// <summary>
            /// Gets or sets the value of the step timeout
            /// </summary>
            public long StepTimeout { get; set; } = 60000;

            /// <summary>
            /// Gets or sets the value of the execution id
            /// </summary>
            public string ExecutionId { get; set; } = "";

            /// <summary>
            /// Gets or sets the value of the cache execution id
            /// </summary>
            public string CacheExecutionId { get; set; } = "";

            /// <summary>
            /// Gets or sets the value of the step execution id
            /// </summary>
            public string StepExecutionId { get; set; } = "";

            /// <summary>
            /// Gets or sets the value of the step order
            /// </summary>
            public int StepOrder { get; set; } = 1;

            /// <summary>
            /// Gets or sets the value of the step code
            /// </summary>
            public string StepCode { get; set; } = "";

            /// <summary>
            /// Gets or sets the value of the user id
            /// </summary>
            public string? UserId { get; set; }

            /// <summary>
            /// Gets or sets the value of the workflow type
            /// </summary>
            public EnumWorkflowType WorkflowType { get; set; }

            /// <summary>
            /// Gets or sets the value of the reversal execution id
            /// </summary>
            public string? ReversalExecutionId { get; set; }

            /// <summary>
            /// Gets or sets the value of the approval execution id
            /// </summary>
            public string? ApprovalExecutionId { get; set; }

            /// <summary>
            /// Gets or sets the value of the sending condition passed
            /// </summary>
            public bool SendingConditionPassed { get; set; } = true;

            /// <summary>
            /// Gets or sets the value of the tx context
            /// </summary>
            public Dictionary<string, object> TxContext { get; set; } =
                new Dictionary<string, object>();

            /// <summary>
            /// Gets or sets the value of the processing version
            /// </summary>
            public ProcessNumber ProcessingVersion { get; set; }

            /// <summary>
            /// Gets or sets the value of the process number
            /// </summary>
            public ProcessNumber ProcessNumber { get; set; }

            /// <summary>
            /// Ises the reversal
            /// </summary>
            /// <returns>The bool</returns>
            public bool IsReversal() => !string.IsNullOrEmpty(this.ReversalExecutionId);

            /// <summary>
            /// Ises the compensated
            /// </summary>
            /// <returns>The bool</returns>
            public bool IsCompensated() => this.is_compensated.Equals("Y");

            /// <summary>
            /// Client Device Id
            /// </summary>
            public string ClientDeviceId { get; set; } = "";

            /// <summary>
            /// Ises the valid using the specified error
            /// </summary>
            /// <param name="error">The error</param>
            /// <returns>The bool</returns>
            public bool IsValid(out string error)
            {
                error = "";
                if (this.ServiceId == "")
                {
                    error = "service_id is required";
                }
                else if (this.ServerIp == "")
                {
                    error = "service_ip is required";
                }
                else if (this.UtcSendTime == 0L)
                {
                    error = "gmt_sent_time is required";
                }
                else if (this.StepExecutionId == "")
                {
                    error = "step_execution_id is required";
                }

                return error == "";
            }

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
            public string? cache_execution_id { get; set; }

            /// <summary>
            /// Gets or sets the value of the user id
            /// </summary>
            public string? user_id { get; set; }

            /// <summary>
            /// Gets or sets the value of the organization id
            /// </summary>
            public string? organization_id { get; set; }

            /// <summary>
            /// Gets or sets the value of the workflow type
            /// </summary>
            public EnumWorkflowType workflow_type { get; set; }

            /// <summary>
            /// Gets or sets the value of the reversal execution id
            /// </summary>
            public string? reversal_execution_id { get; set; }

            /// <summary>
            /// Gets or sets the value of the approval execution id
            /// </summary>
            public string? approval_execution_id { get; set; }

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
            /// Gets or sets the value of the tx context
            /// </summary>
            public Dictionary<string, object> tx_context { get; set; } =
                new Dictionary<string, object>();

            /// <summary>
            /// Gets or sets the value of the processing version
            /// </summary>
            public ProcessNumber processing_version { get; set; }

            /// <summary>
            /// Ises the reversal 1
            /// </summary>
            /// <returns>The bool</returns>
            public bool IsReversal1()
            {
                return !string.IsNullOrEmpty(reversal_execution_id);
            }

            /// <summary>
            /// Ises the compensated 1
            /// </summary>
            /// <returns>The bool</returns>
            public bool IsCompensated1()
            {
                return is_compensated.Equals("Y");
            }

            /// <summary>
            /// Ises the valid 1 using the specified error
            /// </summary>
            /// <param name="error">The error</param>
            /// <returns>The bool</returns>
            public bool IsValid1(out string error)
            {
                error = "";
                if (service_id == "")
                {
                    error = "service_id is required";
                }
                else if (server_ip == "")
                {
                    error = "service_ip is required";
                }
                else if (utc_send_time == 0L)
                {
                    error = "gmt_sent_time is required";
                }
                else if (step_execution_id == "")
                {
                    error = "step_execution_id is required";
                }
                else if (from_queue_name == "")
                {
                    error = "from_queue_name is required";
                }
                else if (to_queue_name == "")
                {
                    error = "to_queue_name is required";
                }
                return error == "";
            }

            /// <summary>
            /// The enum workflow type enum
            /// </summary>
            public enum EnumWorkflowType
            {
                /// <summary>
                /// The normal enum workflow type
                /// </summary>
                normal,

                /// <summary>
                /// The reverse enum workflow type
                /// </summary>
                reverse,

                /// <summary>
                /// The approval enum workflow type
                /// </summary>
                approval,
            }
        }

        /// <summary>
        /// The requestbody class
        /// </summary>
        /// <seealso cref="IValidScheme"/>
        public class REQUESTBODY : IValidScheme
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="REQUESTBODY"/> class
            /// </summary>
            public REQUESTBODY() { }

            /// <summary>
            /// Gets or sets the value of the workflow input
            /// </summary>
            public object WorkflowInput { get; set; } = new object();

            /// <summary>
            /// Gets or sets the value of the data
            /// </summary>
            public object Data { get; set; }

            /// <summary>
            /// Ises the valid using the specified error
            /// </summary>
            /// <param name="error">The error</param>
            /// <returns>The bool</returns>
            public bool IsValid(out string error)
            {
                error = "";
                return error == "";
            }

            /// <summary>
            /// Gets or sets the value of the workflow input
            /// </summary>
            public object workflow_input { get; set; } = new object();

            /// <summary>
            /// Gets or sets the value of the data
            /// </summary>
            public object data { get; set; }
        }
    }

    /// <summary>
    /// The response class
    /// </summary>
    /// <seealso cref="IValidScheme"/>
    public class RESPONSE : IValidScheme
    {
        /// <summary>
        /// Gets or sets the value of the status
        /// </summary>
        public EnumResponseStatus Status { get; set; } = EnumResponseStatus.PROCESSING;

        /// <summary>
        /// Gets or sets the value of the data
        /// </summary>
        public object Data { get; set; } = new object();

        /// <summary>
        /// Gets or sets the value of the error message
        /// </summary>
        public string ErrorMessage { get; set; } = "";

        /// <summary>
        /// Gets or sets the value of the raw error
        /// </summary>
        public string RawError { get; set; } = "";

        /// <summary>
        /// Gets or sets the value of the error code
        /// </summary>
        public string ErrorCode { get; set; } = "";

        /// <summary>
        /// Gets or sets the value of the status
        /// </summary>
        public EnumResponseStatus status { get; set; }

        /// <summary>
        /// Gets or sets the value of the data
        /// </summary>
        public object data { get; set; } = new object();

        /// <summary>
        /// Gets or sets the value of the error message
        /// </summary>
        public string error_message { get; set; } = "";

        /// <summary>
        /// Gets or sets the value of the error code
        /// </summary>
        public string error_code { get; set; } = "";

        /// <summary>
        /// Gets or sets the value of the raw error
        /// </summary>
        public string raw_error { get; set; } = "";

        /// <summary>
        /// Ises the valid using the specified error
        /// </summary>
        /// <param name="error">The error</param>
        /// <returns>The bool</returns>
        public bool IsValid(out string error)
        {
            error = "";
            return error == "";
        }

        /// <summary>
        /// Ises the success
        /// </summary>
        /// <returns>The bool</returns>
        public bool IsSuccess()
        {
            return status == EnumResponseStatus.SUCCESS;
        }

        /// <summary>
        /// The enum response status enum
        /// </summary>
        public enum EnumResponseStatus
        {
            /// <summary>
            /// The success enum response status
            /// </summary>
            SUCCESS,

            /// <summary>
            /// The error enum response status
            /// </summary>
            ERROR,

            /// <summary>
            /// The processing enum response status
            /// </summary>
            PROCESSING,
        }
    }
}
