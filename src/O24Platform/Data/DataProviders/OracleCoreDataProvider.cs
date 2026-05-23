using System.Data;
using System.Data.Common;
using System.Text;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.DataProvider;
using LinqToDB.DataProvider.Oracle;
using O24OpenAPI.Core;
using O24OpenAPI.Core.Domain;
using O24OpenAPI.Core.Helper;
using O24OpenAPI.Data.Mapping;
using Oracle.ManagedDataAccess.Client;

namespace O24OpenAPI.Data.DataProviders;

/// <summary>
/// The oracle core data provider class
/// </summary>
/// <seealso cref="BaseDataProvider"/>
/// <seealso cref="IO24OpenAPIDataProvider"/>
/// <seealso cref="IMappingEntityAccessor"/>
public class OracleCoreDataProvider
    : BaseDataProvider,
        IO24OpenAPIDataProvider,
        IMappingEntityAccessor
{
    private const string HASH_ALGORITHM = "SHA1";

    protected override IDataProvider LinqToDbDataProvider =>
        OracleTools.GetDataProvider(OracleVersion.v12, OracleProvider.Managed);

    public int SupportedLengthOfBinaryHash { get; } = 0;

    public virtual bool BackupSupported => false;

    /// <summary>
    /// Creates the data connection
    /// </summary>
    /// <returns>The data connection</returns>
    public override DataConnection CreateDataConnection()
    {
        return CreateDataConnection(LinqToDbDataProvider);
    }

    /// <summary>
    /// Gets the connection string builder
    /// </summary>
    /// <returns>The oracle connection string builder</returns>
    protected static OracleConnectionStringBuilder GetConnectionStringBuilder()
    {
        return new OracleConnectionStringBuilder(GetCurrentConnectionString());
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
        return new OracleConnection(connectionString);
    }

    /// <summary>
    /// Gets the sequence name using the specified data connection
    /// </summary>
    /// <typeparam name="TEntity">The entity</typeparam>
    /// <param name="dataConnection">The data connection</param>
    /// <exception cref="O24OpenAPIException"></exception>
    /// <exception cref="O24OpenAPIException">A table's primary key does not have an identity constraint</exception>
    /// <exception cref="ArgumentNullException">dataConnection</exception>
    /// <returns>The string</returns>
    private string GetSequenceName<TEntity>(DataConnection dataConnection)
        where TEntity : BaseEntity
    {
        ArgumentNullException.ThrowIfNull(dataConnection);
        O24OpenAPIEntityDescriptor entityDescriptor = GetEntityDescriptor(typeof(TEntity));
        if (entityDescriptor == null)
        {
            throw new O24OpenAPIException(
                "Mapped entity descriptor is not found: " + typeof(TEntity).Name
            );
        }
        string entityName = entityDescriptor.EntityName;
        string text = entityDescriptor
            .Fields.FirstOrDefault(
                (O24OpenAPIEntityFieldDescriptor x) => x.IsIdentity && x.IsPrimaryKey
            )
            ?.Name;
        if (string.IsNullOrEmpty(text))
        {
            throw new O24OpenAPIException(
                "A table's primary key does not have an identity constraint"
            );
        }
        string sql =
            $"\n                ECLARE\n                  VAR_SEQUENCE_EXIST VARCHAR(1);\n                BEGIN\n\n                  SELECT COUNT(1)\n                    INTO VAR_SEQUENCE_EXIST\n                    FROM USER_SEQUENCES\n                   WHERE SEQUENCE_NAME = '{entityName}_{text}_SEQ';\n\n                  IF VAR_SEQUENCE_EXIST = 0 THEN       \n                    -- Create sequence \n                    EXECUTE IMMEDIATE 'create sequence {entityName}_{text}_SEQ\n                                        minvalue 1\n                                        maxvalue 9999999999\n                                        start with 1\n                                        increment by 1\n                                        nocache';\n                  END IF;\n                END;";
        dataConnection.Execute(sql);
        return entityName + "_" + text + "_SEQ";
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
        OracleConnectionStringBuilder connectionStringBuilder = GetConnectionStringBuilder();
        string userID = connectionStringBuilder.UserID;
        using (
            DbConnection dbConnection = GetInternalDbConnection(
                connectionStringBuilder.ConnectionString
            )
        )
        {
            dbConnection.Open();
            string commandText =
                $"CREATE USER \"{connectionStringBuilder.UserID}\" IDENTIFIED BY \"{connectionStringBuilder.Password}\" DEFAULT TABLESPACE \"USERS\" TEMPORARY TABLESPACE \"TEMP\"";
            DbCommand dbCommand = dbConnection.CreateCommand();
            dbCommand.CommandText = commandText;
            dbCommand.ExecuteNonQuery();
            commandText = "GRANT ALL PRIVILEGES TO \"" + connectionStringBuilder.UserID + "\"";
            dbCommand.CommandText = commandText;
            dbCommand.ExecuteNonQuery();
            commandText =
                "GRANT CONNECT, CREATE SESSION, DBA, RESOURCE to \""
                + connectionStringBuilder.UserID
                + "\"";
            dbCommand.CommandText = commandText;
            dbCommand.ExecuteNonQuery();
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
        using DataConnection dataConnection = CreateDataConnection();
        string sequenceName = GetSequenceName<TEntity>(dataConnection);
        return dataConnection
            .Query<int>("SELECT " + sequenceName + ".NEXTVAL FROM DUAL;")
            .FirstOrDefault();
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
        using DataConnection currentConnection = CreateDataConnection();
        await currentConnection.ExecuteAsync(
            "REINDEX DATABASE \"" + currentConnection.Connection.Database + "\";"
        );
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

        foreach (TEntity entity in entityList)
        {
            entity.CreatedOnUtc = DateTime.UtcNow;
            entity.UpdatedOnUtc = DateTime.UtcNow;
            await dataContext.InsertWithInt32IdentityAsync(entity, token: cancellationToken);
        }
    }

    private async Task ReseedIdentity<TEntity>(
        DataConnection dataContext,
        CancellationToken cancellationToken = default
    )
        where TEntity : BaseEntity
    {
        O24OpenAPIEntityDescriptor entityDescriptor = GetEntityDescriptor(typeof(TEntity));
        O24OpenAPIEntityFieldDescriptor? identityField = entityDescriptor.Fields.FirstOrDefault(x =>
            x.IsIdentity && x.IsPrimaryKey
        );
        if (identityField == null)
        {
            return;
        }

        string tableName = EscapeName(entityDescriptor.EntityName);
        string columnName = EscapeName(identityField.Name);
        string? schemaName = string.IsNullOrWhiteSpace(entityDescriptor.SchemaName)
            ? null
            : EscapeName(entityDescriptor.SchemaName);
        string fullTableName = schemaName == null ? tableName : $"{schemaName}.{tableName}";
        string sequenceName = EscapeName($"{entityDescriptor.EntityName}_{identityField.Name}_SEQ");
        string sequenceLiteral = $"{entityDescriptor.EntityName}_{identityField.Name}_SEQ".Replace(
            "'",
            "''"
        );

        await dataContext.ExecuteAsync(
            $"""
            DECLARE
                v_max_id NUMBER;
                v_current_value NUMBER;
                v_increment_by NUMBER;
                v_sequence_count NUMBER;
            BEGIN
                SELECT NVL(MAX({columnName}), 0) + 1 INTO v_max_id FROM {fullTableName};

                SELECT COUNT(1)
                  INTO v_sequence_count
                  FROM USER_SEQUENCES
                 WHERE SEQUENCE_NAME = UPPER('{sequenceLiteral}');

                IF v_sequence_count = 0 THEN
                    EXECUTE IMMEDIATE 'CREATE SEQUENCE {sequenceName} MINVALUE 1 MAXVALUE 9999999999 START WITH ' || v_max_id || ' INCREMENT BY 1 NOCACHE';
                ELSE
                    SELECT LAST_NUMBER
                      INTO v_current_value
                      FROM USER_SEQUENCES
                     WHERE SEQUENCE_NAME = UPPER('{sequenceLiteral}');

                    v_increment_by := v_max_id - v_current_value;
                    IF v_increment_by != 0 THEN
                        EXECUTE IMMEDIATE 'ALTER SEQUENCE {sequenceName} INCREMENT BY ' || v_increment_by;
                        EXECUTE IMMEDIATE 'SELECT {sequenceName}.NEXTVAL FROM DUAL' INTO v_current_value;
                        EXECUTE IMMEDIATE 'ALTER SEQUENCE {sequenceName} INCREMENT BY 1';
                    END IF;
                END IF;
            END;
            """,
            cancellationToken: cancellationToken
        );
    }

    private static string EscapeName(string name)
    {
        return $"\"{name.Replace("\"", "\"\"").ToUpperInvariant()}\"";
    }

    /// <summary>
    /// Builds the connection string using the specified o 9 connection string
    /// </summary>
    /// <param name="o9ConnectionString">The connection string</param>
    /// <exception cref="O24OpenAPIException">Data provider supports connection only with login and password</exception>
    /// <exception cref="ArgumentNullException">o9ConnectionString</exception>
    /// <returns>The string</returns>
    public virtual string BuildConnectionString(IO24OpenAPIConnectionStringInfo o9ConnectionString)
    {
        ArgumentNullException.ThrowIfNull(o9ConnectionString);
        if (o9ConnectionString.IntegratedSecurity)
        {
            throw new O24OpenAPIException(
                "Data provider supports connection only with login and password"
            );
        }
        OracleConnectionStringBuilder oracleConnectionStringBuilder =
            new OracleConnectionStringBuilder
            {
                UserID = o9ConnectionString.Username,
                Password = o9ConnectionString.Password,
                DataSource = o9ConnectionString.ServerName,
            };
        return oracleConnectionStringBuilder.ConnectionString;
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
        return "FK_"
            + HashHelper.CreateHash(
                Encoding.UTF8.GetBytes(
                    $"{foreignTable}_{foreignColumn}_{primaryTable}_{primaryColumn}"
                ),
                "SHA1"
            );
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
}
