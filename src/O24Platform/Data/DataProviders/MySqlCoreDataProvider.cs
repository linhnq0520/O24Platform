using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.DataProvider;
using LinqToDB.DataProvider.MySql;
using LinqToDB.Mapping;
using LinqToDB.SqlQuery;
using LinqToDB.Tools;
using MySqlConnector;
using O24OpenAPI.Core;
using O24OpenAPI.Core.Domain;
using O24OpenAPI.Core.Helper;
using O24OpenAPI.Data.Mapping;

namespace O24OpenAPI.Data.DataProviders;

/// <summary>
/// The my sql core data provider class
/// </summary>
/// <seealso cref="BaseDataProvider"/>
/// <seealso cref="IO24OpenAPIDataProvider"/>
/// <seealso cref="IMappingEntityAccessor"/>
public class MySqlCoreDataProvider
    : BaseDataProvider,
        IO24OpenAPIDataProvider,
        IMappingEntityAccessor
{
    /// <summary>
    /// The hash algorithm
    /// </summary>
    private const string HASH_ALGORITHM = "SHA1";

    /// <summary>
    /// Creates the data connection
    /// </summary>
    /// <returns>The data connection</returns>
    public override DataConnection CreateDataConnection()
    {
        DataConnection dataConnection = this.CreateDataConnection(this.LinqToDbDataProvider);
        dataConnection.MappingSchema.SetDataType(
            typeof(Guid),
            new SqlDataType(DataType.NChar, typeof(Guid), 36)
        );
        dataConnection.MappingSchema.SetConvertExpression<string, Guid>(
            (Expression<Func<string, Guid>>)(strGuid => new Guid(strGuid))
        );
        return dataConnection;
    }

    /// <summary>
    /// Gets the connection string builder
    /// </summary>
    /// <returns>The my sql connection string builder</returns>
    protected static MySqlConnectionStringBuilder GetConnectionStringBuilder()
    {
        return new MySqlConnectionStringBuilder(GetCurrentConnectionString());
    }

    /// <summary>
    /// Gets the internal db connection using the specified connection string
    /// </summary>
    /// <param name="connectionString">The connection string</param>
    /// <exception cref="ArgumentException"></exception>
    /// <returns>The db connection</returns>
    protected override DbConnection GetInternalDbConnection(string connectionString)
    {
        return !string.IsNullOrEmpty(connectionString)
            ? (DbConnection)new MySqlConnector.MySqlConnection(connectionString)
            : throw new ArgumentException(nameof(connectionString));
    }

    /// <summary>
    /// Creates the database using the specified collation
    /// </summary>
    /// <param name="collation">The collation</param>
    /// <param name="triesToConnect">The tries to connect</param>
    /// <exception cref="Exception">Unable to connect to the new database. Please try one more time</exception>
    public void CreateDatabase(string collation, int triesToConnect = 10)
    {
        if (this.DatabaseExists())
        {
            return;
        }

        MySqlConnectionStringBuilder connectionStringBuilder = GetConnectionStringBuilder();
        string database = connectionStringBuilder.Database;
        connectionStringBuilder.Database = null;
        using (
            DbConnection internalDbConnection = this.GetInternalDbConnection(
                connectionStringBuilder.ConnectionString
            )
        )
        {
            string str = "CREATE DATABASE IF NOT EXISTS " + database;
            if (!string.IsNullOrWhiteSpace(collation))
            {
                str = str + " COLLATE " + collation;
            }

            DbCommand command = internalDbConnection.CreateCommand();
            command.CommandText = str;
            command.Connection.Open();
            command.ExecuteNonQuery();
        }
        if (triesToConnect <= 0)
        {
            return;
        }

        for (int index = 0; index <= triesToConnect; ++index)
        {
            if (index == triesToConnect)
            {
                throw new Exception(
                    "Unable to connect to the new database. Please try one more time"
                );
            }

            if (this.DatabaseExists())
            {
                break;
            }

            Thread.Sleep(1000);
        }
    }

    /// <summary>
    /// Databases the exists
    /// </summary>
    /// <returns>The bool</returns>
    public bool DatabaseExists()
    {
        try
        {
            using (
                DbConnection internalDbConnection = this.GetInternalDbConnection(
                    GetCurrentConnectionString()
                )
            )
            {
                internalDbConnection.Open();
                return true;
            }
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
        using (DataConnection dataConnection = this.CreateDataConnection())
        {
            string entityName = this.GetEntityDescriptor(typeof(TEntity)).EntityName;
            string database = dataConnection.Connection.Database;
            using (
                DbConnection internalDbConnection = this.GetInternalDbConnection(
                    GetCurrentConnectionString()
                )
            )
            {
                internalDbConnection.StateChange += (sender, e) =>
                {
                    try
                    {
                        if (e.CurrentState != ConnectionState.Open)
                        {
                            return;
                        }

                        IDbConnection dbConnection = (IDbConnection)sender;
                        using (IDbCommand command = dbConnection.CreateCommand())
                        {
                            command.Connection = dbConnection;
                            command.CommandText =
                                "SET @SESSION.information_schema_stats_expiry = 0;";
                            command.ExecuteNonQuery();
                        }
                    }
                    catch (MySqlException ex) when (ex.Number == 1193) { }
                };
                using (DbCommand command = internalDbConnection.CreateCommand())
                {
                    command.Connection = internalDbConnection;
                    DbCommand dbCommand = command;
                    DefaultInterpolatedStringHandler interpolatedStringHandler = new(96, 2);
                    interpolatedStringHandler.AppendLiteral(
                        "SELECT AUTO_INCREMENT FROM information_schema.TABLES WHERE TABLE_SCHEMA = '"
                    );
                    interpolatedStringHandler.AppendFormatted(database);
                    interpolatedStringHandler.AppendLiteral("' AND TABLE_NAME = '");
                    interpolatedStringHandler.AppendFormatted(entityName);
                    interpolatedStringHandler.AppendLiteral("'");
                    string stringAndClear = interpolatedStringHandler.ToStringAndClear();
                    dbCommand.CommandText = stringAndClear;
                    internalDbConnection.Open();
                    return Convert.ToInt32(command.ExecuteScalar() ?? 1);
                }
            }
        }
    }

    /// <summary>
    /// Backups the database using the specified file name
    /// </summary>
    /// <param name="fileName">The file name</param>
    /// <exception cref="DataException">This database provider does not support backup</exception>
    public virtual Task BackupDatabase(string fileName)
    {
        throw new DataException("This database provider does not support backup");
    }

    /// <summary>
    /// Restores the database using the specified backup file name
    /// </summary>
    /// <param name="backupFileName">The backup file name</param>
    /// <exception cref="DataException">This database provider does not support backup</exception>
    public virtual Task RestoreDatabase(string backupFileName)
    {
        throw new DataException("This database provider does not support backup");
    }

    /// <summary>
    /// Res the index tables
    /// </summary>
    public virtual async Task ReIndexTables()
    {
        DataConnection currentConnection = this.CreateDataConnection();
        try
        {
            List<string> tables = currentConnection
                .Query<string>("SHOW TABLES FROM `" + currentConnection.Connection.Database + "`")
                .ToList<string>();
            if (tables.Count <= 0)
            {
                currentConnection = null;
                tables = null;
            }
            else
            {
                int num = await currentConnection.ExecuteAsync(
                    "OPTIMIZE TABLE `" + string.Join("`, `", (IEnumerable<string>)tables) + "`"
                );
                currentConnection = null;
                tables = null;
            }
        }
        finally
        {
            currentConnection?.Dispose();
        }
    }

    /// <summary>
    /// Builds the connection string using the specified neptune connection string
    /// </summary>
    /// <param name="neptuneConnectionString">The neptune connection string</param>
    /// <exception cref="O24OpenAPIException">Data provider supports connection only with login and password</exception>
    /// <returns>The string</returns>
    public virtual string BuildConnectionString(
        IO24OpenAPIConnectionStringInfo neptuneConnectionString
    )
    {
        ArgumentNullException.ThrowIfNull(neptuneConnectionString);
        if (neptuneConnectionString.IntegratedSecurity)
        {
            throw new O24OpenAPIException(
                "Data provider supports connection only with login and password"
            );
        }

        return new MySqlConnectionStringBuilder()
        {
            Server = neptuneConnectionString.ServerName,
            Database = neptuneConnectionString.DatabaseName.ToLower(),
            AllowUserVariables = true,
            UserID = neptuneConnectionString.Username,
            Password = neptuneConnectionString.Password,
            Port = neptuneConnectionString.Port,
        }.ConnectionString;
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
        Encoding utF8 = Encoding.UTF8;
        DefaultInterpolatedStringHandler interpolatedStringHandler = new(3, 4);
        interpolatedStringHandler.AppendFormatted(foreignTable);
        interpolatedStringHandler.AppendLiteral("_");
        interpolatedStringHandler.AppendFormatted(foreignColumn);
        interpolatedStringHandler.AppendLiteral("_");
        interpolatedStringHandler.AppendFormatted(primaryTable);
        interpolatedStringHandler.AppendLiteral("_");
        interpolatedStringHandler.AppendFormatted(primaryColumn);
        string stringAndClear = interpolatedStringHandler.ToStringAndClear();
        return "FK_" + HashHelper.CreateHash(utF8.GetBytes(stringAndClear), "SHA1");
    }

    /// <summary>
    /// Gets the index name using the specified target table
    /// </summary>
    /// <param name="targetTable">The target table</param>
    /// <param name="targetColumn">The target column</param>
    /// <returns>The string</returns>
    public virtual string GetIndexName(string targetTable, string targetColumn)
    {
        return "IX_"
            + HashHelper.CreateHash(
                Encoding.UTF8.GetBytes(targetTable + "_" + targetColumn),
                "SHA1"
            );
    }

    /// <summary>
    /// Gets the table
    /// </summary>
    /// <typeparam name="TEntity">The entity</typeparam>
    /// <exception cref="NotImplementedException"></exception>
    /// <returns>A queryable of t entity</returns>
    public override IQueryable<TEntity> GetTable<TEntity>()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Executes the non query using the specified sql
    /// </summary>
    /// <param name="sql">The sql</param>
    /// <param name="dataParameters">The data parameters</param>
    /// <exception cref="NotImplementedException"></exception>
    /// <returns>A task containing the int</returns>
    public override Task<int> ExecuteNonQuery(string sql, params DataParameter[] dataParameters)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Queries the proc using the specified procedure name
    /// </summary>
    /// <typeparam name="T">The </typeparam>
    /// <param name="procedureName">The procedure name</param>
    /// <param name="parameters">The parameters</param>
    /// <exception cref="NotImplementedException"></exception>
    /// <returns>A task containing a list of t</returns>
    public override Task<IList<T>> QueryProc<T>(
        string procedureName,
        params DataParameter[] parameters
    )
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Executes the proc using the specified procedure name
    /// </summary>
    /// <param name="procedureName">The procedure name</param>
    /// <param name="parameters">The parameters</param>
    /// <exception cref="NotImplementedException"></exception>
    /// <returns>A task containing the int</returns>
    public override Task<int> ExecuteProc(string procedureName, params DataParameter[] parameters)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Queries the sql
    /// </summary>
    /// <typeparam name="T">The </typeparam>
    /// <param name="sql">The sql</param>
    /// <param name="parameters">The parameters</param>
    /// <exception cref="NotImplementedException"></exception>
    /// <returns>A task containing a list of t</returns>
    public override Task<IList<T>> Query<T>(string sql, params DataParameter[] parameters)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Truncates the reset identity
    /// </summary>
    /// <typeparam name="TEntity">The entity</typeparam>
    /// <param name="resetIdentity">The reset identity</param>
    /// <exception cref="NotImplementedException"></exception>
    public override Task Truncate<TEntity>(bool resetIdentity = false)
    {
        return base.Truncate<TEntity>(resetIdentity);
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
        int nextId = await dataContext.ExecuteAsync<int>(
            $"SELECT COALESCE(MAX({columnName}), 0) + 1 FROM {fullTableName}",
            cancellationToken: cancellationToken
        );

        await dataContext.ExecuteAsync(
            $"ALTER TABLE {fullTableName} AUTO_INCREMENT = {nextId}",
            cancellationToken: cancellationToken
        );
    }

    private static string EscapeName(string name)
    {
        return $"`{name.Replace("`", "``")}`";
    }

    /// <summary>
    /// Gets the value of the linq to db data provider
    /// </summary>
    protected override IDataProvider LinqToDbDataProvider
    {
        get => MySqlTools.GetDataProvider();
    }

    /// <summary>
    /// Gets the value of the supported length of binary hash
    /// </summary>
    public int SupportedLengthOfBinaryHash { get; } = 0;

    /// <summary>
    /// Gets the value of the backup supported
    /// </summary>
    public virtual bool BackupSupported => false;
}
