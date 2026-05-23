using System.Linq.Expressions;
using System.Reflection;
using System.Transactions;
using LinKit.Json.Runtime;
using LinqToDB;
using LinqToDB.Async;
using O24OpenAPI.Core;
using O24OpenAPI.Core.Caching;
using O24OpenAPI.Core.Configuration;
using O24OpenAPI.Core.Constants;
using O24OpenAPI.Core.Domain;
using O24OpenAPI.Core.Extensions;
using O24OpenAPI.Core.Infrastructure;
using O24OpenAPI.Core.SeedWork;
using O24OpenAPI.Data.System.Linq;

namespace O24OpenAPI.Data;

/// <summary>
/// The entity repository class
/// </summary>
/// <seealso cref="IRepository{TEntity}"/>
public class EntityRepository<TEntity>(
    IO24OpenAPIDataProvider dataProvider,
    IStaticCacheManager staticCacheManager
) : IRepository<TEntity>
    where TEntity : BaseEntity, new()
{
    private readonly IO24OpenAPIDataProvider _dataProvider = dataProvider;
    private readonly IStaticCacheManager _staticCacheManager = staticCacheManager;

    protected virtual async Task<IList<TEntity>> GetEntities(
        Func<Task<IList<TEntity>>> getAll,
        Func<IStaticCacheManager, CacheKey> getCacheKey,
        CancellationToken cancellationToken = default
    )
    {
        if (getCacheKey == null)
        {
            return await getAll();
        }

        CacheKey cacheKey = CachingKey.EntityKey<TEntity>(
            Singleton<O24OpenAPIConfiguration>.InstanceRequired.YourServiceID,
            "all"
        );
        IList<TEntity> entities1 = await _staticCacheManager.Get(cacheKey, getAll);
        return entities1;
    }

    public virtual Task<TEntity> GetById(
        int id,
        Func<IStaticCacheManager, CacheKey> getCacheKey = null,
        CancellationToken cancellationToken = default
    )
    {
        if (id == 0)
        {
            return null;
        }

        return getEntity();

        async Task<TEntity> getEntity()
        {
            return await Table.FirstOrDefaultAsync(entity => entity.Id == id);
        }
    }

    public virtual Task<TEntity> GetByIdAsync(
        int id,
        Func<IStaticCacheManager, CacheKey> getCacheKey = null,
        CancellationToken cancellationToken = default
    )
    {
        if (id == 0)
        {
            return null;
        }

        return getEntity();

        async Task<TEntity> getEntity()
        {
            return await Table.FirstOrDefaultAsync(entity => entity.Id == id);
        }
    }

    public virtual async Task<IList<TEntity>> GetByIds(
        IList<int> ids,
        Func<IStaticCacheManager, CacheKey> getCacheKey = null,
        CancellationToken cancellationToken = default
    )
    {
        IList<int> list = ids;
        if (list == null || !list.Any())
        {
            return [];
        }

        if (getCacheKey == null)
        {
            return await getByIds();
        }

        CacheKey cacheKey = CachingKey.EntityKeyWithService<TEntity>(ids.ToListString());

        return await _staticCacheManager.Get(cacheKey, getByIds);

        async Task<IList<TEntity>> getByIds()
        {
            IQueryable<TEntity> query = Table;
            List<TEntity> entries = await query
                .Where(entry => ids.Contains(entry.Id))
                .ToListAsync();
            List<TEntity> sortedEntries = [];
            foreach (int id in ids)
            {
                TEntity sortedEntry = entries.Find(entry => entry.Id == id);
                if (sortedEntry != null)
                {
                    sortedEntries.Add(sortedEntry);
                }
            }

            return sortedEntries;
        }
    }

    public virtual async Task<IList<TEntity>> GetAll(
        Func<IQueryable<TEntity>, IQueryable<TEntity>> func = null,
        Func<IStaticCacheManager, CacheKey> getCacheKey = null,
        CancellationToken cancellationToken = default
    )
    {
        return await getAll();

        async Task<IList<TEntity>> getAll()
        {
            IQueryable<TEntity> query = Table;
            query = (func != null) ? func(query) : query;
            return await query.ToListAsync();
        }
    }

    public virtual async Task<IList<TEntity>> GetAll(
        Func<IQueryable<TEntity>, Task<IQueryable<TEntity>>> func = null,
        Func<IStaticCacheManager, CacheKey> getCacheKey = null,
        CancellationToken cancellationToken = default
    )
    {
        return await getAll();

        async Task<IList<TEntity>> getAll()
        {
            IQueryable<TEntity> query = Table;
            IQueryable<TEntity> queryable = (func == null) ? query : (await func(query));
            query = queryable;
            return await query.ToListAsync();
        }
    }

    public virtual Task<IPagedList<TEntity>> GetAllPaged(
        Func<IQueryable<TEntity>, IQueryable<TEntity>> func = null,
        int pageIndex = 0,
        int pageSize = 2147483647,
        bool getOnlyTotalCount = false,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<TEntity> query = func != null ? func(Table) : Table;
        Task<IPagedList<TEntity>> pagedList = query.ToPagedList(
            pageIndex,
            pageSize,
            getOnlyTotalCount
        );
        return pagedList;
    }

    public virtual async Task<IPagedList<TEntity>> GetAllPaged(
        Func<IQueryable<TEntity>, Task<IQueryable<TEntity>>> func = null,
        int pageIndex = 0,
        int pageSize = 2147483647,
        bool getOnlyTotalCount = false,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<TEntity> queryable;
        if (func != null)
        {
            queryable = await func(Table);
        }
        else
        {
            queryable = Table;
        }

        IQueryable<TEntity> query = queryable;
        IPagedList<TEntity> pagedList = await query.ToPagedList(
            pageIndex,
            pageSize,
            getOnlyTotalCount
        );
        return pagedList;
    }

    public virtual Task<List<TEntity>> SearchByFields(
        Dictionary<string, string> searchInput,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<TEntity> query = TableFilter(searchInput);
        return query.ToListAsync();
    }

    public virtual async Task<TEntity> GetByFields(
        Dictionary<string, string> searchInput,
        CancellationToken cancellationToken = default
    )
    {
        List<TEntity> entities = await SearchByFields(searchInput, cancellationToken);
        TEntity byFields =
            entities != null && entities.Count != 0 ? entities.FirstOrDefault() : default(TEntity);
        return byFields;
    }

    public virtual Task<TEntity> Insert(
        TEntity entity,
        CancellationToken cancellationToken = default
    )
    {
        return InsertAsync(entity, cancellationToken);
    }

    public virtual async Task<TEntity> InsertAsync(
        TEntity entity,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(entity);

        entity.CreatedOnUtc = DateTime.UtcNow;
        entity.UpdatedOnUtc = DateTime.UtcNow;

        TEntity entity1 = await _dataProvider.InsertEntity(entity, cancellationToken);
        if (entity.IsAuditable())
        {
            WorkContext workContext = EngineContext.Current.Resolve<WorkContext>();
            EntityAudit entityAudit = new()
            {
                EntityName = typeof(TEntity).Name,
                EntityId = entity1.Id,
                UserId = workContext.UserContext.UserId,
                ExecutionId = workContext.ExecutionId,
                ActionType = EntityAuditActionType.Insert,
                Changes = string.Empty,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
            };
            await _dataProvider.InsertEntity(entityAudit, cancellationToken);
        }

        return entity1;
    }

    public async Task<TEntity?> InsertWithoutAuditAsync(
        TEntity entity,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(entity);

        entity.CreatedOnUtc = DateTime.UtcNow;
        entity.UpdatedOnUtc = DateTime.UtcNow;

        TEntity entity1 = await _dataProvider.InsertEntity(entity, cancellationToken);
        return entity1;
    }

    public virtual async Task BulkInsert(
        IList<TEntity> entities,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(entities);
        if (entities.Count == 0)
        {
            return;
        }

        using TransactionScope transaction = new(TransactionScopeAsyncFlowOption.Enabled);
        await _dataProvider.BulkInsertEntities(entities, cancellationToken: cancellationToken);
        transaction.Complete();
    }

    public virtual async Task Update(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        entity.UpdatedOnUtc = DateTime.UtcNow;

        TEntity? oldSnapshot = null;
        if (entity.IsAuditable())
        {
            oldSnapshot = await LoadOriginalCopy(entity, cancellationToken);
        }

        await _dataProvider.UpdateEntity(entity, cancellationToken);

        if (entity.IsAuditable())
        {
            await InsertEntityAuditForUpdateIfAuditableAsync(
                entity,
                oldSnapshot ?? new TEntity(),
                cancellationToken
            );
        }
    }

    public virtual async Task UpdateAsync(
        TEntity entity,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(entity);
        entity.UpdatedOnUtc = DateTime.UtcNow;

        TEntity? oldSnapshot = null;
        if (entity.IsAuditable())
        {
            oldSnapshot = await LoadOriginalCopy(entity, cancellationToken);
        }

        await _dataProvider.UpdateEntity(entity, cancellationToken);

        if (entity.IsAuditable())
        {
            await InsertEntityAuditForUpdateIfAuditableAsync(
                entity,
                oldSnapshot ?? new TEntity(),
                cancellationToken
            );
        }
    }

    public virtual async Task InsertEntityAuditForUpdateIfAuditableAsync(
        TEntity newState,
        TEntity oldState,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(newState);
        ArgumentNullException.ThrowIfNull(oldState);

        if (!newState.IsAuditable())
        {
            return;
        }

        List<AuditDiff>? changes = newState.GetChanges(oldState);
        if (changes is not { Count: > 0 })
        {
            return;
        }

        WorkContext workContext = EngineContext.Current.ResolveRequired<WorkContext>();
        EntityAudit entityAudit = new()
        {
            EntityName = typeof(TEntity).Name,
            EntityId = newState.Id,
            UserId = workContext.UserContext.UserId,
            ExecutionId = workContext.ExecutionId,
            ActionType = EntityAuditActionType.Update,
            Changes = changes.ToJson(),
            CreatedOnUtc = DateTime.UtcNow,
            UpdatedOnUtc = DateTime.UtcNow,
        };
        await _dataProvider.InsertEntity(entityAudit, cancellationToken);
    }

    public virtual async Task Delete(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        await _dataProvider.DeleteEntity(entity, cancellationToken);
        if (entity.IsAuditable())
        {
            WorkContext workContext = EngineContext.Current.Resolve<WorkContext>();
            List<AuditDiff> changes = new TEntity().GetChanges(entity);
            EntityAudit entityAudit = new()
            {
                EntityName = typeof(TEntity).Name,
                EntityId = entity.Id,
                UserId = workContext.UserContext.UserId,
                ExecutionId = workContext.ExecutionId,
                ActionType = EntityAuditActionType.Delete,
                Changes = changes.ToJson(),
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
            };
            await _dataProvider.InsertEntity(entityAudit, cancellationToken);
        }
    }

    public virtual async Task DeleteAsync(
        TEntity entity,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(entity);
        await _dataProvider.DeleteEntity(entity, cancellationToken);
    }

    public virtual async Task DeleteByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        TEntity entity = await GetByIdAsync(id, cancellationToken: cancellationToken);
        await DeleteAsync(entity, cancellationToken);
    }

    public virtual Task BulkDelete(
        IList<TEntity> entities,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(entities);
        return _dataProvider.BulkDeleteEntities(entities, cancellationToken);
    }

    public virtual Task UpdateNoAudit(
        IQueryable<TEntity> query,
        string propertyName,
        string value,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(query);
        return _dataProvider.UpdateEntities(query, propertyName, value, cancellationToken);
    }

    /// <summary>
    /// Loads matching rows before bulk UPDATE; writes <see cref="EntityAudit"/> per changed row when the entity type is auditable.
    /// </summary>
    public virtual async Task UpdateNoAuditWithAudit(
        IQueryable<TEntity> query,
        string propertyName,
        string value,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(query);

        var probe = new TEntity();
        if (!probe.IsAuditable())
        {
            await UpdateNoAudit(query, propertyName, value, cancellationToken)
                .ConfigureAwait(false);
            return;
        }

        List<TEntity> oldRows = await query.ToListAsync(cancellationToken);
        if (oldRows.Count == 0)
        {
            return;
        }

        await _dataProvider.UpdateEntities(query, propertyName, value, cancellationToken);

        List<int> ids = [.. oldRows.ConvertAll(static o => o.Id)];
        List<TEntity> freshRows = await Table
            .Where(e => ids.Contains(e.Id))
            .ToListAsync(cancellationToken);
        Dictionary<int, TEntity> freshById = freshRows.ToDictionary(e => e.Id);

        WorkContext workContext = EngineContext.Current.ResolveRequired<WorkContext>();
        foreach (TEntity oldRow in oldRows)
        {
            if (!freshById.TryGetValue(oldRow.Id, out TEntity? newRow))
            {
                continue;
            }

            List<AuditDiff>? changes = newRow.GetChanges(oldRow);
            if (changes is not { Count: > 0 })
            {
                continue;
            }

            EntityAudit entityAudit = new()
            {
                EntityName = typeof(TEntity).Name,
                EntityId = newRow.Id,
                UserId = workContext.UserContext.UserId,
                ExecutionId = workContext.ExecutionId,
                ActionType = EntityAuditActionType.Update,
                Changes = changes.ToJson(),
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
            };
            await _dataProvider.InsertEntity(entityAudit, cancellationToken);
        }
    }

    public virtual Task FilterAndUpdate(
        Dictionary<string, string> searchInput,
        string propertyName,
        string value,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<TEntity> query = TableFilter(searchInput);
        return UpdateNoAudit(query, propertyName, value, cancellationToken);
    }

    public virtual Task FilterAndUpdateWithAudit(
        Dictionary<string, string> searchInput,
        string propertyName,
        string value,
        CancellationToken cancellationToken = default
    )
    {
        return UpdateNoAuditWithAudit(
            TableFilter(searchInput),
            propertyName,
            value,
            cancellationToken
        );
    }

    public virtual Task<int> DeleteWhere(
        Expression<Func<TEntity, bool>> predicate,
        int batchSize = 0,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return _dataProvider.BulkDeleteEntities(predicate, batchSize, cancellationToken);
    }

    public virtual Task<TEntity> LoadOriginalCopy(
        TEntity entity,
        CancellationToken cancellationToken = default
    )
    {
        return _dataProvider.GetTable<TEntity>().FirstOrDefaultAsync(e => e.Id == entity.Id);
    }

    public virtual Task Truncate(
        bool resetIdentity = false,
        CancellationToken cancellationToken = default
    )
    {
        return _dataProvider.Truncate<TEntity>(resetIdentity);
    }

    public virtual IQueryable<TEntity> Table => _dataProvider.GetTable<TEntity>();

    public virtual IQueryable<TEntity> TableFilter(Expression<Func<TEntity, bool>> filter)
    {
        return _dataProvider.GetTable<TEntity>().Where(filter);
    }

    public IQueryable<TEntity> TableFilterExpression(Expression<Func<TEntity, bool>> filter)
    {
        throw new NotImplementedException();
    }

    public virtual IQueryable<TEntity> TableFilter(Dictionary<string, string> searchInput)
    {
        IQueryable<TEntity> source = Table;
        foreach (KeyValuePair<string, string> keyValuePair in searchInput)
        {
            KeyValuePair<string, string> item = keyValuePair;
            PropertyInfo property = typeof(TEntity).GetProperty(item.Key);
            if (property != null)
            {
                if (property.PropertyType == typeof(string))
                {
                    if (!string.IsNullOrEmpty(item.Value))
                    {
                        source = source.Where(e => Sql.Property<string>(e, item.Key) == item.Value);
                    }
                }
                else if (property.PropertyType == typeof(int) && !string.IsNullOrEmpty(item.Value))
                {
                    source = source.Where(e =>
                        Sql.Property<int>(e, item.Key) == int.Parse(item.Value)
                    );
                }
            }
        }

        return source;
    }

    public Task FilterAndDelete(
        Dictionary<string, string> searchInput,
        CancellationToken cancellationToken = default
    )
    {
        throw new NotImplementedException();
    }

    public IQueryable<TEntity> GetTable()
    {
        return Table;
    }

    public virtual async Task UpdateRangeNoAuditAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default
    )
    {
        foreach (TEntity entity in entities)
        {
            await _dataProvider.UpdateEntity(entity, cancellationToken);
        }
    }

    public async Task DeleteById(int id, CancellationToken cancellationToken = default)
    {
        TEntity entity = await GetById(id, cancellationToken: cancellationToken);
        await Delete(entity, cancellationToken);
    }

    public Task BulkCopyAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default
    )
    {
        return _dataProvider.BulkCopyAsync(entities, cancellationToken);
    }
}
