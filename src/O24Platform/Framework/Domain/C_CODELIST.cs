using O24OpenAPI.Core.Domain;

namespace O24OpenAPI.Framework.Domain;

public partial class C_CODELIST : BaseEntity
{
    public string CodeId { get; set; }
    public string CodeName { get; set; }
    public string CodeGroup { get; set; }
    public string Caption { get; set; }
    public string MCaption { get; set; }
    public int CodeIndex { get; set; }
    public string CodeValue { get; set; }
    public string Ftag { get; set; }
    public bool Visible { get; set; }
    public string App { get; set; }
}
