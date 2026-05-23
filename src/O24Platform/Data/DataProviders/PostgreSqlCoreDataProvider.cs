using System.Data.Common;
using LinqToDB.Data;
using LinqToDB.DataProvider;
using LinqToDB.DataProvider.PostgreSQL;
using LinqToDB.Mapping;
using Npgsql;
using O24OpenAPI.Core;
using O24OpenAPI.Core.Domain;
using O24OpenAPI.Data.Mapping;

namespace O24OpenAPI.Data.DataProviders;

/// <summary>
/// The postgre sql core data provider class
/// </summary>
/// <seealso cref="BaseDataProvider"/>
/// <seealso cref="IO24OpenAPIDataProvider"/>
/// <seealso cref="IMappingEntityAccessor"/>
public class PostgreSqlCoreDataProvider
    : BaseDataProvider,
        IO24OpenAPIDataProvider,
        IMappingEntityAccessor
{
    protected override IDataProvider LinqToDbDataProvider => PostgreSQLTools.GetDataProvider();

    public int SupportedLengthOfBinaryHash { get; } = 0;

    public virtual bool BackupSupported => false;

    protected static NpgsqlConnectionStringBuilder GetConnectionStringBuilder()
    {
        return new NpgsqlConnectionStringBuilder(GetCurrentConnectionString());
    }

    protected override DbConnection GetInternalDbConnection(string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentException(nameof(connectionString));
        }

        return new NpgsqlConnection(connectionString);
    }

    public void CreateDatabase(string collation, int triesToConnect = 10)
    {
        if (DatabaseExists())
        {
            return;
        }

        NpgsqlConnectionStringBuilder connectionStringBuilder = GetConnectionStringBuilder();
        string database = connectionStringBuilder.Database;
        connectionStringBuilder.Database = "postgres";

        using DbConnection dbConnection = GetInternalDbConnection(
            connectionStringBuilder.ConnectionString
        );
        dbConnection.Open();

        using DbCommand dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = $"CREATE DATABASE {EscapeName(database)}";
        dbCommand.ExecuteNonQuery();

        for (int i = 0; i <= triesToConnect; i++)
        {
            if (DatabaseExists())
            {
                return;
            }

            if (i == triesToConnect)
            {
                throw new Exception(
                    "Unable to connect to the new database. Please try one more time"
                );
            }

            Thread.Sleep(1000);
        }
    }

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

    public virtual int GetTableIdent<TEntity>()
        where TEntity : BaseEntity
    {
        using DataConnection connection = CreateDataConnection();
        string entityName = EscapeName(GetEntityDescriptor(typeof(TEntity)).EntityName);
        int? value = connection
            .Query<int?>($"SELECT COALESCE(MAX(\"Id\"), 0) FROM {entityName}")
            .FirstOrDefault();
        return value.GetValueOrDefault();
    }

    public virtual Task BackupDatabase(string fileName)
    {
        throw new NotSupportedException("PostgreSQL backup is not supported by this provider.");
    }

    public virtual Task RestoreDatabase(string backupFileName)
    {
        throw new NotSupportedException("PostgreSQL restore is not supported by this provider.");
    }

    public virtual async Task ReIndexTables()
    {
        using DataConnection currentConnection = CreateDataConnection();
        await currentConnection.ExecuteAsync("REINDEX DATABASE CURRENT_DATABASE();");
    }

    public virtual string BuildConnectionString(
        IO24OpenAPIConnectionStringInfo connectionStringInfo
    )
    {
        ArgumentNullException.ThrowIfNull(connectionStringInfo);
        if (connectionStringInfo.IntegratedSecurity)
        {
            throw new O24OpenAPIException(
                "Data provider supports connection only with login and password"
            );
        }

        NpgsqlConnectionStringBuilder builder = new()
        {
            Host = connectionStringInfo.ServerName,
            Database = connectionStringInfo.DatabaseName,
            Username = connectionStringInfo.Username,
            Password = connectionStringInfo.Password,
        };

        return builder.ConnectionString;
    }

    public virtual string CreateForeignKeyName(
        string foreignTable,
        string foreignColumn,
        string primaryTable,
        string primaryColumn
    )
    {
        return $"FK_{foreignTable}_{foreignColumn}_{primaryTable}_{primaryColumn}";
    }

    public virtual string GetIndexName(string targetTable, string targetColumn)
    {
        return $"IX_{targetTable}_{targetColumn}";
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

        await dataContext.BulkCopyAsync(
            new BulkCopyOptions { KeepIdentity = true },
            entityList,
            cancellationToken: cancellationToken
        );
        await ReseedIdentity<TEntity>(dataContext, cancellationToken);
    }

    private static async Task ReseedIdentity<TEntity>(
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
        string columnLiteral = identityColumn.ColumnName.Replace("'", "''");

        await dataContext.ExecuteAsync(
            $"""
            SELECT setval(
                pg_get_serial_sequence('{tableLiteral}', '{columnLiteral}'),
                GREATEST((SELECT COALESCE(MAX({columnName}), 1) FROM {fullTableName}), 1),
                true
            );
            """,
            cancellationToken: cancellationToken
        );
    }

    private static string EscapeName(string name)
    {
        return $"\"{name.Replace("\"", "\"\"")}\"";
    }
}
