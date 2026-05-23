using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using LinKit.Json.Runtime;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using Newtonsoft.Json;
using O24OpenAPI.Client.Scheme.Workflow;
using O24OpenAPI.Core.Enums;
using O24OpenAPI.Core.Extensions;
using O24OpenAPI.Core.Infrastructure;
using O24OpenAPI.Data;
using O24OpenAPI.Framework.Constants;
using O24OpenAPI.Framework.Domain;
using O24OpenAPI.Framework.Models;
using O24OpenAPI.Framework.Services.Logging;
using Oracle.ManagedDataAccess.Client;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace O24OpenAPI.Framework.DBContext;

/// <summary>
/// The service db context class
/// </summary>
/// <seealso cref="DbContext"/>
public class ServiceDBContext : DbContext
{
    /// <summary>
    /// Gets or sets the value of the connection string
    /// </summary>
    public string ConnectionString { get; set; } = "";

    /// <summary>
    /// Gets or sets the value of the schema name
    /// </summary>
    public string SchemaName { get; set; } = "";

    /// <summary>
    /// Gets or sets the value of the rdbms type
    /// </summary>
    public DbTypeEnum RDBMSType { get; set; } = DbTypeEnum.sqlserver;

    /// <summary>
    /// Gets the value of the command timeout in second
    /// </summary>
    public int CommandTimeoutInSecond { get; } = 60;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceDBContext"/> class
    /// </summary>
    public ServiceDBContext()
        : this(
            DataSettingsManager.LoadSettings().ConnectionString,
            DataSettingsManager.LoadSettings().LoadTimeout(),
            DataSettingsManager.LoadSettings().LoadSchema(),
            DataSettingsManager.LoadSettings().LoadType()
        ) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceDBContext"/> class
    /// </summary>
    /// <param name="CommandTimeoutInSecond">The command timeout in second</param>
    public ServiceDBContext(int CommandTimeoutInSecond)
        : this(DataSettingsManager.LoadSettings().ConnectionString, CommandTimeoutInSecond, "dbo")
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceDBContext"/> class
    /// </summary>
    /// <param name="CommandTimeoutInSecond">The command timeout in second</param>
    /// <param name="SchemaName">The schema name</param>
    public ServiceDBContext(int CommandTimeoutInSecond, string SchemaName)
        : this(
            DataSettingsManager.LoadSettings().ConnectionString,
            CommandTimeoutInSecond,
            SchemaName
        ) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceDBContext"/> class
    /// </summary>
    /// <param name="ConnectionString">The connection string</param>
    /// <param name="CommandTimeoutInSecond">The command timeout in second</param>
    /// <param name="SchemaName">The schema name</param>
    public ServiceDBContext(string ConnectionString, int CommandTimeoutInSecond, string SchemaName)
        : this(ConnectionString, CommandTimeoutInSecond, SchemaName, DbTypeEnum.sqlserver) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceDBContext"/> class
    /// </summary>
    /// <param name="ConnectionString">The connection string</param>
    /// <param name="CommandTimeoutInSecond">The command timeout in second</param>
    /// <param name="SchemaName">The schema name</param>
    /// <param name="DBType">The db type</param>
    public ServiceDBContext(
        string ConnectionString,
        int CommandTimeoutInSecond,
        string SchemaName,
        DbTypeEnum DBType
    )
    {
        this.ConnectionString = ConnectionString;
        this.CommandTimeoutInSecond = CommandTimeoutInSecond;
        this.SchemaName = SchemaName;
        RDBMSType = DBType;
    }

    /// <summary>
    /// Ons the configuring using the specified options builder
    /// </summary>
    /// <param name="optionsBuilder">The options builder</param>
    /// <exception cref="Exception"></exception>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        Console.WriteLine(
            $"[ServiceDBContext] RDBMSType={RDBMSType}, IsConfigured={optionsBuilder.IsConfigured}"
        );
        switch (RDBMSType)
        {
            case DbTypeEnum.sqlserver:
                optionsBuilder.UseSqlServer(
                    ConnectionString,
                    options =>
                    {
                        options.EnableRetryOnFailure();
                        options.CommandTimeout(new int?(CommandTimeoutInSecond));
                    }
                );
                break;
            // case DbTypeEnum.postgresql:
            //     optionsBuilder.UseNpgsql(this.ConnectionString, (Action<NpgsqlDbContextOptionsBuilder>)(options =>
            //     {
            //         options.EnableRetryOnFailure();
            //         options.CommandTimeout(new int?(this.CommandTimeoutInSecond));
            //     }));
            //     break;
            case DbTypeEnum.oracle:
                optionsBuilder.UseOracle(
                    this.ConnectionString,
                    options => options.CommandTimeout(new int?(this.CommandTimeoutInSecond))
                );
                break;
            //case DbTypeEnum.mysql:
            // case DbTypeEnum.mariadb:
            //     optionsBuilder.UseMySql(this.ConnectionString,
            //         (ServerVersion)new MySqlServerVersion(new Version(8, 0, 29)),
            //         (Action<MySqlDbContextOptionsBuilder>)(o =>
            //         {
            //             o.UseRelationalNulls();
            //             o.SchemaBehavior(MySqlSchemaBehavior.Ignore);
            //             o.EnableRetryOnFailure();
            //             o.UseRelationalNulls();
            //             o.CommandTimeout(new int?(this.CommandTimeoutInSecond));
            //         }));
            //     break;
            default:
                DefaultInterpolatedStringHandler interpolatedStringHandler = new(25, 1);
                interpolatedStringHandler.AppendLiteral("DBType ");
                interpolatedStringHandler.AppendFormatted(RDBMSType);
                interpolatedStringHandler.AppendLiteral(" is not supported.");
                throw new Exception(interpolatedStringHandler.ToStringAndClear());
        }
    }

    /// <summary>
    /// Calls the service stored procedure using the specified p stored procedure name
    /// </summary>
    /// <param name="pStoredProcedureName">The stored procedure name</param>
    /// <param name="pWorkflowScheme">The workflow scheme</param>
    /// <param name="pIsReverse">The is reverse</param>
    /// <exception cref="Exception"></exception>
    /// <returns>The workflow scheme</returns>
    public WorkflowScheme CallServiceStoredProcedure(
        string pStoredProcedureName,
        WorkflowScheme pWorkflowScheme,
        EnumIsReverse pIsReverse
    )
    {
        switch (RDBMSType)
        {
            case DbTypeEnum.sqlserver:
                return CallStoredProcedureInMSSQL(
                    pStoredProcedureName,
                    pWorkflowScheme,
                    pIsReverse
                );
            // case DbTypeEnum.postgresql:
            //     return this.__CallStoredProcedureInPostgreSQL(pStoredProcedureName, pWorkflowScheme, pIsReverse);
            case DbTypeEnum.oracle:
                return CallStoredProcedureInOracle(
                    pStoredProcedureName,
                    pWorkflowScheme,
                    pIsReverse
                );
            case DbTypeEnum.mysql:
                return __CallStoredProcedureInMySQL(
                    pStoredProcedureName,
                    pWorkflowScheme,
                    pIsReverse
                );
            default:
                DefaultInterpolatedStringHandler interpolatedStringHandler = new(67, 2);
                interpolatedStringHandler.AppendFormatted(GetType().FullName);
                interpolatedStringHandler.AppendLiteral(
                    ": the method [CallStoredProcedure] did not implement for RDBMSType "
                );
                interpolatedStringHandler.AppendFormatted(RDBMSType);
                throw new Exception(interpolatedStringHandler.ToStringAndClear());
        }
    }

    /// <summary>
    /// Calls the service stored procedure using the specified p stored procedure name
    /// </summary>
    /// <param name="pStoredProcedureName">The stored procedure name</param>
    /// <param name="pWorkflowScheme">The workflow scheme</param>
    /// <param name="pIsReverse">The is reverse</param>
    /// <exception cref="Exception"></exception>
    /// <returns>The wf scheme</returns>
    public WFScheme CallServiceStoredProcedure(
        string pStoredProcedureName,
        WFScheme pWorkflowScheme,
        EnumIsReverse pIsReverse
    )
    {
        switch (RDBMSType)
        {
            case DbTypeEnum.sqlserver:
                return __CallStoredProcedureInMSSQL(
                    pStoredProcedureName,
                    pWorkflowScheme,
                    pIsReverse
                );
            // case DbTypeEnum.postgresql:
            //     return this.__CallStoredProcedureInPostgreSQL(pStoredProcedureName, pWorkflowScheme, pIsReverse);
            case DbTypeEnum.oracle:
                return this.CallStoredProcedureInOracle(
                    pStoredProcedureName,
                    pWorkflowScheme,
                    pIsReverse
                );
            // case DbTypeEnum.mysql:
            //     return this.__CallStoredProcedureInMySQL(
            //         pStoredProcedureName,
            //         pWorkflowScheme,
            //         pIsReverse
            //     );
            default:
                DefaultInterpolatedStringHandler interpolatedStringHandler = new(67, 2);
                interpolatedStringHandler.AppendFormatted(GetType().FullName);
                interpolatedStringHandler.AppendLiteral(
                    ": the method [CallStoredProcedure] did not implement for RDBMSType "
                );
                interpolatedStringHandler.AppendFormatted(RDBMSType);
                throw new Exception(interpolatedStringHandler.ToStringAndClear());
        }
    }

    /// <summary>
    /// Calls the service stored procedure using the specified stored procedure name
    /// </summary>
    /// <param name="storedProcedureName">The stored procedure name</param>
    /// <param name="parameters">The parameters</param>
    /// <param name="databaseName">The database name</param>
    /// <param name="schemaName">The schema name</param>
    /// <exception cref="Exception">Invalid RDBMSType</exception>
    /// <returns>A task containing the object</returns>
    public async Task<object> CallServiceStoredProcedure(
        string storedProcedureName,
        Dictionary<string, object>? parameters = null,
        string databaseName = null,
        string schemaName = "dbo"
    )
    {
        switch (RDBMSType)
        {
            case DbTypeEnum.sqlserver:
                return await ExecuteStoredProcedureInMSSQL(
                    storedProcedureName,
                    parameters,
                    databaseName,
                    schemaName
                );
            // case DbTypeEnum.postgresql:
            //     return this.__CallStoredProcedureInPostgreSQL(pStoredProcedureName, pWorkflowScheme, pIsReverse);
            // case DbTypeEnum.oracle:
            //     return this.__CallStoredProcedureInOracle(
            //         pStoredProcedureName,
            //         pWorkflowScheme,
            //         pIsReverse
            //     );
            // case DbTypeEnum.mysql:
            //     return this.__CallStoredProcedureInMySQL(
            //         pStoredProcedureName,
            //         pWorkflowScheme,
            //         pIsReverse
            //     );
            default:
                //DefaultInterpolatedStringHandler interpolatedStringHandler = new(67, 2);
                //interpolatedStringHandler.AppendFormatted(this.GetType().FullName);
                //interpolatedStringHandler.AppendLiteral(
                //    ": the method [CallStoredProcedure] did not implement for RDBMSType "
                //);
                //interpolatedStringHandler.AppendFormatted<DbTypeEnum>(
                //    this.RDBMSType
                //);
                throw new Exception("Invalid RDBMSType");
        }
    }

    /// <summary>
    /// Calls the stored procedure in mssql using the specified p stored procedure name
    /// </summary>
    /// <param name="pStoredProcedureName">The stored procedure name</param>
    /// <param name="pWorkflowScheme">The workflow scheme</param>
    /// <param name="pIsReverse">The is reverse</param>
    /// <returns>The workflow scheme</returns>
    private WorkflowScheme CallStoredProcedureInMSSQL(
        string pStoredProcedureName,
        WorkflowScheme pWorkflowScheme,
        EnumIsReverse pIsReverse
    )
    {
        string str = JsonSerializer.Serialize(pWorkflowScheme);
        SqlParameter sqlParameter1 = new();
        sqlParameter1.ParameterName = "@WorkflowScheme";
        sqlParameter1.Direction = ParameterDirection.Input;
        sqlParameter1.SqlValue = str;
        sqlParameter1.Size = str.Length;
        sqlParameter1.SqlDbType = SqlDbType.NVarChar;
        SqlParameter sqlParameter2 = sqlParameter1;
        SqlParameter sqlParameter3 = new();
        sqlParameter3.ParameterName = "@IsReverse";
        sqlParameter3.Direction = ParameterDirection.Input;
        sqlParameter3.SqlDbType = SqlDbType.NVarChar;
        sqlParameter3.Size = 1;
        sqlParameter3.SqlValue = pIsReverse.ToString();
        SqlParameter sqlParameter4 = sqlParameter3;
        SqlParameter sqlParameter5 = new();
        sqlParameter5.ParameterName = "@OutputWorkflowScheme";
        sqlParameter5.SqlDbType = SqlDbType.NVarChar;
        sqlParameter5.Size = int.MaxValue;
        sqlParameter5.Direction = ParameterDirection.Output;
        SqlParameter sqlParameter6 = sqlParameter5;
        Database.SetCommandTimeout(new int?(CommandTimeoutInSecond));
        Database.BeginTransaction();
        var a = JsonConvert.SerializeObject(sqlParameter2);
        Database.ExecuteSqlRaw(
            "EXEC "
                + pStoredProcedureName
                + " @WorkflowScheme, @IsReverse, @OutputWorkflowScheme OUTPUT",
            sqlParameter2,
            sqlParameter4,
            sqlParameter6
        );
        Database.CommitTransaction();
        return JsonSerializer.Deserialize<WorkflowScheme>(sqlParameter6.Value.ToString());
    }

    /// <summary>
    /// Calls the stored procedure in mssql using the specified p stored procedure name
    /// </summary>
    /// <param name="pStoredProcedureName">The stored procedure name</param>
    /// <param name="pWorkflowScheme">The workflow scheme</param>
    /// <param name="pIsReverse">The is reverse</param>
    /// <returns>The wf scheme</returns>
    private WFScheme __CallStoredProcedureInMSSQL(
        string pStoredProcedureName,
        WFScheme pWorkflowScheme,
        EnumIsReverse pIsReverse
    )
    {
        string str = JsonSerializer.Serialize(pWorkflowScheme);

        SqlParameter sqlParameter1 = new()
        {
            ParameterName = "@WorkflowScheme",
            Direction = ParameterDirection.Input,
            SqlValue = str,
            Size = str.Length,
            SqlDbType = SqlDbType.NVarChar,
        };

        SqlParameter sqlParameter2 = sqlParameter1;

        SqlParameter sqlParameter3 = new()
        {
            ParameterName = "@IsReverse",
            Direction = ParameterDirection.Input,
            SqlDbType = SqlDbType.NVarChar,
            Size = 1,
            SqlValue = pIsReverse.ToString(),
        };

        SqlParameter sqlParameter4 = sqlParameter3;

        SqlParameter sqlParameter5 = new()
        {
            ParameterName = "@OutputWorkflowScheme",
            SqlDbType = SqlDbType.NVarChar,
            Size = int.MaxValue,
            Direction = ParameterDirection.Output,
        };

        SqlParameter sqlParameter6 = sqlParameter5;

        Database.SetCommandTimeout(new int?(CommandTimeoutInSecond));
        Database.BeginTransaction();
        Database.ExecuteSqlRaw(
            "EXEC "
                + pStoredProcedureName
                + " @WorkflowScheme, @IsReverse, @OutputWorkflowScheme OUTPUT",
            sqlParameter2,
            sqlParameter4,
            sqlParameter6
        );
        Database.CommitTransaction();

        return JsonSerializer.Deserialize<WFScheme>(sqlParameter6.Value.ToString());
    }

    /// <summary>
    /// Executes the stored procedure in mssql using the specified stored procedure name
    /// </summary>
    /// <param name="storedProcedureName">The stored procedure name</param>
    /// <param name="parameters">The parameters</param>
    /// <param name="databaseName">The database name</param>
    /// <param name="schemaName">The schema name</param>
    /// <returns>A task containing the object</returns>
    private async Task<object> ExecuteStoredProcedureInMSSQL(
        string storedProcedureName,
        Dictionary<string, object> parameters = null,
        string databaseName = null,
        string schemaName = "dbo"
    )
    {
        using var connection = Database.GetDbConnection();
        if (connection.State == ConnectionState.Closed)
        {
            connection.Open();
        }

        using var command = connection.CreateCommand();
        if (!string.IsNullOrEmpty(databaseName))
        {
            storedProcedureName = $"{databaseName}.{schemaName}.{storedProcedureName}";
        }

        command.CommandText = storedProcedureName;
        command.CommandType = CommandType.StoredProcedure;
        command.CommandTimeout = CommandTimeoutInSecond;

        if (parameters != null)
        {
            foreach (var param in parameters)
            {
                var sqlParam = new SqlParameter($"@{param.Key}", param.Value ?? DBNull.Value)
                {
                    SqlDbType = GetSqlDbType(param.Value),
                };
                command.Parameters.Add(sqlParam);
            }
        }

        var outputParam = new SqlParameter("@OutputValue", SqlDbType.NVarChar, -1)
        {
            Direction = ParameterDirection.Output,
        };
        command.Parameters.Add(outputParam);

        try
        {
            await command.ExecuteNonQueryAsync();
            return outputParam.Value ?? null;
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"An error occur when calling stored procedure {storedProcedureName}: {ex.Message}"
            );
            throw;
        }
    }

    /// <summary>
    /// Gets the sql db type using the specified value
    /// </summary>
    /// <param name="value">The value</param>
    /// <returns>The sql db type</returns>
    private static SqlDbType GetSqlDbType(object value)
    {
        return value switch
        {
            string => SqlDbType.NVarChar,
            int => SqlDbType.Int,
            bool => SqlDbType.Bit,
            DateTime => SqlDbType.DateTime,
            decimal => SqlDbType.Decimal,
            _ => SqlDbType.Variant,
        };
    }

    /// <summary>
    /// Calls the stored procedure in oracle using the specified p stored procedure name
    /// </summary>
    /// <param name="pStoredProcedureName">The stored procedure name</param>
    /// <param name="pWorkflowScheme">The workflow scheme</param>
    /// <param name="pIsReverse">The is reverse</param>
    /// <returns>The workflow scheme</returns>
    private WorkflowScheme CallStoredProcedureInOracle(
        string pStoredProcedureName,
        WorkflowScheme pWorkflowScheme,
        EnumIsReverse pIsReverse
    )
    {
        string str = JsonSerializer.Serialize(pWorkflowScheme);
        OracleParameter oracleParameter1 = new()
        {
            ParameterName = "@WorkflowScheme",
            Direction = ParameterDirection.Input,
            Value = str,
            Size = str.Length,
            OracleDbType = OracleDbType.NVarchar2,
        };
        OracleParameter oracleParameter2 = oracleParameter1;
        OracleParameter oracleParameter3 = new()
        {
            ParameterName = "@IsReverse",
            Direction = ParameterDirection.Input,
            OracleDbType = OracleDbType.NVarchar2,
            Size = 1,
            Value = pIsReverse.ToString(),
        };
        OracleParameter oracleParameter4 = oracleParameter3;
        OracleParameter oracleParameter5 = new()
        {
            ParameterName = "@OutputWorkflowScheme",
            OracleDbType = OracleDbType.NVarchar2,
            Size = int.MaxValue,
            Direction = ParameterDirection.Output,
        };
        OracleParameter oracleParameter6 = oracleParameter5;
        Database.SetCommandTimeout(new int?(CommandTimeoutInSecond));
        Database.BeginTransaction();
        Database.ExecuteSqlRaw(
            "BEGIN "
                + pStoredProcedureName
                + "(:@WorkflowScheme, :@IsReverse, :@OutputWorkflowScheme); END;",
            oracleParameter2,
            oracleParameter4,
            oracleParameter6
        );
        Database.CommitTransaction();
        return JsonSerializer.Deserialize<WorkflowScheme>(oracleParameter6.Value.ToString());
    }

    private WFScheme CallStoredProcedureInOracle(
        string pStoredProcedureName,
        WFScheme pWorkflowScheme,
        EnumIsReverse pIsReverse
    )
    {
        string str = pWorkflowScheme.ToJson();
        OracleParameter oracleParameter1 = new()
        {
            ParameterName = "@WorkflowScheme",
            Direction = ParameterDirection.Input,
            Value = str,
            Size = str.Length,
            OracleDbType = OracleDbType.NVarchar2,
        };
        OracleParameter oracleParameter2 = oracleParameter1;
        OracleParameter oracleParameter3 = new()
        {
            ParameterName = "@IsReverse",
            Direction = ParameterDirection.Input,
            OracleDbType = OracleDbType.NVarchar2,
            Size = 1,
            Value = pIsReverse.ToString(),
        };
        OracleParameter oracleParameter4 = oracleParameter3;
        OracleParameter oracleParameter5 = new()
        {
            ParameterName = "@OutputWorkflowScheme",
            OracleDbType = OracleDbType.NVarchar2,
            Size = int.MaxValue,
            Direction = ParameterDirection.Output,
        };
        OracleParameter oracleParameter6 = oracleParameter5;
        Database.SetCommandTimeout(new int?(CommandTimeoutInSecond));
        Database.BeginTransaction();
        Database.ExecuteSqlRaw(
            "BEGIN "
                + pStoredProcedureName
                + "(:@WorkflowScheme, :@IsReverse, :@OutputWorkflowScheme); END;",
            oracleParameter2,
            oracleParameter4,
            oracleParameter6
        );
        Database.CommitTransaction();
        return oracleParameter6.Value.ToObject<WFScheme>();
    }

    /// <summary>
    /// Calls the stored procedure in my sql using the specified p stored procedure name
    /// </summary>
    /// <param name="pStoredProcedureName">The stored procedure name</param>
    /// <param name="pWorkflowScheme">The workflow scheme</param>
    /// <param name="pIsReverse">The is reverse</param>
    /// <returns>The workflow scheme</returns>
    private WorkflowScheme __CallStoredProcedureInMySQL(
        string pStoredProcedureName,
        WorkflowScheme pWorkflowScheme,
        EnumIsReverse pIsReverse
    )
    {
        string str = JsonSerializer.Serialize(pWorkflowScheme);
        MySqlParameter mySqlParameter1 = new();
        mySqlParameter1.ParameterName = "@WorkflowScheme";
        mySqlParameter1.Direction = ParameterDirection.Input;
        mySqlParameter1.Value = str;
        mySqlParameter1.Size = str.Length;
        mySqlParameter1.MySqlDbType = MySqlDbType.VarChar;
        MySqlParameter mySqlParameter2 = mySqlParameter1;
        MySqlParameter mySqlParameter3 = new();
        mySqlParameter3.ParameterName = "@IsReverse";
        mySqlParameter3.Direction = ParameterDirection.Input;
        mySqlParameter3.MySqlDbType = MySqlDbType.VarChar;
        mySqlParameter3.Size = 1;
        mySqlParameter3.Value = pIsReverse.ToString();
        MySqlParameter mySqlParameter4 = mySqlParameter3;
        MySqlParameter mySqlParameter5 = new();
        mySqlParameter5.ParameterName = "@OutputWorkflowScheme";
        mySqlParameter5.MySqlDbType = MySqlDbType.VarChar;
        mySqlParameter5.Size = int.MaxValue;
        mySqlParameter5.Direction = ParameterDirection.Output;
        MySqlParameter mySqlParameter6 = mySqlParameter5;
        Database.SetCommandTimeout(new int?(CommandTimeoutInSecond));
        Database.BeginTransaction();
        Database.ExecuteSqlRaw(
            "CALL "
                + pStoredProcedureName
                + " (@WorkflowScheme, @IsReverse, @OutputWorkflowScheme OUTPUT)",
            mySqlParameter2,
            mySqlParameter4,
            mySqlParameter6
        );
        Database.CommitTransaction();
        return JsonSerializer.Deserialize<WorkflowScheme>(mySqlParameter6.Value.ToString());
    }

    // public object ExecuteQuery(string stringQuery)
    // {
    //     Database.BeginTransaction();
    //     var result = Database.SqlQueryRaw<object>(stringQuery).ToList();
    //     Database.CommitTransaction();
    //     return result;
    // }

    /// <summary>
    /// Executes the query using the specified string query
    /// </summary>
    /// <param name="stringQuery">The string query</param>
    /// <returns>The dt</returns>
    public DataTable ExecuteQuery(string stringQuery)
    {
        DataTable dt = new();

        using (var connection = Database.GetDbConnection())
        {
            using var command = connection.CreateCommand();
            command.CommandText = stringQuery;

            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            using var reader = command.ExecuteReader();
            dt.Load(reader);
        }
        return dt;
    }

    /// <summary>
    /// Executes the dml using the specified sql
    /// </summary>
    /// <param name="sql">The sql</param>
    /// <param name="sQLAuditLog">The ql audit log</param>
    /// <param name="parameters">The parameters</param>
    /// <exception cref="InvalidOperationException">An error occurred while executing the SQL command. </exception>
    /// <returns>A task containing the int</returns>
    public async Task<int> ExecuteDML(
        string sql,
        SQLAuditLog sQLAuditLog,
        Dictionary<string, object> parameters = null
    )
    {
        using var connection = Database.GetDbConnection();
        using var command = connection.CreateCommand();
        command.CommandText = sql;

        // Thêm tham số vào command
        if (parameters != null)
        {
            foreach (var param in parameters)
            {
                var dbParam = command.CreateParameter();
                dbParam.ParameterName = param.Key;

                if (param.Value is JsonElement jsonElement)
                {
                    dbParam.Value = jsonElement.ValueKind switch
                    {
                        JsonValueKind.String => jsonElement.GetString(),
                        JsonValueKind.Number => jsonElement.TryGetInt32(out var intValue)
                            ? intValue
                            : jsonElement.GetDouble(),
                        JsonValueKind.True or JsonValueKind.False => jsonElement.GetBoolean(),
                        _ => DBNull.Value,
                    };
                }
                else
                {
                    dbParam.Value = param.Value ?? DBNull.Value;
                }
                command.Parameters.Add(dbParam);
            }
        }
        var query = GetMappedSql(sql, parameters);
        sQLAuditLog.Query = query;
        sQLAuditLog.ExecutionTime = DateTime.Now;

        try
        {
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            var count = await command.ExecuteNonQueryAsync();
            sQLAuditLog.Status = SQLAuditStatus.Success;
            sQLAuditLog.Result = $"{count} row(s) affected";
            return count;
        }
        catch (Exception ex)
        {
            sQLAuditLog.Status = SQLAuditStatus.Failure;
            sQLAuditLog.ErrorMessage = ex.Message;
            sQLAuditLog.Query = query;
            throw new InvalidOperationException(
                "An error occurred while executing the SQL command.",
                ex
            );
        }
        finally
        {
            await EngineContext.Current.Resolve<ISQLAuditLogService>().InsertAsync(sQLAuditLog);
        }
    }

    /// <summary>
    /// Gets the mapped sql using the specified sql
    /// </summary>
    /// <param name="sql">The sql</param>
    /// <param name="parameters">The parameters</param>
    /// <returns>The sql</returns>
    public static string GetMappedSql(string sql, Dictionary<string, object> parameters)
    {
        if (parameters == null)
        {
            return sql;
        }

        foreach (var param in parameters)
        {
            sql = sql.Replace($"@{param.Key}", $"'{param.Value}'");
        }
        return sql;
    }

    /// <summary>
    /// Executes the query using the specified query
    /// </summary>
    /// <param name="query">The query</param>
    /// <param name="sQLAuditLog">The ql audit log</param>
    /// <param name="parameters">The parameters</param>
    /// <param name="schemaName">The schema name</param>
    /// <exception cref="InvalidOperationException">An error occurred while executing the SQL command. </exception>
    /// <returns>The result</returns>
    public async Task<List<Dictionary<string, object>>> ExecuteQueryAsync(
        string query,
        SQLAuditLog sQLAuditLog,
        Dictionary<string, object> parameters = null,
        string schemaName = null
    )
    {
        var result = new List<Dictionary<string, object>>();

        using (var connection = Database.GetDbConnection())
        {
            if (!string.IsNullOrEmpty(schemaName))
            {
                using var useCommand = connection.CreateCommand();
                useCommand.CommandText = $"USE {schemaName};";
                useCommand.CommandType = CommandType.Text;
                useCommand.ExecuteNonQuery();
            }
            using var command = connection.CreateCommand();

            command.CommandText = query;
            command.CommandTimeout = 180;

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    var dbParam = command.CreateParameter();
                    dbParam.ParameterName = param.Key;

                    if (
                        query.Contains("LIKE")
                        && param.Value is string value
                        && !value.Contains('%')
                    )
                    {
                        dbParam.Value = $"%{value}%";
                    }
                    else if (param.Value is JsonElement jsonElement)
                    {
                        if (query.Contains($"LIKE '%' + @{param.Key} + '%'"))
                        {
                            dbParam.Value = $"%{jsonElement.GetString()}%";
                        }
                        else
                        {
                            if (jsonElement.ValueKind == JsonValueKind.String)
                            {
                                dbParam.Value = jsonElement.GetString();
                            }
                            else if (jsonElement.ValueKind == JsonValueKind.Number)
                            {
                                dbParam.Value = jsonElement.GetDouble();
                            }
                            else if (
                                jsonElement.ValueKind == JsonValueKind.True
                                || jsonElement.ValueKind == JsonValueKind.False
                            )
                            {
                                dbParam.Value = jsonElement.GetBoolean();
                            }
                            else
                            {
                                dbParam.Value = DBNull.Value;
                            }
                        }
                    }
                    else
                    {
                        dbParam.Value = param.Value ?? DBNull.Value;
                    }

                    command.Parameters.Add(dbParam);
                }
            }

            sQLAuditLog.Query = GetMappedSql(query, parameters);
            sQLAuditLog.ExecutionTime = DateTime.Now;
            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }

                using var reader = await command.ExecuteReaderAsync(
                    CommandBehavior.SequentialAccess
                );
                var columns = Enumerable
                    .Range(0, reader.FieldCount)
                    .Select(reader.GetName)
                    .ToList();

                while (await reader.ReadAsync())
                {
                    var row = new Dictionary<string, object>(reader.FieldCount);
                    foreach (var col in columns)
                    {
                        var value = reader[col];
                        if (!col.StartsWith("JSON") && col != "Result")
                        {
                            row[col.ToLower()] = value == DBNull.Value ? null : value;
                        }
                        else
                        {
                            if (value.TryParse<Dictionary<string, object>>(out var dic))
                            {
                                row = dic;
                            }
                        }
                    }
                    result.Add(row);
                }
                sQLAuditLog.Status = SQLAuditStatus.Success; // Ghi log thành công
                sQLAuditLog.Result = JsonConvert.SerializeObject(result);
            }
            catch (Exception ex)
            {
                sQLAuditLog.Status = SQLAuditStatus.Failure; // Ghi log thất bại
                sQLAuditLog.ErrorMessage = ex.Message;
                throw new InvalidOperationException(
                    "An error occurred while executing the SQL command.",
                    ex
                );
            }
            finally
            {
                await EngineContext.Current.Resolve<ISQLAuditLogService>().InsertAsync(sQLAuditLog); // Insert log
            }
        }
        return result;
    }

    /// <summary>
    /// Columns the exists using the specified reader
    /// </summary>
    /// <param name="reader">The reader</param>
    /// <param name="columnName">The column name</param>
    /// <returns>The bool</returns>
    private static bool ColumnExists(DbDataReader reader, string columnName)
    {
        for (int i = 0; i < reader.FieldCount; i++)
        {
            if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// EXECUTES the paged query SQL server asynchronous using the specified connection, paged query, page index, page size, parameters and s ql audit log
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="pagedQuery"></param>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <param name="parameters"></param>
    /// <param name="sQLAuditLog"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private static async Task<
        PagedListModel<Dictionary<string, object>>
    > ExecutePagedQuerySqlServerAsync(
        DbConnection connection,
        string pagedQuery,
        int pageIndex,
        int pageSize,
        Dictionary<string, object> parameters,
        SQLAuditLog sQLAuditLog
    )
    {
        var pagedResult = new PagedListModel<Dictionary<string, object>>
        {
            PageIndex = pageIndex,
            PageSize = pageSize,
        };

        using var pagedCommand = connection.CreateCommand();
        pagedCommand.CommandText = pagedQuery;
        pagedCommand.CommandTimeout = 180;

        if (parameters != null)
        {
            foreach (var param in parameters)
            {
                var dbParam = pagedCommand.CreateParameter();
                dbParam.ParameterName = param.Key;

                if (param.Value is JsonElement jsonElement)
                {
                    if (pagedQuery.Contains($"LIKE '%' + @{param.Key} + '%'"))
                    {
                        dbParam.Value = $"%{jsonElement.GetString()}%";
                    }
                    else
                    {
                        if (jsonElement.ValueKind == JsonValueKind.String)
                        {
                            dbParam.Value = jsonElement.GetString();
                        }
                        else if (jsonElement.ValueKind == JsonValueKind.Number)
                        {
                            dbParam.Value = jsonElement.GetDouble();
                        }
                        else if (
                            jsonElement.ValueKind == JsonValueKind.True
                            || jsonElement.ValueKind == JsonValueKind.False
                        )
                        {
                            dbParam.Value = jsonElement.GetBoolean();
                        }
                        else
                        {
                            dbParam.Value = DBNull.Value;
                        }
                    }
                }
                else
                {
                    dbParam.Value = param.Value ?? DBNull.Value;
                }
                pagedCommand.Parameters.Add(dbParam);
            }
        }

        var offsetParam = pagedCommand.CreateParameter();
        offsetParam.ParameterName = "@Offset";
        offsetParam.Value = pageIndex == 0 ? pageIndex * pageSize : (pageIndex - 1) * pageSize;
        pagedCommand.Parameters.Add(offsetParam);

        var pageSizeParam = pagedCommand.CreateParameter();
        pageSizeParam.ParameterName = "@PageSize";
        pageSizeParam.Value = pageSize;
        pagedCommand.Parameters.Add(pageSizeParam);

        sQLAuditLog.Query = pagedCommand.CommandText;
        sQLAuditLog.ExecutionTime = DateTime.Now;
        try
        {
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            using var reader = await pagedCommand.ExecuteReaderAsync();
            var columns = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToList();

            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object>(reader.FieldCount);
                var list = new List<Dictionary<string, object>>();
                foreach (var col in columns)
                {
                    var value = reader[col];
                    if (!col.StartsWith("JSON") && col != "Result")
                    {
                        row[col.ToLower()] = value == DBNull.Value ? null : value;
                    }
                    else
                    {
                        if (col == "Result")
                        {
                            if (value.TryParse<List<Dictionary<string, object>>>(out var listDic))
                            {
                                list = listDic;
                                break;
                            }
                        }
                        else if (value.TryParse<Dictionary<string, object>>(out var dic))
                        {
                            row = dic;
                        }
                    }
                }
                if (list.Count > 0)
                {
                    pagedResult.Items = list;
                    if (
                        pagedResult.TotalCount == 0
                        && list[0].TryGetValue("TotalCount", out var totalCount)
                    )
                    {
                        pagedResult.TotalCount = Convert.ToInt32(totalCount);
                    }
                }
                else
                {
                    pagedResult.Items.Add(row);
                    if (pagedResult.TotalCount == 0)
                    {
                        if (reader.HasRows && ColumnExists(reader, "TotalCount"))
                        {
                            var totalCountValue = reader["TotalCount"];
                            if (totalCountValue != DBNull.Value)
                            {
                                pagedResult.TotalCount = Convert.ToInt32(totalCountValue);
                            }
                        }
                    }
                }
            }

            pagedResult.TotalPages = (int)Math.Ceiling((double)pagedResult.TotalCount / pageSize);
            pagedResult.HasNextPage = pageIndex < pagedResult.TotalPages;
            pagedResult.HasPreviousPage = pageIndex > 1;

            sQLAuditLog.Status = SQLAuditStatus.Success; // Ghi log thành công
            sQLAuditLog.Result = JsonConvert.SerializeObject(pagedResult);
        }
        catch (Exception ex)
        {
            // Ghi log thất bại
            sQLAuditLog.Status = SQLAuditStatus.Failure;
            sQLAuditLog.ErrorMessage = ex.Message;
            throw new InvalidOperationException(
                "An error occurred while executing the SQL command.",
                ex
            );
        }
        finally
        {
            await EngineContext.Current.Resolve<ISQLAuditLogService>().InsertAsync(sQLAuditLog);
        }

        return pagedResult;
    }

    private static async Task<
        PagedListModel<Dictionary<string, object>>
    > ExecutePagedQueryOracleAsync(
        DbConnection connection,
        string pagedQuery,
        int pageIndex,
        int pageSize,
        Dictionary<string, object> parameters,
        SQLAuditLog sQLAuditLog
    )
    {
        var result = new PagedListModel<Dictionary<string, object>>
        {
            PageIndex = pageIndex,
            PageSize = pageSize,
        };

        using var cmd = connection.CreateCommand();
        cmd.CommandText = pagedQuery;
        cmd.CommandTimeout = 180;

        if (cmd.GetType().Name == "OracleCommand")
        {
            dynamic ocmd = cmd;
            ocmd.BindByName = true;
        }

        if (parameters != null)
        {
            foreach (var kv in parameters)
            {
                var p = cmd.CreateParameter();
                p.ParameterName = kv.Key.TrimStart(':');
                p.Value = ConvertParamValueForDb(kv.Value);
                cmd.Parameters.Add(p);
            }
        }

        // Phân trang
        var offset = pageIndex == 0 ? 0 : (pageIndex - 1) * pageSize;

        var pOffset = cmd.CreateParameter();
        pOffset.ParameterName = "Offset";
        pOffset.Value = offset;
        cmd.Parameters.Add(pOffset);

        var pPageSize = cmd.CreateParameter();
        pPageSize.ParameterName = "PageSize";
        pPageSize.Value = pageSize;
        cmd.Parameters.Add(pPageSize);

        // Log câu SQL thế tham số để debug
        sQLAuditLog.Query = RenderSqlForDebug(cmd, SqlDialect.Oracle);
        sQLAuditLog.ExecutionTime = DateTime.Now;

        try
        {
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            using var reader = await cmd.ExecuteReaderAsync();
            await FillPagedResultFromReader(reader, result, pageSize, pageIndex);

            sQLAuditLog.Status = SQLAuditStatus.Success;
            sQLAuditLog.Result = JsonConvert.SerializeObject(result);
        }
        catch (Exception ex)
        {
            sQLAuditLog.Status = SQLAuditStatus.Failure;
            sQLAuditLog.ErrorMessage = ex.Message;
            throw new InvalidOperationException(
                "An error occurred while executing the SQL command.",
                ex
            );
        }
        finally
        {
            await EngineContext.Current.Resolve<ISQLAuditLogService>().InsertAsync(sQLAuditLog);
        }

        return result;
    }

    /// <summary>
    /// Executes the paged query my sql asynchronous using the specified connection, paged query, page index, page size, parameters and s ql audit log
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="pagedQuery"></param>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <param name="parameters"></param>
    /// <param name="sQLAuditLog"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private static async Task<
        PagedListModel<Dictionary<string, object>>
    > ExecutePagedQueryMySqlAsync(
        DbConnection connection,
        string pagedQuery,
        int pageIndex,
        int pageSize,
        Dictionary<string, object> parameters,
        SQLAuditLog sQLAuditLog
    )
    {
        var result = new PagedListModel<Dictionary<string, object>>
        {
            PageIndex = pageIndex,
            PageSize = pageSize,
        };

        using var cmd = connection.CreateCommand();
        cmd.CommandText = pagedQuery; // Query nên dùng @Param và phân trang: "... LIMIT @PageSize OFFSET @Offset"
        cmd.CommandTimeout = 180;

        // Add params từ dictionary
        if (parameters != null)
        {
            foreach (var kv in parameters)
            {
                var p = cmd.CreateParameter();
                p.ParameterName = "@" + kv.Key.TrimStart('@', ':'); // MySQL dùng @name
                p.Value = ConvertParamValueForDb(kv.Value);
                cmd.Parameters.Add(p);
            }
        }

        // Paging
        var offset = pageIndex == 0 ? 0 : (pageIndex - 1) * pageSize;

        var pOffset = cmd.CreateParameter();
        pOffset.ParameterName = "@Offset";
        pOffset.Value = offset;
        cmd.Parameters.Add(pOffset);

        var pPageSize = cmd.CreateParameter();
        pPageSize.ParameterName = "@PageSize";
        pPageSize.Value = pageSize;
        cmd.Parameters.Add(pPageSize);

        // Log SQL đã inline tham số (debug only)
        sQLAuditLog.Query = RenderSqlForDebug(cmd, SqlDialect.MySql);
        sQLAuditLog.ExecutionTime = DateTime.Now;

        try
        {
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            using var reader = await cmd.ExecuteReaderAsync();
            await FillPagedResultFromReader(reader, result, pageSize, pageIndex);

            sQLAuditLog.Status = SQLAuditStatus.Success;
            sQLAuditLog.Result = JsonConvert.SerializeObject(result);
        }
        catch (Exception ex)
        {
            sQLAuditLog.Status = SQLAuditStatus.Failure;
            sQLAuditLog.ErrorMessage = ex.Message;
            throw new InvalidOperationException(
                "An error occurred while executing the SQL command.",
                ex
            );
        }
        finally
        {
            await EngineContext.Current.Resolve<ISQLAuditLogService>().InsertAsync(sQLAuditLog);
        }

        return result;
    }

    /// <summary>
    /// Executes the paged query postgres asynchronous using the specified connection, paged query, page index, page size, parameters and s ql audit log
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="pagedQuery"></param>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <param name="parameters"></param>
    /// <param name="sQLAuditLog"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private async Task<PagedListModel<Dictionary<string, object>>> ExecutePagedQueryPostgresAsync(
        DbConnection connection,
        string pagedQuery,
        int pageIndex,
        int pageSize,
        Dictionary<string, object> parameters,
        SQLAuditLog sQLAuditLog
    )
    {
        var result = new PagedListModel<Dictionary<string, object>>
        {
            PageIndex = pageIndex,
            PageSize = pageSize,
        };

        using var cmd = connection.CreateCommand();
        cmd.CommandText = pagedQuery; // Query dùng @Param (Npgsql chấp nhận), phân trang: "... LIMIT @PageSize OFFSET @Offset"
        cmd.CommandTimeout = 180;

        // Add params
        if (parameters != null)
        {
            foreach (var kv in parameters)
            {
                var p = cmd.CreateParameter();
                // Với Npgsql, ParameterName có thể đặt "name" hoặc "@name"; dùng "@name" để đồng bộ với SQL
                p.ParameterName = "@" + kv.Key.TrimStart('@', ':');
                p.Value = ConvertParamValueForDb(kv.Value);
                cmd.Parameters.Add(p);
            }
        }

        // Paging
        var offset = pageIndex == 0 ? 0 : (pageIndex - 1) * pageSize;

        var pOffset = cmd.CreateParameter();
        pOffset.ParameterName = "@Offset";
        pOffset.Value = offset;
        cmd.Parameters.Add(pOffset);

        var pPageSize = cmd.CreateParameter();
        pPageSize.ParameterName = "@PageSize";
        pPageSize.Value = pageSize;
        cmd.Parameters.Add(pPageSize);

        // Log SQL inline tham số
        sQLAuditLog.Query = RenderSqlForDebug(cmd, SqlDialect.Postgres);
        sQLAuditLog.ExecutionTime = DateTime.Now;

        try
        {
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            using var reader = await cmd.ExecuteReaderAsync();
            await FillPagedResultFromReader(reader, result, pageSize, pageIndex);

            sQLAuditLog.Status = SQLAuditStatus.Success;
            sQLAuditLog.Result = JsonConvert.SerializeObject(result);
        }
        catch (Exception ex)
        {
            sQLAuditLog.Status = SQLAuditStatus.Failure;
            sQLAuditLog.ErrorMessage = ex.Message;
            throw new InvalidOperationException(
                "An error occurred while executing the SQL command.",
                ex
            );
        }
        finally
        {
            await EngineContext.Current.Resolve<ISQLAuditLogService>().InsertAsync(sQLAuditLog);
        }

        return result;
    }

    /// <summary>
    /// Executes the paged query using the specified paged query
    /// </summary>
    /// <param name="pagedQuery">The paged query</param>
    /// <param name="pageIndex">The page index</param>
    /// <param name="pageSize">The page size</param>
    /// <param name="parameters">The parameters</param>
    /// <param name="sQLAuditLog">The ql audit log</param>
    /// <exception cref="InvalidOperationException">An error occurred while executing the SQL command. </exception>
    /// <returns>The paged result</returns>
    public async Task<PagedListModel<Dictionary<string, object>>> ExecutePagedQueryAsync(
        string pagedQuery,
        int pageIndex,
        int pageSize,
        Dictionary<string, object> parameters,
        SQLAuditLog sQLAuditLog
    )
    {
        using var connection = Database.GetDbConnection();

        var connType = connection.GetType().Name;

        switch (connType)
        {
            case "OracleConnection":
                return await ExecutePagedQueryOracleAsync(
                    connection,
                    pagedQuery,
                    pageIndex,
                    pageSize,
                    parameters,
                    sQLAuditLog
                );

            case "SqlConnection":
                return await ExecutePagedQuerySqlServerAsync(
                    connection,
                    pagedQuery,
                    pageIndex,
                    pageSize,
                    parameters,
                    sQLAuditLog
                );

            // sau này mở rộng dễ dàng:
            case "MySqlConnection":
                return await ExecutePagedQueryMySqlAsync(
                    connection,
                    pagedQuery,
                    pageIndex,
                    pageSize,
                    parameters,
                    sQLAuditLog
                );

            case "NpgsqlConnection": // PostgreSQL
                return await ExecutePagedQueryPostgresAsync(
                    connection,
                    pagedQuery,
                    pageIndex,
                    pageSize,
                    parameters,
                    sQLAuditLog
                );

            default:
                throw new NotSupportedException(
                    $"Unsupported database connection type: {connType}"
                );
        }
    }

    /// <summary>
    /// Ons the model creating using the specified model builder
    /// </summary>
    /// <param name="modelBuilder">The model builder</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder) { }

    /// <summary>
    /// Disposes this instance
    /// </summary>
    public override void Dispose() => base.Dispose();

    /// <summary>
    /// The enum is reverse enum
    /// </summary>
    public enum EnumIsReverse
    {
        /// <summary>
        /// The  enum is reverse
        /// </summary>
        N,

        /// <summary>
        /// The  enum is reverse
        /// </summary>
        R,
    }

    private enum SqlDialect
    {
        SqlServer,
        Oracle,
        MySql,
        Postgres,
    }

    private static string RenderSqlForDebug(DbCommand cmd, SqlDialect dialect)
    {
        string sql = cmd.CommandText;

        static string TrimPrefix(string name) => name?.TrimStart('@', ':') ?? string.Empty;

        foreach (DbParameter p in cmd.Parameters)
        {
            var name = TrimPrefix(p.ParameterName);

            string lit =
                dialect == SqlDialect.Oracle
                    ? ToOracleLiteral(p.Value)
                    : ToSqlServerLiteral(p.Value);

            var patterns =
                dialect == SqlDialect.Oracle
                    ? [$@"(?<!:):{Regex.Escape(name)}\b"]
                    : new[] { $@"@{Regex.Escape(name)}\b" };

            foreach (var pattern in patterns)
            {
                sql = Regex.Replace(sql, pattern, lit, RegexOptions.IgnoreCase);
            }
        }

        return sql;

        static string ToSqlServerLiteral(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return "NULL";
            }

            return value switch
            {
                string s => $"N'{s.Replace("'", "''")}'",
                bool b => b ? "1" : "0",
                DateTime dt => $"CONVERT(datetime2, '{dt:yyyy-MM-dd HH:mm:ss.fff}', 121)",
                DateTimeOffset dto =>
                    $"SWITCHOFFSET(CONVERT(datetimeoffset, '{dto:yyyy-MM-dd HH:mm:ss.fff zzz}', 127), DATENAME(TzOffset, SYSDATETIMEOFFSET()))",
                IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
                _ => $"N'{value.ToString().Replace("'", "''")}'",
            };
        }

        static string ToOracleLiteral(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return "NULL";
            }

            return value switch
            {
                string s => $"'{s.Replace("'", "''")}'",
                bool b => b ? "1" : "0",
                DateTime dt =>
                    $"TO_TIMESTAMP('{dt:yyyy-MM-dd HH:mm:ss.fff}', 'YYYY-MM-DD HH24:MI:SS.FF3')",
                DateTimeOffset dto =>
                    $"TO_TIMESTAMP_TZ('{dto:yyyy-MM-dd HH:mm:ss.fff zzz}', 'YYYY-MM-DD HH24:MI:SS.FF3 TZH:TZM')",
                IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
                _ => $"'{value.ToString().Replace("'", "''")}'",
            };
        }
    }

    private static object ConvertParamValueForDb(object value)
    {
        if (value is null)
        {
            return DBNull.Value;
        }

        if (value is JsonElement je)
        {
            return je.ValueKind switch
            {
                JsonValueKind.String => (object?)je.GetString() ?? DBNull.Value,
                JsonValueKind.Number => je.TryGetInt64(out var l) ? l
                : je.TryGetDecimal(out var d) ? d
                : je.TryGetDouble(out var dbl) ? dbl
                : DBNull.Value,
                JsonValueKind.True => 1,
                JsonValueKind.False => 0,
                JsonValueKind.Null => DBNull.Value,
                _ => DBNull.Value,
            };
        }

        return value ?? DBNull.Value;
    }

    private static async Task FillPagedResultFromReader(
        DbDataReader reader,
        PagedListModel<Dictionary<string, object>> pagedResult,
        int pageSize,
        int pageIndex
    )
    {
        var cols = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToList();

        while (await reader.ReadAsync())
        {
            var row = new Dictionary<string, object>(reader.FieldCount);
            var list = new List<Dictionary<string, object>>();

            foreach (var col in cols)
            {
                var value = reader[col];
                var colName = col;

                if (
                    !colName.StartsWith("JSON", StringComparison.OrdinalIgnoreCase)
                    && !colName.Equals("Result", StringComparison.OrdinalIgnoreCase)
                )
                {
                    row[colName.ToLowerInvariant()] = value == DBNull.Value ? null : value;
                }
                else
                {
                    if (colName.Equals("Result", StringComparison.OrdinalIgnoreCase))
                    {
                        var str =
                            value == DBNull.Value ? null : (value as string ?? value.ToString());
                        if (
                            !string.IsNullOrWhiteSpace(str)
                            && str.TryParse<List<Dictionary<string, object>>>(out var listDic)
                        )
                        {
                            list = listDic;
                            break;
                        }
                    }
                    else
                    {
                        if (value.TryParse<Dictionary<string, object>>(out var dic))
                        {
                            row = dic;
                        }
                    }
                }
            }

            if (list.Count > 0)
            {
                pagedResult.Items = list;
                if (
                    pagedResult.TotalCount == 0
                    && list[0].TryGetValue("TotalCount", out var totalCount)
                )
                {
                    pagedResult.TotalCount = Convert.ToInt32(totalCount);
                }
            }
            else
            {
                pagedResult.Items.Add(row);
                if (
                    pagedResult.TotalCount == 0
                    && reader.HasRows
                    && ColumnExists(reader, "TotalCount")
                )
                {
                    var tc = reader["TotalCount"];
                    if (tc != DBNull.Value)
                    {
                        pagedResult.TotalCount = Convert.ToInt32(tc);
                    }
                }
            }
        }

        pagedResult.TotalPages = (int)Math.Ceiling((double)pagedResult.TotalCount / pageSize);
        pagedResult.HasNextPage = pageIndex < pagedResult.TotalPages;
        pagedResult.HasPreviousPage = pageIndex > 1;
    }
}
