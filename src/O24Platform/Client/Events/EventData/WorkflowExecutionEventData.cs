namespace O24OpenAPI.Client.Events.EventData;

public class WorkflowExecutionEventData
{
    public string? WorkflowId { get; set; }
    public string? ExecutionId { get; set; }
    public Dictionary<string, object>? Data { get; set; }
    public bool IsSuccess { get; set; }
    public string? ChannelId { get; set; }
    public string? UserId { get; set; }
}
