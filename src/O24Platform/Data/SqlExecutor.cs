using Microsoft.Data.SqlClient;
using O24OpenAPI.Core.Infrastructure;
using O24OpenAPI.Data.Configuration;
using Oracle.ManagedDataAccess.Client;

namespace O24OpenAPI.Data;

/// <summary>
/// The sql executor class
/// </summary>
public class SqlExecutor
{
    /// <summary>
    /// Gets the value of the connection string
    /// </summary>
    private static string ConnectionString => Singleton<DataConfig>.Instance.DefaultConnection;

    /// <summary>
    /// Thực thi truy vấn và trả về giá trị scalar kiểu T.
    /// </summary>
    public static async Task<T> ExecuteScalarAsync<T>(string sql, params SqlParameter[] parameters)
    {
        using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand(sql, connection);
        if (parameters?.Length > 0)
        {
            command.Parameters.AddRange(parameters);
        }

        object result = await command.ExecuteScalarAsync();
        return result is null ? default : (T)Convert.ChangeType(result, typeof(T));
    }

    /// <summary>
    /// Thực thi script SQL không trả về dữ liệu.
    /// </summary>
    public static async Task ExecuteSqlScriptAsync(string sql, params SqlParameter[] parameters)
    {
        using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand(sql, connection);
        if (parameters?.Length > 0)
        {
            command.Parameters.AddRange(parameters);
        }

        await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Executes the sql script using the specified sql script
    /// </summary>
    /// <param name="sqlScript">The sql script</param>
    /// <param name="connectionString">The connection string</param>
    public static async Task ExecuteSqlScriptAsync(string sqlScript, string connectionString = null)
    {
        connectionString ??= Singleton<DataConfig>.Instance.DefaultConnection;
        await using SqlConnection connection = new(connectionString);
        await connection.OpenAsync();

        await using SqlCommand command = new(sqlScript, connection);
        await command.ExecuteNonQueryAsync();

        Console.WriteLine($"SQL script executed successfully: {sqlScript}");
    }

    /// <summary>
    /// Executes the sql from file using the specified connection string
    /// </summary>
    /// <param name="connectionString">The connection string</param>
    /// <param name="filePath">The file path</param>
    /// <exception cref="FileNotFoundException">SQL file not found: {filePath}</exception>
    public static async Task ExecuteSqlFromFileAsync(
        string connectionString,
        string filePath,
        DataProviderType providerType = DataProviderType.SqlServer
    )
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"SQL file not found: {filePath}");
        }

        string sqlScript = await File.ReadAllTextAsync(filePath);

        string[] separator = providerType switch
        {
            DataProviderType.SqlServer => ["GO", "go"],
            DataProviderType.Oracle => ["/"],
            _ => throw new NotSupportedException(
                $"DataProvider '{providerType}' is not supported."
            ),
        };

        string[] sqlStatements = sqlScript.Split(separator, StringSplitOptions.RemoveEmptyEntries);

        switch (providerType)
        {
            case DataProviderType.SqlServer:
                await using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    foreach (var sql in sqlStatements)
                    {
                        var trimmedSql = sql.Trim();
                        if (string.IsNullOrWhiteSpace(trimmedSql))
                        {
                            continue;
                        }

                        await using var command = new SqlCommand(trimmedSql, connection);
                        try
                        {
                            await command.ExecuteNonQueryAsync();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(
                                $"[SQL SERVER] Failed statement:\n{trimmedSql}\nError: {ex.Message}\n"
                            );
                        }
                    }
                }
                break;

            case DataProviderType.Oracle:
                await using (var connection = new OracleConnection(connectionString))
                {
                    await connection.OpenAsync();
                    foreach (var sql in sqlStatements)
                    {
                        var trimmedSql = sql.Trim();
                        if (string.IsNullOrWhiteSpace(trimmedSql))
                        {
                            continue;
                        }

                        await using var command = new OracleCommand(trimmedSql, connection);
                        try
                        {
                            await command.ExecuteNonQueryAsync();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(
                                $"[ORACLE] Failed statement:\n{trimmedSql}\nError: {ex.Message}\n"
                            );
                        }
                    }
                }
                break;

            default:
                throw new NotSupportedException($"DataProvider '{providerType}' is not supported.");
        }

        Console.WriteLine("SQL script executed (errors, if any, were logged).");
    }

    /// <summary>
    /// Databases the exists using the specified connection string
    /// </summary>
    /// <param name="connectionString">The connection string</param>
    /// <param name="databaseName">The database name</param>
    /// <returns>A task containing the bool</returns>
    public static async Task<bool> DatabaseExistsAsync(
        string connectionString,
        string databaseNameOrSchema,
        DataProviderType providerType = DataProviderType.SqlServer
    )
    {
        switch (providerType)
        {
            case DataProviderType.SqlServer:
                var sqlQuery = "SELECT COUNT(*) FROM sys.databases WHERE name = @dbName";

                await using (var sqlConn = new SqlConnection(connectionString))
                {
                    await sqlConn.OpenAsync();
                    await using var sqlCmd = new SqlCommand(sqlQuery, sqlConn);
                    sqlCmd.Parameters.AddWithValue("@dbName", databaseNameOrSchema);

                    var result = (int)await sqlCmd.ExecuteScalarAsync();
                    return result > 0;
                }

            case DataProviderType.Oracle:
                var oraQuery = "SELECT COUNT(*) FROM all_users WHERE username = :schemaName";

                await using (
                    var oraConn = new Oracle.ManagedDataAccess.Client.OracleConnection(
                        connectionString
                    )
                )
                {
                    await oraConn.OpenAsync();
                    await using var oraCmd = new Oracle.ManagedDataAccess.Client.OracleCommand(
                        oraQuery,
                        oraConn
                    );
                    oraCmd.Parameters.Add(
                        new Oracle.ManagedDataAccess.Client.OracleParameter(
                            "schemaName",
                            databaseNameOrSchema.ToUpper()
                        )
                    );

                    var result = Convert.ToInt32(await oraCmd.ExecuteScalarAsync());
                    return result > 0;
                }

            default:
                throw new NotSupportedException($"Unsupported provider: {providerType}");
        }
    }

    /// <summary>
    /// Cdcs the enable using the specified connection string
    /// </summary>
    /// <param name="connectionString">The connection string</param>
    /// <param name="databaseName">The database name</param>
    /// <returns>A task containing the bool</returns>
    public static async Task<bool> CDCEnableAsync(string connectionString, string databaseName)
    {
        var query =
            $"SELECT COUNT(*) FROM sys.databases WHERE name = @dbName AND is_cdc_enabled = 1";

        await using SqlConnection connection = new(connectionString);
        await connection.OpenAsync();

        await using SqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@dbName", databaseName);

        var result = (int)await command.ExecuteScalarAsync();
        return result > 0;
    }
}
