using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace O24OpenAPI.Framework.Models;

/// <summary>
/// The field definition model class
/// </summary>
public class FieldDefinitionModel
{
    /// <summary>
    /// Gets or sets the value of the step code
    /// </summary>
    public string StepCode { get; set; }

    /// <summary>
    /// Gets or sets the value of the field name
    /// </summary>
    public string FieldName { get; set; }

    /// <summary>
    /// Gets or sets the value of the is required
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Gets or sets the value of the data type
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public DataTypeEnum DataType { get; set; }

    /// <summary>
    /// Gets or sets the value of the value range
    /// </summary>
    public ValueRangeModel ValueRange { get; set; }
}
