using O24OpenAPI.Client.Enums;
using O24OpenAPI.Client.Log;
using O24OpenAPI.Core.Domain;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace O24OpenAPI.Client.Scheme.Workflow;

/// <summary>
/// The wf scheme class
/// </summary>
/// <seealso cref="IValidScheme"/>
public class WFScheme : IValidScheme
{
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
        /// The requestheader class
        /// </summary>
        /// <seealso cref="IValidScheme"/>
        public class REQUESTHEADER : IValidScheme
        {
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
                /// /// The reverse enum workflow type
                /// </summary>
                reverse,

                /// <summary>
                /// The approval enum workflow type
                /// </summary>
                approval,
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
            /// Gets or sets the value of the publish queue name
            /// </summary>
            /// <value></value>
            public string fanout_exchange { get; set; } = "";

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
            /// Gets or sets the value of the user_code
            /// </summary>
            public string user_code { get; set; } = string.Empty;

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

            public object? worker_id { get; set; }

            /// <summary>
            /// Ises the reversal
            /// </summary>
            /// <returns>The bool</returns>
            public bool IsReversal()
            {
                return !string.IsNullOrEmpty(reversal_execution_id);
            }

            /// <summary>
            /// Ises the compensated
            /// </summary>
            /// <returns>The bool</returns>
            public bool IsCompensated()
            {
                return is_compensated.Equals("Y");
            }

            /// <summary>
            /// Ises the valid using the specified error
            /// </summary>
            /// <param name="error">The error</param>
            /// <returns>The bool</returns>
            public bool IsValid(out string error)
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
        }

        /// <summary>
        /// The requestbody class
        /// </summary>
        /// <seealso cref="IValidScheme"/>
        public class REQUESTBODY : IValidScheme
        {
            /// <summary>
            /// Gets or sets the value of the workflow input
            /// </summary>
            public object workflow_input { get; set; } = new object();

            /// <summary>
            /// Gets or sets the value of the data
            /// </summary>
            public Dictionary<string, object>? data { get; set; }

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
        }

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
            bool num = request_header.IsValid(out string error2);
            bool flag = request_header.IsValid(out string error3);
            object value = new
            {
                RequestHeader = new { IsValid = num, Error = error2 },
                RequestBody = new { IsValid = flag, Error = error3 },
            };
            error = JsonSerializer.Serialize(value);
            return num && flag;
        }
    }

    /// <summary>
    /// The response class
    /// </summary>
    /// <seealso cref="IValidScheme"/>
    public class RESPONSE : IValidScheme
    {
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
        public string error_next_action { get; set; } = "";

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
    }

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
        bool num = request.IsValid(out string error2);
        bool flag = response.IsValid(out string error3);
        object value = new
        {
            Request = new { IsValid = num, Error = error2 },
            Response = new { IsValid = flag, Error = error3 },
        };
        error = JsonSerializer.Serialize(value);
        return num && flag;
    }

    /// <summary>
    /// Ises the reversal
    /// </summary>
    /// <returns>The bool</returns>
    public bool IsReversal()
    {
        return request.request_header.IsReversal();
    }

    /// <summary>
    /// Ises the compensated
    /// </summary>
    /// <returns>The bool</returns>
    public bool IsCompensated()
    {
        return request.request_header.IsCompensated();
    }

    /// <summary>
    /// Ises the success
    /// </summary>
    /// <returns>The bool</returns>
    public bool IsSuccess()
    {
        return response.status == RESPONSE.EnumResponseStatus.SUCCESS;
    }

    /// <summary>
    /// Ises the error
    /// </summary>
    /// <returns>The bool</returns>
    public bool IsError()
    {
        return response.status == RESPONSE.EnumResponseStatus.ERROR;
    }

    /// <summary>
    /// Creates the centralized log using the specified subject
    /// </summary>
    /// <param name="subject">The subject</param>
    /// <param name="log_text">The log text</param>
    /// <returns>The centralized log</returns>
    public static CentralizedLog CreateCentralizedLog(string subject, string log_text)
    {
        return new CentralizedLog { subject = subject, log_text = log_text };
    }

    public void SetWorkContext(WorkContext workContext)
    {
        workContext.SetCurrentChannel(request.request_header.channel_id);
        workContext.SetExecutionId(request.request_header.execution_id);
        workContext.UserContext.SetUserId(request.request_header.user_id);
        workContext.UserContext.SetUserCode(request.request_header.user_code);
        workContext.UserContext.SetUserChannel(request.request_header.channel_id);
    }

    public object? GetWorkerId()
    {
        return request.request_header.worker_id;
    }
}
