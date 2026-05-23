using System.Runtime.Serialization;

namespace O24OpenAPI.Framework.Models;

/// <summary>
/// The data type enum enum
/// </summary>
public enum DataTypeEnum
{
    /// <summary>
    /// The number data type enum
    /// </summary>
    [EnumMember(Value = "Number")]
    Number,

    /// <summary>
    /// The string data type enum
    /// </summary>
    [EnumMember(Value = "String")]
    String,

    /// <summary>
    /// The boolean data type enum
    /// </summary>
    [EnumMember(Value = "Boolean")]
    Boolean,
}
