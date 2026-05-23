namespace O24OpenAPI.Framework.Models;

public class JobExecutionHistorySearchResponse : BaseO24OpenAPIModel
{
    public int Id { get; set; }
    public string JobName { get; set; } = string.Empty;

    public DateTime StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public bool IsSuccess { get; set; }

    public string? ErrorMessage { get; set; }

    public string? EmbeddedData { get; set; }
}
