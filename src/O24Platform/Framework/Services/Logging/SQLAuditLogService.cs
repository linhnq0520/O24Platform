using O24OpenAPI.Core.SeedWork;
using O24OpenAPI.Framework.Domain;

namespace O24OpenAPI.Framework.Services.Logging;

/// <summary>
/// The sql audit log service class
/// </summary>
/// <seealso cref="ISQLAuditLogService"/>
public class SQLAuditLogService(IRepository<SQLAuditLog> sqlAuditLogRepo) : ISQLAuditLogService
{
    /// <summary>
    /// The sql audit log repo
    /// </summary>
    private readonly IRepository<SQLAuditLog> _sqlAuditLogRepo = sqlAuditLogRepo;

    /// <summary>
    /// Inserts the sql audit log
    /// </summary>
    /// <param name="sqlAuditLog">The sql audit log</param>
    public async Task InsertAsync(SQLAuditLog sqlAuditLog)
    {
        await _sqlAuditLogRepo.Insert(sqlAuditLog);
    }

    /// <summary>
    /// Updates the sql audit log
    /// </summary>
    /// <param name="sqlAuditLog">The sql audit log</param>
    public async Task UpdateAsync(SQLAuditLog sqlAuditLog)
    {
        await _sqlAuditLogRepo.Update(sqlAuditLog);
    }
}
