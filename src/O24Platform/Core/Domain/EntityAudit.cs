namespace O24OpenAPI.Core.Domain;

public partial class EntityAudit : BaseEntity
{
    public string? EntityName { get; set; }
    public int EntityId { get; set; }
    public string? UserId { get; set; }
    public string? ExecutionId { get; set; }
    public string? ActionType { get; set; } // I, U, D, DR, CR
    public string? Changes { get; set; }
}
