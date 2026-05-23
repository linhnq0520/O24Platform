using System.Runtime.Serialization;

namespace O24OpenAPI.Data;

/// <summary>
/// The data provider type enum
/// </summary>
public enum DataProviderType
{
    /// <summary>
    /// The unknown data provider type
    /// </summary>
    [EnumMember(Value = "")]
    Unknown,

    /// <summary>
    /// The sql server data provider type
    /// </summary>
    [EnumMember(Value = "sqlserver")]
    SqlServer,

    /// <summary>
    /// The my sql data provider type
    /// </summary>
    [EnumMember(Value = "mysql")]
    MySql,

    /// <summary>
    /// The postgre sql data provider type
    /// </summary>
    [EnumMember(Value = "postgresql")]
    PostgreSQL,

    /// <summary>
    /// The oracle data provider type
    /// </summary>
    [EnumMember(Value = "oracle")]
    Oracle,
}
