using System.ComponentModel.DataAnnotations;

namespace O24OpenAPI.Client.Workflow;

/// <summary>
/// The workflow step execution class
/// </summary>
public class WorkflowStepExecution
{
    /// <summary>
    /// The enum p1 status enum
    /// </summary>
    public enum Enum_P1_STATUS
    {
        /// <summary>
        /// The not processed enum p1 status
        /// </summary>
        NotProcessed,

        /// <summary>
        /// The created enum p1 status
        /// </summary>
        Created,

        /// <summary>
        /// The completed enum p1 status
        /// </summary>
        Completed,

        /// <summary>
        /// The error enum p1 status
        /// </summary>
        Error,
    }

    /// <summary>
    /// The enum p2 status enum
    /// </summary>
    public enum Enum_P2_STATUS
    {
        /// <summary>
        /// The not processed enum p2 status
        /// </summary>
        NotProcessed,

        /// <summary>
        /// The completed enum p2 status
        /// </summary>
        Completed,

        /// <summary>
        /// The failed enum p2 status
        /// </summary>
        Failed,

        /// <summary>
        /// The error enum p2 status
        /// </summary>
        Error,

        /// <summary>
        /// The timeout enum p2 status
        /// </summary>
        Timeout,
    }

    /// <summary>
    /// The enum p3 status enum
    /// </summary>
    public enum Enum_P3_STATUS
    {
        /// <summary>
        /// The not processed enum p3 status
        /// </summary>
        NotProcessed,

        /// <summary>
        /// The completed enum p3 status
        /// </summary>
        Completed,

        /// <summary>
        /// The error enum p3 status
        /// </summary>
        Error,
    }

    /// <summary>
    /// The enum is success enum
    /// </summary>
    public enum Enum_IS_SUCCESS
    {
        /// <summary>
        /// The not evaluated enum is success
        /// </summary>
        NotEvaluated,

        /// <summary>
        /// The  enum is success
        /// </summary>
        Y,

        /// <summary>
        /// The  enum is success
        /// </summary>
        N,
    }

    /// <summary>
    /// The enum is timeout enum
    /// </summary>
    public enum Enum_IS_TIMEOUT
    {
        /// <summary>
        /// The  enum is timeout
        /// </summary>
        Y,

        /// <summary>
        /// The  enum is timeout
        /// </summary>
        N,
    }

    /// <summary>
    /// Gets or sets the value of the step execution id
    /// </summary>
    [Key]
    [MaxLength(100)]
    public string STEP_EXECUTION_ID { get; set; } = "";

    /// <summary>
    /// Gets or sets the value of the execution id
    /// </summary>
    [MaxLength(100)]
    public string EXECUTION_ID { get; set; } = "";

    /// <summary>
    /// Gets or sets the value of the workflow id
    /// </summary>
    [MaxLength(100)]
    public string WORKFLOW_ID { get; set; } = "";

    /// <summary>
    /// Gets or sets the value of the step group
    /// </summary>
    public int STEP_GROUP { get; set; }

    /// <summary>
    /// Gets or sets the value of the step order
    /// </summary>
    public int STEP_ORDER { get; set; }

    /// <summary>
    /// Gets or sets the value of the step code
    /// </summary>
    [MaxLength(100)]
    public string STEP_CODE { get; set; } = "";

    /// <summary>
    /// Gets or sets the value of the p1 request id
    /// </summary>
    [MaxLength(100)]
    public string P1_REQUEST_ID { get; set; } = "";

    /// <summary>
    /// Gets or sets the value of the p1 start
    /// </summary>
    public long P1_START { get; set; }

    /// <summary>
    /// Gets or sets the value of the p1 finish
    /// </summary>
    public long P1_FINISH { get; set; }

    /// <summary>
    /// Gets or sets the value of the p1 status
    /// </summary>
    [MaxLength(100)]
    public Enum_P1_STATUS P1_STATUS { get; set; }

    /// <summary>
    /// Gets or sets the value of the p1 error
    /// </summary>
    [MaxLength(4000)]
    public string P1_ERROR { get; set; } = "";

    /// <summary>
    /// Gets or sets the value of the p1 content
    /// </summary>
    [MaxLength(4000)]
    public string P1_CONTENT { get; set; } = "";

    /// <summary>
    /// Gets or sets the value of the p2 request id
    /// </summary>
    [MaxLength(100)]
    public string P2_REQUEST_ID { get; set; } = "";

    /// <summary>
    /// Gets or sets the value of the p2 start
    /// </summary>
    public long P2_START { get; set; }

    /// <summary>
    /// Gets or sets the value of the p2 finish
    /// </summary>
    public long P2_FINISH { get; set; }

    /// <summary>
    /// Gets or sets the value of the p2 status
    /// </summary>
    [MaxLength(100)]
    public Enum_P2_STATUS P2_STATUS { get; set; }

    /// <summary>
    /// Gets or sets the value of the p2 response status
    /// </summary>
    [MaxLength(100)]
    public string P2_RESPONSE_STATUS { get; set; } = "";

    /// <summary>
    /// Gets or sets the value of the p2 error
    /// </summary>
    [MaxLength(4000)]
    public string P2_ERROR { get; set; } = "";

    /// <summary>
    /// Gets or sets the value of the p2 content
    /// </summary>
    [MaxLength(4000)]
    public string P2_CONTENT { get; set; } = "";

    /// <summary>
    /// Gets or sets the value of the is success
    /// </summary>
    [MaxLength(100)]
    public Enum_IS_SUCCESS IS_SUCCESS { get; set; }

    /// <summary>
    /// Gets or sets the value of the is timeout
    /// </summary>
    [MaxLength(100)]
    public Enum_IS_TIMEOUT IS_TIMEOUT { get; set; } = Enum_IS_TIMEOUT.N;

    /// <summary>
    /// Gets or sets the value of the p3 start
    /// </summary>
    public long P3_START { get; set; }

    /// <summary>
    /// Gets or sets the value of the p3 finish
    /// </summary>
    public long P3_FINISH { get; set; }

    /// <summary>
    /// Gets or sets the value of the p3 status
    /// </summary>
    [MaxLength(100)]
    public Enum_P3_STATUS P3_STATUS { get; set; }

    /// <summary>
    /// Gets or sets the value of the p3 content
    /// </summary>
    [MaxLength(4000)]
    public string P3_CONTENT { get; set; } = "";

    /// <summary>
    /// Gets or sets the value of the p3 error
    /// </summary>
    [MaxLength(4000)]
    public string P3_ERROR { get; set; } = "";

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkflowStepExecution"/> class
    /// </summary>
    public WorkflowStepExecution()
    {
        P1_STATUS = Enum_P1_STATUS.Created;
        P2_STATUS = Enum_P2_STATUS.NotProcessed;
        IS_TIMEOUT = Enum_IS_TIMEOUT.N;
    }
}
