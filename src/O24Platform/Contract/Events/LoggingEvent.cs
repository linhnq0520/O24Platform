using O24OpenAPI.Contracts.Models;

namespace O24OpenAPI.Contracts.Events;

public class LoggingEvent : IntegrationEvent
{
    public LogEntryModel? LogEntryModel { get; set; }
}
