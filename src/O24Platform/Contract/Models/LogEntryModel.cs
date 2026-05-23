namespace O24OpenAPI.Contracts.Models;

public class LogEntryModel
{
    public DateTimeOffset LogTimestamp { get; set; }
    public string LogLevel { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Exception { get; set; }
    public string? LogType { get; set; }
    public string? Direction { get; set; }
    public string? ActionName { get; set; }
    public string? Flow { get; set; }
    public long? DurationMs { get; set; }
    public string? Headers { get; set; } = string.Empty;
    public string? ServiceName { get; set; }
    public string? CorrelationId { get; set; }
    public string? RequestPayload { get; set; }
    public string? ResponsePayload { get; set; }
    public Dictionary<string, object?> AdditionalProperties { get; set; } = [];
}
