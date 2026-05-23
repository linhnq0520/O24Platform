using Newtonsoft.Json;

namespace O24OpenAPI.Framework.Models;

public class CodeListResponseModel : BaseO24OpenAPIModel
{
    public CodeListResponseModel() { }

    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("codeid")]
    public string CodeId { get; set; }

    [JsonProperty("codename")]
    public string CodeName { get; set; }

    [JsonProperty("caption")]
    public string Caption { get; set; }

    [JsonProperty("languagecaption")]
    public string MCaption { get; set; }

    [JsonProperty("codegroup")]
    public string CodeGroup { get; set; }

    [JsonProperty("codeindex")]
    public int? CodeIndex { get; set; }

    [JsonProperty("codevalue")]
    public string CodeValue { get; set; }

    [JsonProperty("ftag")]
    public string Ftag { get; set; }

    [JsonProperty("visible")]
    public bool Visible { get; set; }
}
