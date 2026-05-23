using O24OpenAPI.Core.Domain;

namespace O24OpenAPI.Framework.Domain;

public partial class MasterMapping : BaseEntity
{
    public string MasterClass { get; set; } = string.Empty;

    public string GLConfigClass { get; set; } = string.Empty;

    public string MasterTransClass { get; set; } = string.Empty;

    public string StatementClass { get; set; } = string.Empty;

    public string GLEntriesClass { get; set; } = string.Empty;

    public string MasterFields { get; set; } = string.Empty;

    public string MasterBranchCodeField { get; set; } = string.Empty;

    public string MasterCurrencyCodeField { get; set; } = string.Empty;

    public string MasterGLClass { get; set; } = string.Empty;

    public string MasterGLFields { get; set; } = string.Empty;
}
