using System.Text;
using System.Text.Json.Serialization;
using FluentMigrator.Runner.Initialization;
using O24OpenAPI.Core.Configuration;
using O24OpenAPI.Core.Enums;
using O24OpenAPI.Core.Helper;

namespace O24OpenAPI.Data.Configuration;

/// <summary>
/// The data config class
/// </summary>
/// <seealso cref="IConfig"/>
/// <seealso cref="IConnectionStringAccessor"/>
public class DataConfig : IConfig, IConnectionStringAccessor
{
    private string _connectionString = string.Empty;
    private string? _decryptedConnectionString;
    private readonly object _lock = new();

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public DataProviderType DataProvider { get; set; } = DataProviderType.SqlServer;
    public int? SQLCommandTimeout { get; set; } = new int?();
    private string _defaultConnectionEncrypted = string.Empty;
    private string? _defaultConnectionDecrypted = null;
    public string SchemaName { get; set; } = "dbo";

    public int LoadTimeout() => SQLCommandTimeout ?? 60;

    public string LoadSchema() => string.IsNullOrWhiteSpace(SchemaName) ? "dbo" : SchemaName;

    public string ConnectionString
    {
        get
        {
            if (_decryptedConnectionString != null)
            {
                return _decryptedConnectionString;
            }

            lock (_lock)
            {
                if (_decryptedConnectionString != null)
                {
                    return _decryptedConnectionString;
                }

                string encryptionPrivateKey = "273ece6f97dd844d";
                _decryptedConnectionString = DecryptConnectionString(
                    _connectionString,
                    encryptionPrivateKey
                );
                return _decryptedConnectionString;
            }
        }
        set => this._connectionString = value;
    }

    /// <summary>
    /// Load type
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public DbTypeEnum LoadType()
    {
        return DataProvider switch
        {
            DataProviderType.SqlServer => DbTypeEnum.sqlserver,
            DataProviderType.Oracle => DbTypeEnum.oracle,
            DataProviderType.MySql => DbTypeEnum.mysql,
            DataProviderType.PostgreSQL => DbTypeEnum.postgresql,
            _ => throw new Exception($"Unsupported provider: {DataProvider}"),
        };
    }

    /// <summary>
    /// Gets or sets the value of the default connection
    /// </summary>
    public string DefaultConnection
    {
        get
        {
            if (_defaultConnectionDecrypted == null)
            {
                string encryptionPrivateKey = "273ece6f97dd844d";
                _defaultConnectionDecrypted = DecryptConnectionString(
                    _defaultConnectionEncrypted,
                    encryptionPrivateKey
                );
            }
            return _defaultConnectionDecrypted;
        }
        set
        {
            _defaultConnectionEncrypted = value;
            _defaultConnectionDecrypted = null;
        }
    }

    /// <summary>
    /// Gets the value of the name
    /// </summary>
    [JsonIgnore]
    public string Name => "ConnectionStrings";

    /// <summary>
    /// Gets the order
    /// </summary>
    /// <returns>The int</returns>
    public int GetOrder() => 0;

    private static string DecryptConnectionString(
        string encryptedConnectionString,
        string encryptionKey
    )
    {
        string[] strArray = encryptedConnectionString.Split(";");
        StringBuilder connectionString = new();

        foreach (string part in strArray)
        {
            string key = part.Split('=')[0];
            string value = part[(part.IndexOf('=') + 1)..];

            if (key.Equals("password", StringComparison.OrdinalIgnoreCase))
            {
                value = CommonHelper.DecryptText(value, encryptionKey);
            }

            if (connectionString.Length > 0)
            {
                connectionString.Append(';');
            }

            connectionString.Append($"{key}={value}");
        }
        return connectionString.ToString();
    }
}
