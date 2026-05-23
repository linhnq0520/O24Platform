namespace O24OpenAPI.Framework.Models.UtilityModels;

/// <summary>
/// The file model class
/// </summary>
/// <seealso cref="BaseO24OpenAPIModel"/>
public partial class FileModel : BaseO24OpenAPIModel
{
    /// <summary>
    /// FileModel
    /// </summary>
    public FileModel() { }

    /// <summary>
    /// FileName
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    /// ContentType
    /// </summary>
    public string ContentType { get; set; }

    /// <summary>
    /// FileContent
    /// </summary>
    public string FileContent { get; set; }
}
