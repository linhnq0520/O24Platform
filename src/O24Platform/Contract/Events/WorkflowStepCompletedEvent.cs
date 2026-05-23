namespace O24OpenAPI.Contracts.Events;

public class WorkflowStepCompletedEvent(string wfSchemeString) : IntegrationEvent
{
    public string? WFSchemeString { get; set; } = wfSchemeString;
}
