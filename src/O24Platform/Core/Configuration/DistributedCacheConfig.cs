using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;

namespace O24OpenAPI.Core.Configuration;

/// <summary>
/// The distributed cache config class
/// </summary>
/// <seealso cref="IConfig"/>
public class DistributedCacheConfig : IConfig
{
    /// <summary>
    /// Gets or sets the value of the distributed cache type
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public DistributedCacheType DistributedCacheType { get; private set; } =
        DistributedCacheType.Redis;

    /// <summary>
    /// Gets or sets the value of the enabled
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Gets or sets the value of the connection string
    /// </summary>
    public string ConnectionString { get; set; } = "127.0.0.1:6379,ssl=False";

    /// <summary>
    /// Gets or sets the value of the schema name
    /// </summary>
    public string SchemaName { get; private set; } = "dbo";

    /// <summary>
    /// Gets or sets the value of the table name
    /// </summary>
    public string TableName { get; private set; } = "DistributedCache";
}
