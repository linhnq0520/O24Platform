using O24OpenAPI.Framework.Domain;

namespace O24OpenAPI.Framework.Services.Logging;

/// <summary>
/// The isql audit log service interface
/// </summary>
public interface ISQLAuditLogService
{
    /// <summary>
    /// Inserts the sql audit log
    /// </summary>
    /// <param name="sqlAuditLog">The sql audit log</param>
    Task InsertAsync(SQLAuditLog sqlAuditLog);
}
