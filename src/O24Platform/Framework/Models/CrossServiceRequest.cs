using O24OpenAPI.Client.Enums;
using O24OpenAPI.Client.Scheme.Workflow;

namespace O24OpenAPI.Framework.Models;

/// <summary>
/// The cross service request class
/// </summary>
public class CrossServiceRequest
{
    /// <summary>
    /// Gets or sets the value of the full class name
    /// </summary>
    public string FullClassName { get; set; }

    /// <summary>
    /// Gets or sets the value of the method name
    /// </summary>
    public string MethodName { get; set; }

    /// <summary>
    /// Gets or sets the value of the workflow scheme
    /// </summary>
    public WorkflowScheme WorkflowScheme { get; set; }

    /// <summary>
    /// Gets or sets the value of the process number
    /// </summary>
    public ProcessNumber ProcessNumber { get; set; }
}
