using System.Data.Common;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.DataProvider;
using LinqToDB.DataProvider.SqlServer;
using LinqToDB.Mapping;
using LinqToDB.Tools;
using Microsoft.Data.SqlClient;
using O24OpenAPI.Core.Domain;
using O24OpenAPI.Data.Configuration;
using O24OpenAPI.Data.Mapping;

namespace O24OpenAPI.Data.DataProviders;

/// <summary>
/// The ms sql core data provider class
/// </summary>
/// <seealso cref="BaseDataProvider"/>
/// <seealso cref="IO24OpenAPIDataProvider"/>
/// <seealso cref="IMappingEntityAccessor"/>
public class MsSqlCoreDataProvider
    : BaseDataProvider,
        IO24OpenAPIDataProvider,
        IMappingEntityAccessor
{
    /// <summary>
    /// Gets the value of the linq to db data provider
    /// </summary>
    protected override IDataProvider LinqToDbDataProvider =>
        SqlServerTools.GetDataProvider(
            SqlServerVersion.v2012,
            SqlServerProvider.MicrosoftDataSqlClient
        );

    /// <summary>
    /// Gets the value of the supported length of binary hash
    /// </summary>
    public int SupportedLengthOfBinaryHash { get; } = 8000;

    /// <summary>
    /// Gets the value of the backup supported
    /// </summary>
    public virtual bool BackupSupported => true;

    /// <summary>
    /// Gets the connection string builder
    /// </summary>
    /// <returns>The sql connection string builder</returns>
    protected virtual SqlConnectionStringBuilder GetConnectionStringBuilder()
    {
        string connectionString = DataSettingsManager.LoadSettings().ConnectionString;
        return new SqlConnectionStringBuilder(connectionString);
    }

    /// <summary>
    /// Gets the internal db connection using the specified connection string
    /// </summary>
    /// <param name="connectionString">The connection string</param>
    /// <exception cref="ArgumentException">connectionString</exception>
    /// <returns>The db connection</returns>
    protected override DbConnection GetInternalDbConnection(string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentException("connectionString");
        }

        return new SqlConnection(connectionString);
    }

    /// <summary>
    /// Creates the database using the specified collation
    /// </summary>
    /// <param name="collation">The collation</param>
    /// <param name="triesToConnect">The tries to connect</param>
    /// <exception cref="Exception">Unable to connect to the new database. Please try one more time</exception>
    public void CreateDatabase(string collation, int triesToConnect = 10)
    {
        if (DatabaseExists())
        {
            return;
        }

        SqlConnectionStringBuilder connectionStringBuilder = GetConnectionStringBuilder();
        string initialCatalog = connectionStringBuilder.InitialCatalog;
        connectionStringBuilder.InitialCatalog = "master";
        using (
            DbConnection dbConnection = GetInternalDbConnection(
                connectionStringBuilder.ConnectionString
            )
        )
        {
            string text = "CREATE DATABASE [" + initialCatalog + "]";
            if (!string.IsNullOrWhiteSpace(collation))
            {
                text = text + " COLLATE " + collation;
            }

            DbCommand dbCommand = dbConnection.CreateCommand();
            dbCommand.CommandText = text;
            dbCommand?.Connection?.Open();
            dbCommand?.ExecuteNonQuery();
        }

        if (triesToConnect <= 0)
        {
            return;
        }

        for (int i = 0; i <= triesToConnect; i++)
        {
            if (i == triesToConnect)
            {
                throw new Exception(
                    "Unable to connect to the new database. Please try one more time"
                );
            }

            if (!DatabaseExists())
            {
                Thread.Sleep(1000);
                continue;
            }

            break;
        }
    }

    /// <summary>
    /// Describes whether this instance database exists
    /// </summary>
    /// <returns>The bool</returns>
    public bool DatabaseExists()
    {
        try
        {
            using DbConnection dbConnection = GetInternalDbConnection(GetCurrentConnectionString());
            dbConnection.Open();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets the table ident
    /// </summary>
    /// <typeparam name="TEntity">The entity</typeparam>
    /// <returns>The int</returns>
    public virtual int GetTableIdent<TEntity>()
        where TEntity : BaseEntity
    {
        using DataConnection connection = CreateDataConnection();
        string entityName = GetEntityDescriptor(typeof(TEntity)).EntityName;
        decimal? num = connection
            .Query<decimal?>("SELECT IDENT_CURRENT('[" + entityName + "]') as Value")
            .FirstOrDefault();
        return (!num.HasValue) ? 1 : Convert.ToInt32(num);
    }

    public override async Task BulkInsertEntities<TEntity>(
        IEnumerable<TEntity> entities,
        bool preserveIdentity = false,
        CancellationToken cancellationToken = default
    )
    {
        using DataConnection dataContext = CreateDataConnection(LinqToDbDataProvider);
        List<TEntity> entityList = entities.ToList();
        if (entityList.Count == 0)
        {
            return;
        }

        if (preserveIdentity)
        {
            await dataContext.BulkCopyAsync(
                new BulkCopyOptions { KeepIdentity = true },
                entityList,
                cancellationToken: cancellationToken
            );
            await ReseedIdentity<TEntity>(dataContext, cancellationToken);
            return;
        }

        await dataContext.BulkCopyAsync(
            new BulkCopyOptions(),
            entityList.RetrieveIdentity(dataContext),
            cancellationToken: cancellationToken
        );
    }

    private async Task ReseedIdentity<TEntity>(
        DataConnection dataContext,
        CancellationToken cancellationToken = default
    )
    {
        EntityDescriptor descriptor = dataContext.MappingSchema.GetEntityDescriptor(
            typeof(TEntity)
        );
        ColumnDescriptor? identityColumn = descriptor.Columns.FirstOrDefault(c => c.IsIdentity);
        if (identityColumn == null)
        {
            return;
        }

        string rawTableName = descriptor.Name.Name;
        string? rawSchemaName = descriptor.Name.Schema;
        string tableName = EscapeName(rawTableName);
        string columnName = EscapeName(identityColumn.ColumnName);
        string? schemaName = string.IsNullOrWhiteSpace(rawSchemaName)
            ? null
            : EscapeName(rawSchemaName);
        string fullTableName = schemaName == null ? tableName : $"{schemaName}.{tableName}";
        string tableLiteral = (
            schemaName == null ? rawTableName : $"{rawSchemaName}.{rawTableName}"
        ).Replace("'", "''");

        string reseedSql = $"""
            DECLARE @maxId bigint;
            SELECT @maxId = ISNULL(MAX({columnName}), 0) FROM {fullTableName};
            DBCC CHECKIDENT ('{tableLiteral}', RESEED, @maxId);
            """;

        string reseedConnectionString = GetReseedConnectionString();
        if (string.IsNullOrWhiteSpace(reseedConnectionString))
        {
            await dataContext.ExecuteAsync(reseedSql, cancellationToken: cancellationToken);
            return;
        }

        using DataConnection reseedDataContext = CreateDataConnection(reseedConnectionString);
        await reseedDataContext.ExecuteAsync(reseedSql, cancellationToken: cancellationToken);
    }

    private DataConnection CreateDataConnection(string connectionString)
    {
        DataOptions dataOptions = new DataOptions()
            .UseConnection(LinqToDbDataProvider, CreateDbConnection(connectionString))
            .UseMappingSchema(GetMappingSchema())
            .UseCommandTimeout(DataSettingsManager.GetSqlCommandTimeout());

        return new DataConnection(dataOptions);
    }

    private static string GetReseedConnectionString()
    {
        DataConfig dataConfig = DataSettingsManager.LoadSettings();
        if (string.IsNullOrWhiteSpace(dataConfig.DefaultConnection))
        {
            return string.Empty;
        }

        SqlConnectionStringBuilder adminBuilder = new(dataConfig.DefaultConnection);
        SqlConnectionStringBuilder appBuilder = new(dataConfig.ConnectionString);
        if (!string.IsNullOrWhiteSpace(appBuilder.InitialCatalog))
        {
            adminBuilder.InitialCatalog = appBuilder.InitialCatalog;
        }

        return adminBuilder.ConnectionString;
    }

    private static string EscapeName(string name)
    {
        return $"[{name.Replace("]", "]]")}]";
    }

    /// <summary>
    /// Backups the database using the specified file name
    /// </summary>
    /// <param name="fileName">The file name</param>
    public virtual async Task BackupDatabase(string fileName)
    {
        using DataConnection currentConnection = CreateDataConnection();
        string commandText =
            $"BACKUP DATABASE [{currentConnection.Connection.Database}] TO DISK = '{fileName}' WITH FORMAT";
        await currentConnection.ExecuteAsync(commandText);
    }

    /// <summary>
    /// Restores the database using the specified backup file name
    /// </summary>
    /// <param name="backupFileName">The backup file name</param>
    public virtual async Task RestoreDatabase(string backupFileName)
    {
        using DataConnection currentConnection = CreateDataConnection();
        string commandText = string.Format(
            "DECLARE @ErrorMessage NVARCHAR(4000)\nALTER DATABASE [{0}] SET OFFLINE WITH ROLLBACK IMMEDIATE\nBEGIN TRY\nRESTORE DATABASE [{0}] FROM DISK = '{1}' WITH REPLACE\nEND TRY\nBEGIN CATCH\nSET @ErrorMessage = ERROR_MESSAGE()\nEND CATCH\nALTER DATABASE [{0}] SET MULTI_USER WITH ROLLBACK IMMEDIATE\nIF (@ErrorMessage is not NULL)\nBEGIN\nRAISEERROR (@ErrorMessage, 16, 1)\nEND",
            currentConnection.Connection.Database,
            backupFileName
        );
        await currentConnection.ExecuteAsync(commandText);
    }

    /// <summary>
    /// Res the index tables
    /// </summary>
    public virtual async Task ReIndexTables()
    {
        using DataConnection currentConnection = CreateDataConnection();
        string commandText =
            "\n\t\t\tDECLARE @TableName sysname\n\t\t\tDECLARE cur_reindex CURSOR FOR\n\t\t\tSELECT table_name\n\t\t\tFROM ["
            + currentConnection.Connection.Database
            + "].information_schema.tables\n\t\t\tWHERE table_type = 'base table'\n\t\t\tOPEN cur_reindex\n\t\t\tFETCH NEXT FROM cur_reindex INTO @TableName\n\t\t\tWHILE @@FETCH_STATUS = 0\n\t\t\t\tBEGIN\n\t\t\t\t\texec('ALTER INDEX ALL ON [' + @TableName + '] REBUILD')\n\t\t\t\t\tFETCH NEXT FROM cur_reindex INTO @TableName\n\t\t\t\tEND\n\t\t\tCLOSE cur_reindex\n\t\t\tDEALLOCATE cur_reindex";
        await currentConnection.ExecuteAsync(commandText);
    }

    /// <summary>
    /// Builds the connection string using the specified neptune connection string
    /// </summary>
    /// <param name="neptuneConnectionString">The neptune connection string</param>
    /// <exception cref="ArgumentNullException">neptuneConnectionString</exception>
    /// <returns>The string</returns>
    public virtual string BuildConnectionString(
        IO24OpenAPIConnectionStringInfo neptuneConnectionString
    )
    {
        ArgumentNullException.ThrowIfNull(neptuneConnectionString);

        SqlConnectionStringBuilder sqlConnectionStringBuilder = new()
        {
            DataSource = neptuneConnectionString.ServerName,
            InitialCatalog = neptuneConnectionString.DatabaseName,
            PersistSecurityInfo = false,
            IntegratedSecurity = neptuneConnectionString.IntegratedSecurity,
        };
        if (!neptuneConnectionString.IntegratedSecurity)
        {
            sqlConnectionStringBuilder.UserID = neptuneConnectionString.Username;
            sqlConnectionStringBuilder.Password = neptuneConnectionString.Password;
        }

        return sqlConnectionStringBuilder.ConnectionString;
    }

    /// <summary>
    /// Creates the foreign key name using the specified foreign table
    /// </summary>
    /// <param name="foreignTable">The foreign table</param>
    /// <param name="foreignColumn">The foreign column</param>
    /// <param name="primaryTable">The primary table</param>
    /// <param name="primaryColumn">The primary column</param>
    /// <returns>The string</returns>
    public virtual string CreateForeignKeyName(
        string foreignTable,
        string foreignColumn,
        string primaryTable,
        string primaryColumn
    )
    {
        return $"FK_{foreignTable}_{foreignColumn}_{primaryTable}_{primaryColumn}";
    }

    /// <summary>
    /// Gets the index name using the specified target table
    /// </summary>
    /// <param name="targetTable">The target table</param>
    /// <param name="targetColumn">The target column</param>
    /// <returns>The string</returns>
    public virtual string GetIndexName(string targetTable, string targetColumn)
    {
        return "IX_" + targetTable + "_" + targetColumn;
    }
}
