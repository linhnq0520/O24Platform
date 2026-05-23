using System.Globalization;
using System.Text;
using LinKit.Json.Runtime;
using O24OpenAPI.Core.Constants;
using O24OpenAPI.Core.Domain;
using O24OpenAPI.Data;

namespace O24OpenAPI.Framework.Services;

public class EntityAuditReverter
{
    public static async Task<string> GenerateRevertSqlBlock(
        List<EntityAudit> entityAudits,
        string executionId,
        string schema = "dbo",
        DataProviderType dataProviderType = DataProviderType.SqlServer
    )
    {
        if (entityAudits.Count == 0)
            return string.Empty;

        StringBuilder sqlBuilder = new();

        // Begin Transaction - syntax varies by provider
        AppendBeginTransaction(sqlBuilder, dataProviderType);

        // Revert theo thứ tự ngược lại (LIFO)
        List<EntityAudit> reversedAudits = entityAudits.OrderByDescending(a => a.Id).ToList();

        foreach (EntityAudit audit in reversedAudits)
        {
            List<AuditDiff> auditDiffs = audit.Changes.FromJson<List<AuditDiff>>();
            string revertSql = GenerateRevertSqlForAudit(
                audit,
                auditDiffs,
                schema,
                dataProviderType
            );

            if (!string.IsNullOrEmpty(revertSql))
            {
                sqlBuilder.AppendLine(
                    $"    {GetCommentPrefix(dataProviderType)} Revert {audit.ActionType} on {audit.EntityName} (Id: {audit.EntityId})"
                );
                sqlBuilder.AppendLine($"    {revertSql}");
                sqlBuilder.AppendLine();
            }
        }

        // Commit and error handling
        AppendCommitAndErrorHandling(sqlBuilder, dataProviderType, executionId);

        return sqlBuilder.ToString();
    }

    private static void AppendBeginTransaction(
        StringBuilder sqlBuilder,
        DataProviderType dataProviderType
    )
    {
        switch (dataProviderType)
        {
            case DataProviderType.SqlServer:
                sqlBuilder.AppendLine("BEGIN TRY");
                sqlBuilder.AppendLine("    BEGIN TRANSACTION;");
                sqlBuilder.AppendLine();
                break;

            case DataProviderType.Oracle:
                sqlBuilder.AppendLine("BEGIN");
                sqlBuilder.AppendLine();
                break;

            case DataProviderType.PostgreSQL:
                sqlBuilder.AppendLine("DO $$");
                sqlBuilder.AppendLine("BEGIN");
                sqlBuilder.AppendLine();
                break;

            case DataProviderType.MySql:
                sqlBuilder.AppendLine("START TRANSACTION;");
                sqlBuilder.AppendLine();
                break;
        }
    }

    private static void AppendCommitAndErrorHandling(
        StringBuilder sqlBuilder,
        DataProviderType dataProviderType,
        string executionId
    )
    {
        switch (dataProviderType)
        {
            case DataProviderType.SqlServer:
                sqlBuilder.AppendLine("    COMMIT TRANSACTION;");
                sqlBuilder.AppendLine(
                    $"    PRINT 'Revert completed successfully for ExecutionId: {executionId}';"
                );
                sqlBuilder.AppendLine("END TRY");
                sqlBuilder.AppendLine("BEGIN CATCH");
                sqlBuilder.AppendLine("    IF @@TRANCOUNT > 0");
                sqlBuilder.AppendLine("        ROLLBACK TRANSACTION;");
                sqlBuilder.AppendLine("    ");
                sqlBuilder.AppendLine(
                    "    DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();"
                );
                sqlBuilder.AppendLine("    DECLARE @ErrorSeverity INT = ERROR_SEVERITY();");
                sqlBuilder.AppendLine("    DECLARE @ErrorState INT = ERROR_STATE();");
                sqlBuilder.AppendLine("    ");
                sqlBuilder.AppendLine("    RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);");
                sqlBuilder.AppendLine("END CATCH;");
                break;

            case DataProviderType.Oracle:
                sqlBuilder.AppendLine("    COMMIT;");
                sqlBuilder.AppendLine(
                    $"    DBMS_OUTPUT.PUT_LINE('Revert completed successfully for ExecutionId: {executionId}');"
                );
                sqlBuilder.AppendLine("EXCEPTION");
                sqlBuilder.AppendLine("    WHEN OTHERS THEN");
                sqlBuilder.AppendLine("        ROLLBACK;");
                sqlBuilder.AppendLine("        RAISE;");
                sqlBuilder.AppendLine("END;");
                sqlBuilder.AppendLine("/");
                break;

            case DataProviderType.PostgreSQL:
                sqlBuilder.AppendLine(
                    "    RAISE NOTICE 'Revert completed successfully for ExecutionId: %', '"
                        + executionId
                        + "';"
                );
                sqlBuilder.AppendLine("EXCEPTION");
                sqlBuilder.AppendLine("    WHEN OTHERS THEN");
                sqlBuilder.AppendLine("        RAISE;");
                sqlBuilder.AppendLine("END $$;");
                break;

            case DataProviderType.MySql:
                sqlBuilder.AppendLine("COMMIT;");
                sqlBuilder.AppendLine(
                    $"SELECT 'Revert completed successfully for ExecutionId: {executionId}' AS Result;"
                );
                break;
        }
    }

    private static string GetCommentPrefix(DataProviderType dataProviderType)
    {
        return dataProviderType == DataProviderType.MySql ? "#" : "--";
    }

    private static string GenerateRevertSqlForAudit(
        EntityAudit audit,
        List<AuditDiff> diffs,
        string schema,
        DataProviderType dataProviderType
    )
    {
        return audit.ActionType switch
        {
            EntityAuditActionType.Insert => GenerateDeleteSql(audit, schema, dataProviderType),
            EntityAuditActionType.Update => GenerateRestoreOrReverseDeltaSql(
                audit,
                diffs,
                schema,
                dataProviderType
            ),
            EntityAuditActionType.Delete => GenerateInsertSql(
                audit,
                diffs,
                schema,
                dataProviderType
            ),
            _ => string.Empty,
        };
    }

    // Revert INSERT -> DELETE
    private static string GenerateDeleteSql(
        EntityAudit audit,
        string schema,
        DataProviderType dataProviderType
    )
    {
        string tableName = FormatTableName(audit.EntityName, schema, dataProviderType);
        return $"DELETE FROM {tableName} WHERE {FormatColumnName("Id", dataProviderType)} = {audit.EntityId};";
    }

    // Revert UPDATE -> Restore old values OR reverse delta
    private static string GenerateRestoreOrReverseDeltaSql(
        EntityAudit audit,
        List<AuditDiff> diffs,
        string schema,
        DataProviderType dataProviderType
    )
    {
        if (diffs.Count == 0)
            return string.Empty;

        List<string> setClauses = [];

        foreach (AuditDiff diff in diffs)
        {
            string columnName = FormatColumnName(diff.FieldName, dataProviderType);

            if (diff.Delta.HasValue && diff.Delta.Value != 0)
            {
                // Financial transaction - reverse the delta
                decimal reverseAmount = -diff.Delta.Value;
                setClauses.Add($"{columnName} = {columnName} + ({reverseAmount})");
            }
            else if (diff.OldValue != null)
            {
                // Regular field update - restore old value
                setClauses.Add($"{columnName} = {FormatSqlValue(diff.OldValue, dataProviderType)}");
            }
        }

        if (setClauses.Count == 0)
            return string.Empty;

        string tableName = FormatTableName(audit.EntityName, schema, dataProviderType);
        string idColumn = FormatColumnName("Id", dataProviderType);

        return $@"UPDATE {tableName}
SET {string.Join(", ", setClauses)}
WHERE {idColumn} = {audit.EntityId};";
    }

    // Revert DELETE -> INSERT
    private static string GenerateInsertSql(
        EntityAudit audit,
        List<AuditDiff> diffs,
        string schema,
        DataProviderType dataProviderType
    )
    {
        if (diffs.Count == 0)
            return string.Empty;

        string tableName = FormatTableName(audit.EntityName, schema, dataProviderType);
        string idColumn = FormatColumnName("Id", dataProviderType);
        string columns = string.Join(
            ", ",
            diffs.Select(d => FormatColumnName(d.FieldName, dataProviderType))
        );
        string values = string.Join(
            ", ",
            diffs.Select(d => FormatSqlValue(d.OldValue, dataProviderType))
        );

        switch (dataProviderType)
        {
            case DataProviderType.SqlServer:
                return $@"SET IDENTITY_INSERT {tableName} ON;
INSERT INTO {tableName} ({idColumn}, {columns})
VALUES ({audit.EntityId}, {values});
SET IDENTITY_INSERT {tableName} OFF;";

            case DataProviderType.Oracle:
                // Oracle doesn't have IDENTITY_INSERT, use trigger disable/enable if needed
                return $@"INSERT INTO {tableName} ({idColumn}, {columns})
VALUES ({audit.EntityId}, {values});";

            case DataProviderType.PostgreSQL:
                // PostgreSQL handles sequences automatically, but we can override
                return $@"INSERT INTO {tableName} ({idColumn}, {columns})
VALUES ({audit.EntityId}, {values});";

            case DataProviderType.MySql:
                return $@"SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='';
INSERT INTO {tableName} ({idColumn}, {columns})
VALUES ({audit.EntityId}, {values});
SET SQL_MODE=@OLD_SQL_MODE;";

            default:
                return $@"INSERT INTO {tableName} ({idColumn}, {columns})
VALUES ({audit.EntityId}, {values});";
        }
    }

    private static string FormatTableName(
        string tableName,
        string schema,
        DataProviderType dataProviderType
    )
    {
        return dataProviderType switch
        {
            DataProviderType.SqlServer => $"[{schema}].[{tableName}]",
            DataProviderType.Oracle => $"{schema.ToUpper()}.{tableName.ToUpper()}",
            DataProviderType.PostgreSQL => $"\"{schema}\".\"{tableName}\"",
            DataProviderType.MySql => $"`{schema}`.`{tableName}`",
            _ => $"{schema}.{tableName}",
        };
    }

    private static string FormatColumnName(string columnName, DataProviderType dataProviderType)
    {
        return dataProviderType switch
        {
            DataProviderType.SqlServer => $"[{columnName}]",
            DataProviderType.Oracle => columnName.ToUpper(),
            DataProviderType.PostgreSQL => $"\"{columnName}\"",
            DataProviderType.MySql => $"`{columnName}`",
            _ => columnName,
        };
    }

    private static string FormatSqlValue(object value, DataProviderType dataProviderType)
    {
        if (value == null)
            return "NULL";

        return value switch
        {
            string s => FormatStringValue(s, dataProviderType),
            DateTime dt => FormatDateTimeValue(dt, dataProviderType),
            bool b => FormatBoolValue(b, dataProviderType),
            decimal d => d.ToString(CultureInfo.InvariantCulture),
            double d => d.ToString(CultureInfo.InvariantCulture),
            float f => f.ToString(CultureInfo.InvariantCulture),
            _ => value.ToString(),
        };
    }

    private static string FormatStringValue(string value, DataProviderType dataProviderType)
    {
        string escaped = value.Replace("'", "''");

        return dataProviderType switch
        {
            DataProviderType.SqlServer => $"N'{escaped}'",
            DataProviderType.Oracle => $"'{escaped}'",
            DataProviderType.PostgreSQL => $"'{escaped}'",
            DataProviderType.MySql => $"'{escaped}'",
            _ => $"'{escaped}'",
        };
    }

    private static string FormatDateTimeValue(DateTime value, DataProviderType dataProviderType)
    {
        return dataProviderType switch
        {
            DataProviderType.SqlServer => $"'{value:yyyy-MM-dd HH:mm:ss.fff}'",
            DataProviderType.Oracle =>
                $"TO_TIMESTAMP('{value:yyyy-MM-dd HH:mm:ss.fff}', 'YYYY-MM-DD HH24:MI:SS.FF3')",
            DataProviderType.PostgreSQL => $"TIMESTAMP '{value:yyyy-MM-dd HH:mm:ss.fff}'",
            DataProviderType.MySql => $"'{value:yyyy-MM-dd HH:mm:ss.fff}'",
            _ => $"'{value:yyyy-MM-dd HH:mm:ss}'",
        };
    }

    private static string FormatBoolValue(bool value, DataProviderType dataProviderType)
    {
        return dataProviderType switch
        {
            DataProviderType.SqlServer => value ? "1" : "0",
            DataProviderType.Oracle => value ? "1" : "0",
            DataProviderType.PostgreSQL => value ? "TRUE" : "FALSE",
            DataProviderType.MySql => value ? "1" : "0",
            _ => value ? "1" : "0",
        };
    }
}
