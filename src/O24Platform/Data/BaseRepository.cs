using System.Linq.Expressions;
using System.Reflection;
using LinKit.Json.Runtime;
using LinqToDB;
using LinqToDB.Async;
using LinqToDB.Data;
using O24OpenAPI.Core;
using O24OpenAPI.Core.Caching;
using O24OpenAPI.Core.Configuration;
using O24OpenAPI.Core.Constants;
using O24OpenAPI.Core.Domain;
using O24OpenAPI.Core.Infrastructure;
using O24OpenAPI.Data.System.Linq;

namespace O24OpenAPI.Data;

public abstract class BaseRepository<TEntity>(
    IDataConnectionFactory dataConnectionFactory,
    IStaticCacheManager staticCacheManager
)
    where TEntity : BaseEntity, new()
{
    private readonly DataConnection _dataConnection = dataConnectionFactory.GetOrCreateConnection();
    public IQueryable<TEntity> Table => _dataConnection.GetTable<TEntity>();

    protected virtual async Task<IList<TEntity>?> GetEntities(
        Func<Task<IList<TEntity>>> getAll,
        Func<IStaticCacheManager, CacheKey>? getCacheKey,
        CancellationToken cancellationToken = default
    )
    {
        if (getCacheKey == null)
        {
            return await getAll();
        }

        CacheKey cacheKey = CachingKey.EntityKey<TEntity>(
            Singleton<O24OpenAPIConfiguration>.Instance!.YourServiceID,
            "all"
        );
        IList<TEntity>? entities1 = await staticCacheManager.Get(cacheKey, getAll);
        return entities1;
    }

    public virtual Task<TEntity?> GetById(
        int id,
        Func<IStaticCacheManager, CacheKey>? getCacheKey = null,
        CancellationToken cancellationToken = default
    )
    {
        if (id == 0)
        {
            return Task.FromResult(default(TEntity));
        }

        return getEntity();

        async Task<TEntity?> getEntity()
        {
            return await Table.FirstOrDefaultAsync(
                entity => entity.Id == id,
                token: cancellationToken
            );
        }
    }

    public virtual Task<TEntity?> GetByIdAsync(
        int id,
        Func<IStaticCacheManager, CacheKey>? getCacheKey = null,
        CancellationToken cancellationToken = default
    )
    {
        if (id == 0)
        {
            return Task.FromResult(default(TEntity));
        }

        return getEntity();

        async Task<TEntity?> getEntity()
        {
            return await Table.FirstOrDefaultAsync(
                entity => entity.Id == id,
                token: cancellationToken
            );
        }
    }

    public virtual async Task<IList<TEntity>> GetByIds(
        IList<int> ids,
        Func<IStaticCacheManager, CacheKey>? getCacheKey = null,
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

        CacheKey cacheKey = CachingKey.EntityKeyWithService<TEntity>(
            ids.Select(id => id.ToString()).ToList()
        );

        return await staticCacheManager.Get(cacheKey, getByIds) ?? [];

        async Task<IList<TEntity>> getByIds()
        {
            IQueryable<TEntity> query = Table;
            List<TEntity> entries = await query
                .Where(entry => ids.Contains(entry.Id))
                .ToListAsync();
            List<TEntity> sortedEntries = [];
            foreach (int id in ids)
            {
                TEntity? sortedEntry = entries.Find(entry => entry.Id == id);
                if (sortedEntry != null)
                {
                    sortedEntries.Add(sortedEntry);
                }
            }

            return sortedEntries;
        }
    }

    public virtual async Task<IList<TEntity>> GetAll(
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? func = null,
        Func<IStaticCacheManager, CacheKey>? getCacheKey = null,
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
        Func<IQueryable<TEntity>, Task<IQueryable<TEntity>>>? func = null,
        Func<IStaticCacheManager, CacheKey>? getCacheKey = null,
        CancellationToken cancellationToken = default
    )
    {
        return await getAll();

        async Task<IList<TEntity>> getAll()
        {
            IQueryable<TEntity> query = Table;
            IQueryable<TEntity> queryable = func != null ? await func(query) : query;
            query = queryable;
            return await query.ToListAsync();
        }
    }

    public virtual Task<IPagedList<TEntity>> GetAllPaged(
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? func = null,
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
        Func<IQueryable<TEntity>, Task<IQueryable<TEntity>>>? func = null,
        int pageIndex = 0,
        int pageSize = 2147483647,
        bool getOnlyTotalCount = false,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<TEntity> queryable;
        queryable = func != null ? await func(Table) : Table;

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

    public virtual async Task<TEntity?> GetByFields(
        Dictionary<string, string> searchInput,
        CancellationToken cancellationToken = default
    )
    {
        List<TEntity> entities = await SearchByFields(searchInput, cancellationToken);
        return entities.Count != 0 ? entities.FirstOrDefault() : default!;
    }

    public virtual Task<TEntity?> Insert(
        TEntity entity,
        CancellationToken cancellationToken = default
    )
    {
        return InsertAsync(entity, cancellationToken);
    }

    public virtual async Task<TEntity?> InsertAsync(
        TEntity entity,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(entity);

        entity.CreatedOnUtc = DateTime.UtcNow;
        entity.UpdatedOnUtc = DateTime.UtcNow;

        entity.Id = await _dataConnection.InsertWithInt32IdentityAsync(
            entity,
            token: cancellationToken
        );
        if (entity.IsAuditable())
        {
            WorkContext workContext = EngineContext.Current.ResolveRequired<WorkContext>();
            EntityAudit entityAudit = new()
            {
                EntityName = typeof(TEntity).Name,
                EntityId = entity.Id,
                UserId = workContext.UserContext.UserId,
                ExecutionId = workContext.ExecutionId,
                ActionType = EntityAuditActionType.Insert,
                Changes = string.Empty,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
            };
            await _dataConnection.InsertAsync(entityAudit, token: cancellationToken);
        }

        return entity;
    }

    public virtual async Task<TEntity?> InsertWithoutAuditAsync(
        TEntity entity,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(entity);

        entity.CreatedOnUtc = DateTime.UtcNow;
        entity.UpdatedOnUtc = DateTime.UtcNow;

        entity.Id = await _dataConnection.InsertWithInt32IdentityAsync(
            entity,
            token: cancellationToken
        );

        return entity;
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

        await _dataConnection.BulkCopyAsync(entities, cancellationToken);
    }

    /// <summary>
    /// Persists one UPDATE <see cref="EntityAudit"/> from a before/after pair (e.g. after atomic <c>Set</c> updates).
    /// Uses the same <see cref="BaseEntity.GetChanges"/> / JSON shape as <see cref="Update"/> so server-side revert can apply deltas consistently.
    /// </summary>
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
        await _dataConnection.InsertAsync(entityAudit, token: cancellationToken);
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

        await _dataConnection.UpdateAsync(entity, token: cancellationToken);

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

        await _dataConnection.UpdateAsync(entity, token: cancellationToken);

        if (entity.IsAuditable())
        {
            await InsertEntityAuditForUpdateIfAuditableAsync(
                entity,
                oldSnapshot ?? new TEntity(),
                cancellationToken
            );
        }
    }

    public virtual async Task Delete(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        await _dataConnection.DeleteAsync(entity, token: cancellationToken);
        if (entity.IsAuditable())
        {
            WorkContext workContext = EngineContext.Current.ResolveRequired<WorkContext>();
            List<AuditDiff>? changes = new TEntity().GetChanges(entity);
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
            await _dataConnection.InsertAsync(entityAudit, token: cancellationToken);
        }
    }

    public virtual async Task DeleteAsync(
        TEntity entity,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(entity);
        await _dataConnection.DeleteAsync(entity, token: cancellationToken);
    }

    public virtual async Task DeleteByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken: cancellationToken);
        if (entity == null)
        {
            return;
        }
        await DeleteAsync(entity, cancellationToken);
    }

    public virtual async Task BulkDelete(
        IList<TEntity> entities,
        CancellationToken cancellationToken = default
    )
    {
        if (entities.All(entity => entity.Id == 0))
        {
            foreach (TEntity entity in entities)
            {
                await _dataConnection.DeleteAsync(entity, token: cancellationToken);
            }

            return;
        }

        await Table
            .Where(entity => entities.Select(x => x.Id).Contains(entity.Id))
            .DeleteAsync(token: cancellationToken);
    }

    public virtual Task UpdateNoAudit(
        IQueryable<TEntity> query,
        string propertyName,
        string value,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(query);
        return _dataConnection.UpdateAsync(
            query.Set(x => Sql.Property<string>(x, propertyName), value),
            token: cancellationToken
        );
    }

    /// <summary>
    /// Loads matching rows before the bulk UPDATE, writes <see cref="EntityAudit"/> per changed row when the entity type is auditable.
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

        await _dataConnection.UpdateAsync(
            query.Set(x => Sql.Property<string>(x, propertyName), value),
            token: cancellationToken
        );

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
            await _dataConnection.InsertAsync(entityAudit, token: cancellationToken);
        }
    }

    public virtual Task<int> DeleteWhere(
        Expression<Func<TEntity, bool>> predicate,
        int batchSize = 0,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return _dataConnection
            .GetTable<TEntity>()
            .Where(predicate)
            .DeleteAsync(token: cancellationToken);
    }

    public virtual Task<TEntity?> LoadOriginalCopy(
        TEntity entity,
        CancellationToken cancellationToken = default
    )
    {
        return Table.FirstOrDefaultAsync(e => e.Id == entity.Id, token: cancellationToken);
    }

    public virtual Task Truncate(
        bool resetIdentity = false,
        CancellationToken cancellationToken = default
    )
    {
        return _dataConnection
            .GetTable<TEntity>()
            .TruncateAsync(resetIdentity, token: cancellationToken);
    }

    public virtual IQueryable<TEntity> TableFilter(Expression<Func<TEntity, bool>> filter)
    {
        return Table.Where(filter);
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
            PropertyInfo? property = typeof(TEntity).GetProperty(item.Key);
            if (property != null)
            {
                if (property.PropertyType == typeof(string))
                {
                    if (!string.IsNullOrEmpty(item.Value))
                    {
                        source = source.Where(
                            (Expression<Func<TEntity, bool>>)(
                                e => Sql.Property<string>(e, item.Key) == item.Value
                            )
                        );
                    }
                }
                else if (property.PropertyType == typeof(int) && !string.IsNullOrEmpty(item.Value))
                {
                    source = source.Where(
                        (Expression<Func<TEntity, bool>>)(
                            e => Sql.Property<int>(e, item.Key) == int.Parse(item.Value)
                        )
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
            await _dataConnection.UpdateAsync(entity, token: cancellationToken);
        }
    }

    public async Task DeleteById(int id, CancellationToken cancellationToken = default)
    {
        var entity = await GetById(id, cancellationToken: cancellationToken);
        if (entity == null)
        {
            return;
        }
        await Delete(entity, cancellationToken);
    }

    public Task BulkCopyAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default
    )
    {
        return _dataConnection.BulkCopyAsync(entities, cancellationToken);
    }

    public Task<int> CountAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default
    )
    {
        return Table.CountAsync(predicate ?? (e => true), token: cancellationToken);
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
}
