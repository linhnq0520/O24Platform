using O24OpenAPI.Core.Domain;

namespace O24OpenAPI.Framework.Domain;

public partial class JobExecutionLog : BaseEntity
{
    public string JobName { get; set; } = string.Empty;

    public DateTime StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public bool IsSuccess { get; set; }

    public string ErrorMessage { get; set; }

    public string EmbeddedData { get; set; }
}
