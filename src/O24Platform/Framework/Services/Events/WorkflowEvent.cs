using O24OpenAPI.Client.Events;
using O24OpenAPI.Client.Events.EventData;

namespace O24OpenAPI.Framework.Services.Events;

/// <summary>
/// The workflow event class
/// </summary>
public class WorkflowEvent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WorkflowEvent"/> class
    /// </summary>
    public WorkflowEvent() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkflowEvent"/> class
    /// </summary>
    /// <param name="e">The </param>
    public WorkflowEvent(O24OpenAPIEvent<WorkflowExecutionEventData> e)
    {
        EventName = e.EventTypeName;
        EventData = e.EventData.data;
        ExecutionId = e.EventData.data.ExecutionId;
        WorkflowId = e.EventData.data.WorkflowId;
    }

    /// <summary>
    /// Gets or sets the value of the event name
    /// </summary>
    public string EventName { get; set; }

    /// <summary>
    /// Gets or sets the value of the execution id
    /// </summary>
    public string ExecutionId { get; set; }

    /// <summary>
    /// Gets or sets the value of the workflow id
    /// </summary>
    public string WorkflowId { get; set; }

    /// <summary>
    /// Gets or sets the value of the event data
    /// </summary>
    public WorkflowExecutionEventData EventData { get; set; }
}
