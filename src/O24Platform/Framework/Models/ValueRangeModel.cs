using O24OpenAPI.Core.Abstractions;

namespace O24OpenAPI.Framework.Models;

/// <summary>
/// The value range model class
/// </summary>
/// <seealso cref="BaseO24OpenAPIModel"/>
public class ValueRangeModel : BaseO24OpenAPIModel
{
    /// <summary>
    /// Gets or sets the value of the nullable
    /// </summary>
    public bool Nullable { get; set; } = true;

    /// <summary>
    /// Gets or sets the value of the length range
    /// </summary>
    public List<int> LengthRange { get; set; } = new List<int>(2);

    /// <summary>
    /// Gets or sets the value of the enum list
    /// </summary>
    public List<object> EnumList { get; set; } = [];

    /// <summary>
    /// Gets or sets the value of the regex
    /// </summary>
    public string Regex { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value of the decimal range
    /// </summary>
    public List<decimal> DecimalRange { get; set; } = new List<decimal>(2);
}
