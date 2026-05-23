using LinKit.Core.Abstractions;
using LinqToDB.Async;
using O24OpenAPI.Core.Caching;
using O24OpenAPI.Core.Domain;
using O24OpenAPI.Core.SeedWork;
using O24OpenAPI.Data;

namespace O24OpenAPI.Framework.Repositories;

public interface IEntityAuditRepository : IRepository<EntityAudit>
{
    Task<List<EntityAudit>> GetByExecutionIdAsync(
        string executionId,
        CancellationToken cancellationToken = default
    );

    Task ClearOldAudits();
}

[RegisterService(Lifetime.Scoped)]
public class EntityAuditRepository(
    IDataConnectionFactory dataConnectionFactory,
    IStaticCacheManager staticCacheManager
) : BaseRepository<EntityAudit>(dataConnectionFactory, staticCacheManager), IEntityAuditRepository
{
    public async Task ClearOldAudits()
    {
        await DeleteWhere(s => s.CreatedOnUtc.Value.Date < DateTime.UtcNow.Date);
    }

    public async Task<List<EntityAudit>> GetByExecutionIdAsync(
        string executionId,
        CancellationToken cancellationToken = default
    )
    {
        List<EntityAudit> query = await Table
            .Where(al => al.ExecutionId == executionId)
            .ToListAsync(cancellationToken);
        return query;
    }
}
