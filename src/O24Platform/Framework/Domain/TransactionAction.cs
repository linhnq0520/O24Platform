using O24OpenAPI.Core.Domain;

namespace O24OpenAPI.Framework.Domain;

public partial class TransactionAction : BaseEntity
{
    public string TransCode { get; set; }

    public string FieldNames { get; set; }

    public string Action { get; set; }

    public bool HasStatement { get; set; }
}
