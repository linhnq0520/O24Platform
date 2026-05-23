using System.Runtime.Serialization;

namespace O24OpenAPI.Core.Configuration;

/// <summary>
/// The distributed cache type enum
/// </summary>
public enum DistributedCacheType
{
    /// <summary>
    /// The memory distributed cache type
    /// </summary>
    /// [EnumMember(Value = "memory")]
    Memory,

    /// <summary>
    /// The sql server distributed cache type
    /// </summary>
    [EnumMember(Value = "sqlserver")]
    SqlServer,

    /// <summary>
    /// The redis distributed cache type
    /// </summary>
    [EnumMember(Value = "redis")]
    Redis,
}
