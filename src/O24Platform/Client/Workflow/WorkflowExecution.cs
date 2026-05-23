using System.ComponentModel.DataAnnotations;

namespace O24OpenAPI.Client.Workflow;

/// <summary>
/// The workflow execution class
/// </summary>
public class WorkflowExecution
{
    /// <summary>
    /// The enum is success enum
    /// </summary>
    public enum Enum_IS_SUCCESS
    {
        /// <summary>
        /// The  enum is success
        /// </summary>
        Y,

        /// <summary>
        /// The  enum is success
        /// </summary>
        N,

        /// <summary>
        /// The unknown enum is success
        /// </summary>
        Unknown,
    }

    /// <summary>
    /// The enum status enum
    /// </summary>
    public enum Enum_STATUS
    {
        /// <summary>
        /// The unknown enum status
        /// </summary>
        Unknown,

        /// <summary>
        /// The registered enum status
        /// </summary>
        Registered,

        /// <summary>
        /// The in progress enum status
        /// </summary>
        InProgress,

        /// <summary>
        /// The completed enum status
        /// </summary>
        Completed,

        /// <summary>
        /// The finished with error enum status
        /// </summary>
        FinishedWithError,

        /// <summary>
        /// The exception enum status
        /// </summary>
        Exception,

        /// <summary>
        /// The timeout enum status
        /// </summary>
        Timeout,
    }

    /// <summary>
    /// The enum is processing enum
    /// </summary>
    public enum Enum_IS_PROCESSING
    {
        /// <summary>
        /// The  enum is processing
        /// </summary>
        Y,

        /// <summary>
        /// The  enum is processing
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
    /// The enum workflow type enum
    /// </summary>
    public enum Enum_WORKFLOW_TYPE
    {
        /// <summary>
        /// The normal enum workflow type
        /// </summary>
        Normal,

        /// <summary>
        /// The reversal enum workflow type
        /// </summary>
        Reversal,
    }

    /// <summary>
    /// Gets or sets the value of the execution id
    /// </summary>
    [Key]
    [MaxLength(100)]
    public string EXECUTION_ID { get; set; }

    /// <summary>
    /// Gets or sets the value of the user id
    /// </summary>
    [MaxLength(100)]
    public string USER_ID { get; set; }

    /// <summary>
    /// Gets or sets the value of the organization id
    /// </summary>
    [MaxLength(100)]
    public string ORGANIZATION_ID { get; set; }

    /// <summary>
    /// Gets or sets the value of the input
    /// </summary>
    [MaxLength(4000)]
    public string INPUT { get; set; } = "";

    /// <summary>
    /// Gets or sets the value of the wfid
    /// </summary>
    [MaxLength(100)]
    public string WFID { get; set; } = "";

    /// <summary>
    /// Gets or sets the value of the lang
    /// </summary>
    [MaxLength(2)]
    public string LANG { get; set; } = "en";

    /// <summary>
    /// Gets or sets the value of the created on
    /// </summary>
    public long CREATED_ON { get; set; }

    /// <summary>
    /// Gets or sets the value of the status
    /// </summary>
    [MaxLength(100)]
    public Enum_STATUS STATUS { get; set; }

    /// <summary>
    /// Gets or sets the value of the finish on
    /// </summary>
    public long FINISH_ON { get; set; }

    /// <summary>
    /// Gets or sets the value of the is success
    /// </summary>
    [MaxLength(100)]
    public Enum_IS_SUCCESS IS_SUCCESS { get; set; } = Enum_IS_SUCCESS.Unknown;

    /// <summary>
    /// Gets or sets the value of the is timeout
    /// </summary>
    [MaxLength(100)]
    public Enum_IS_TIMEOUT IS_TIMEOUT { get; set; } = Enum_IS_TIMEOUT.N;

    /// <summary>
    /// Gets or sets the value of the is processing
    /// </summary>
    [MaxLength(100)]
    public Enum_IS_PROCESSING IS_PROCESSING { get; set; } = Enum_IS_PROCESSING.N;

    /// <summary>
    /// Gets or sets the value of the stop error
    /// </summary>
    [MaxLength(4000)]
    public string STOP_ERROR { get; set; }

    /// <summary>
    /// Gets or sets the value of the workflow type
    /// </summary>
    public Enum_WORKFLOW_TYPE WORKFLOW_TYPE { get; set; }

    /// <summary>
    /// Gets or sets the value of the reversed execution id
    /// </summary>
    public string REVERSED_EXECUTION_ID { get; set; } = "";

    /// <summary>
    /// Gets or sets the value of the reversed by execution id
    /// </summary>
    public string REVERSED_BY_EXECUTION_ID { get; set; } = "";

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkflowExecution"/> class
    /// </summary>
    public WorkflowExecution()
    {
        IS_SUCCESS = Enum_IS_SUCCESS.Unknown;
    }
}
