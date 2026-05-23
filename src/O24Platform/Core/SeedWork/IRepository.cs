using O24OpenAPI.Core.Caching;
using System.Linq.Expressions;

namespace O24OpenAPI.Core.SeedWork;

public interface IRepository<TEntity>
    where TEntity : BaseEntity
{
    Task<TEntity?> GetById(
        int id,
        Func<IStaticCacheManager, CacheKey>? getCacheKey = null,
        CancellationToken cancellationToken = default
    );
    Task<TEntity?> GetByIdAsync(
        int id,
        Func<IStaticCacheManager, CacheKey>? getCacheKey = null,
        CancellationToken cancellationToken = default
    );
    Task<IList<TEntity>> GetByIds(
        IList<int> ids,
        Func<IStaticCacheManager, CacheKey>? getCacheKey = null,
        CancellationToken cancellationToken = default
    );
    Task<IList<TEntity>> GetAll(
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? func = null,
        Func<IStaticCacheManager, CacheKey>? getCacheKey = null,
        CancellationToken cancellationToken = default
    );
    Task<IList<TEntity>> GetAll(
        Func<IQueryable<TEntity>, Task<IQueryable<TEntity>>>? func = null,
        Func<IStaticCacheManager, CacheKey>? getCacheKey = null,
        CancellationToken cancellationToken = default
    );
    Task<IPagedList<TEntity>> GetAllPaged(
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? func = null,
        int pageIndex = 0,
        int pageSize = 2147483647,
        bool getOnlyTotalCount = false,
        CancellationToken cancellationToken = default
    );
    Task<IPagedList<TEntity>> GetAllPaged(
        Func<IQueryable<TEntity>, Task<IQueryable<TEntity>>>? func = null,
        int pageIndex = 0,
        int pagSize = 2147483647,
        bool getOnlyTotalCount = false,
        CancellationToken cancellationToken = default
    );
    Task<List<TEntity>> SearchByFields(
        Dictionary<string, string> searchInput,
        CancellationToken cancellationToken = default
    );
    Task<TEntity?> GetByFields(
        Dictionary<string, string> searchInput,
        CancellationToken cancellationToken = default
    );
    Task<TEntity?> InsertAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task<TEntity?> Insert(TEntity entity, CancellationToken cancellationToken = default);
    Task BulkInsert(IList<TEntity> entities, CancellationToken cancellationToken = default);
    Task Update(TEntity entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task InsertEntityAuditForUpdateIfAuditableAsync(
        TEntity newState,
        TEntity oldState,
        CancellationToken cancellationToken = default
    );
    Task Delete(TEntity entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task DeleteByIdAsync(int id, CancellationToken cancellationToken = default);
    Task DeleteById(int id, CancellationToken cancellationToken = default);
    Task BulkDelete(IList<TEntity> entities, CancellationToken cancellationToken = default);
    Task UpdateNoAudit(
        IQueryable<TEntity> query,
        string propertyName,
        string value,
        CancellationToken cancellationToken = default
    );
    Task FilterAndUpdate(
        Dictionary<string, string> searchInput,
        string propertyName,
        string value,
        CancellationToken cancellationToken = default
    );
    Task FilterAndUpdateWithAudit(
        Dictionary<string, string> searchInput,
        string propertyName,
        string value,
        CancellationToken cancellationToken = default
    );
    Task UpdateNoAuditWithAudit(
        IQueryable<TEntity> query,
        string propertyName,
        string value,
        CancellationToken cancellationToken = default
    );
    Task<int> DeleteWhere(
        Expression<Func<TEntity, bool>> predicate,
        int batchSize = 0,
        CancellationToken cancellationToken = default
    );
    Task<TEntity?> LoadOriginalCopy(TEntity entity, CancellationToken cancellationToken = default);
    Task Truncate(bool resetIdentity = false, CancellationToken cancellationToken = default);
    IQueryable<TEntity> Table { get; }
    IQueryable<TEntity> GetTable();
    IQueryable<TEntity> TableFilterExpression(Expression<Func<TEntity, bool>> filter);
    IQueryable<TEntity> TableFilter(Dictionary<string, string> searchInput);
    Task FilterAndDelete(
        Dictionary<string, string> searchInput,
        CancellationToken cancellationToken = default
    );
    Task UpdateRangeNoAuditAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default
    );
    Task<TEntity?> InsertWithoutAuditAsync(
        TEntity entity,
        CancellationToken cancellationToken = default
    );
}
