using System.Globalization;
using LinKit.Json.Runtime;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;
using O24OpenAPI.Core.Configuration;
using O24OpenAPI.Core.Extensions;
using O24OpenAPI.Core.Infrastructure;
using O24OpenAPI.Data;
using O24OpenAPI.Data.Configuration;
using O24OpenAPI.Framework.Models;
using O24OpenAPI.Framework.Services.CDC;
using O24OpenAPI.Logging.Helpers;

namespace O24OpenAPI.Framework.Utils;

/// <summary>
/// The cdc utils class
/// </summary>
public class CDCUtils
{
    public static async Task EnableCDCAsync()
    {
        var config = Singleton<O24OpenAPIConfiguration>.Instance;
        if (config.YourDatabase.NullOrEmpty())
        {
            throw new Exception("YourDatabase is null");
        }
        if (string.IsNullOrEmpty(config.YourCDCSchema))
        {
            throw new Exception("YourCDCSchema is null");
        }
        var appSettings = Singleton<AppSettings>.Instance;
        var dataConfig =
            appSettings.Get<DataConfig>() ?? throw new Exception("ConnectionStrings is null");
        string connectionString = dataConfig.DefaultConnection;
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new Exception("ConnectionString is null");
        }
        if (await SqlExecutor.CDCEnableAsync(connectionString, config.YourDatabase))
        {
            return;
        }
        string enableDbCdc =
            $@"Use {Singleton<O24OpenAPIConfiguration>.Instance.YourDatabase};
                EXEC sys.sp_cdc_enable_db;";

        await SqlExecutor.ExecuteSqlScriptAsync(enableDbCdc, connectionString);
    }

    /// <summary>
    /// Gens the dml using the specified data
    /// </summary>
    /// <param name="data">The data</param>
    /// <returns>The string sql string action</returns>
    public static (string sql, string action) GenDML(CDCData data)
    {
        BusinessLogHelper.Info("===================CDC GenDML called===========================");
        if (data == null || string.IsNullOrWhiteSpace(data.Data))
            return ("", "");

        var raw = data.Data;
        BusinessLogHelper.Info("CDC RAW JSON: " + raw);

        Dictionary<string, object> objectData;
        var trimmed = raw.TrimStart();

        if (trimmed.StartsWith('['))
        {
            var list = trimmed.FromJson<List<Dictionary<string, object>>>();
            objectData = list?.FirstOrDefault();
        }
        else
        {
            objectData = trimmed.FromJson<Dictionary<string, object>>();
        }

        if (objectData == null || objectData.Count == 0)
            return ("", "");

        var tableName = data.TableName;

        // 1) Bỏ các cột kỹ thuật của CDC
        var ignoredColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "__$start_lsn",
            "__$seqval",
            "__$operation",
            "__$update_mask",
            "LSN",
            "Operation",
            "Id",
        };

        foreach (var col in ignoredColumns)
        {
            objectData.Remove(col);
        }

        // 2) Lấy danh sách key cho bảng
        var keyColumns = GetKeyColumns(tableName);
        if (keyColumns.Length == 0)
        {
            BusinessLogHelper.Warning($"CDC GenDML: No key configuration for table {tableName}");
            return ("", "");
        }

        // 3) Build WHERE từ key
        var whereConditions = new List<string>();

        foreach (var key in keyColumns)
        {
            if (!objectData.TryGetValue(key, out var keyValue))
            {
                BusinessLogHelper.Warning(
                    $"CDC GenDML: Key column '{key}' not found in data for table {tableName}"
                );
                return ("", "");
            }

            whereConditions.Add($"{key} = {FormatValue(keyValue)}");
        }

        var whereClause = string.Join(" AND ", whereConditions);

        switch (data.Operation)
        {
            case 1: // DELETE
                return ($"DELETE FROM {tableName} WHERE {whereClause};", "DELETE");

            case 2: // INSERT
            {
                var columns = string.Join(", ", objectData.Keys);
                var values = string.Join(", ", objectData.Values.Select(v => FormatValue(v)));
                return ($"INSERT INTO {tableName} ({columns}) VALUES ({values});", "INSERT");
            }

            case 4: // AFTER UPDATE
            {
                var setPairs = objectData
                    .Where(kvp => !keyColumns.Contains(kvp.Key, StringComparer.OrdinalIgnoreCase))
                    .Select(kvp => $"{kvp.Key} = {FormatValue(kvp.Value)}")
                    .ToList();

                if (setPairs.Count == 0)
                {
                    BusinessLogHelper.Info(
                        $"CDC GenDML: No non-key columns to update for table {tableName}"
                    );
                    return ("", "");
                }

                var setClause = string.Join(", ", setPairs);
                return ($"UPDATE {tableName} SET {setClause} WHERE {whereClause};", "UPDATE");
            }

            default:
                return ("", "");
        }
    }

    /// <summary>
    /// Formats the value using the specified value
    /// </summary>
    /// <param name="value">The value</param>
    /// <returns>The string</returns>
    private static string FormatValue(object value)
    {
        if (value == null || value == DBNull.Value)
            return "NULL";

        if (value is JValue jv)
            value = jv.Value;

        switch (value)
        {
            case string s:
                return "N'" + s.Replace("'", "''") + "'";

            case bool b:
                return b ? "1" : "0";

            case DateTime dt:
                return "'"
                    + dt.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)
                    + "'";

            case DateTimeOffset dto:
                return "'"
                    + dto.ToString("yyyy-MM-dd HH:mm:ss.fff zzz", CultureInfo.InvariantCulture)
                    + "'";

            case byte
            or sbyte
            or short
            or ushort
            or int
            or uint
            or long
            or ulong
            or float
            or double
            or decimal:
                return Convert.ToString(value, CultureInfo.InvariantCulture);

            default:
                var sDefault =
                    Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
                return "N'" + sDefault.Replace("'", "''") + "'";
        }
    }

    /// <summary>
    /// Enables the for table using the specified table name
    /// </summary>
    /// <param name="tableName">The table name</param>
    public static async Task EnableForTable(string tableName)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        string checkCdcEnabled =
            $@"USE {Singleton<O24OpenAPIConfiguration>.Instance.YourDatabase}
           SELECT COUNT(*)
           FROM sys.tables t
           JOIN sys.schemas s ON t.schema_id = s.schema_id
           WHERE s.name = 'dbo'
           AND t.name = @tableName
           AND t.is_tracked_by_cdc = 1;";

        int cdcEnabled = await SqlExecutor.ExecuteScalarAsync<int>(
            checkCdcEnabled,
            new SqlParameter("@tableName", tableName)
        );

        if (cdcEnabled == 0)
        {
            string enableTableCdc =
                $@"USE {Singleton<O24OpenAPIConfiguration>.Instance.YourDatabase}
               EXEC sys.sp_cdc_enable_table
               @source_schema = 'dbo',
               @source_name = @tableName,
               @role_name = NULL;";

            await SqlExecutor.ExecuteSqlScriptAsync(
                enableTableCdc,
                new SqlParameter("@tableName", tableName)
            );
        }
    }

    /// <summary>
    /// Enables the for tables using the specified table names
    /// </summary>
    /// <param name="tableNames">The table names</param>
    public static async Task EnableForTables(HashSet<string> tableNames)
    {
        if (tableNames == null || tableNames.Count < 1)
        {
            return;
        }

        foreach (var tableName in tableNames)
        {
            await EnableForTable(tableName);
        }
    }

    /// <summary>
    /// Get key columns for the specified table
    /// </summary>
    /// <param name="tableName"></param>
    /// <returns></returns>
    private static string[] GetKeyColumns(string tableName)
    {
        if (string.IsNullOrWhiteSpace(tableName))
            return [];

        try
        {
            var keyService = EngineContext.Current.Resolve<ICdcKeyConfigService>();
            if (keyService == null)
            {
                BusinessLogHelper.Warning(
                    "CDC GetKeyColumns: ICdcKeyConfigService is not registered in DI container"
                );
                return [];
            }

            var task = keyService.GetKeyColumnsAsync(tableName);
            var result = task.GetAwaiter().GetResult();

            return result ?? [];
        }
        catch (Exception ex)
        {
            BusinessLogHelper.Error(ex, $"CDC GetKeyColumns error for table {tableName}");
            return [];
        }
    }
}
