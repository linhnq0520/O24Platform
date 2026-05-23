using O24OpenAPI.Core.Domain;

namespace O24OpenAPI.Framework.Models;

public abstract partial class BaseGLConfig : BaseEntity
{
    public string ConfigurationGroup { get; set; }

    public string TransCode { get; set; }

    public bool GenerateDebitEntry { get; set; }

    public bool GenerateCreditEntry { get; set; }

    public string? DebitSysAccountName { get; set; }

    public string? CreditSysAccountName { get; set; }
}
