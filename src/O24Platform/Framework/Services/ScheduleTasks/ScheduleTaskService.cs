using LinqToDB;
using LinqToDB.Async;
using O24OpenAPI.Core.Caching;
using O24OpenAPI.Core.Domain;
using O24OpenAPI.Core.SeedWork;

namespace O24OpenAPI.Framework.Services.ScheduleTasks;

/// <summary>
/// The schedule task service class
/// </summary>
/// <seealso cref="IScheduleTaskService"/>
public class ScheduleTaskService(
    IRepository<ScheduleTask> taskRepository,
    IStaticCacheManager staticCacheManager
) : IScheduleTaskService
{
    private readonly IRepository<ScheduleTask> _taskRepository = taskRepository;
    private readonly IStaticCacheManager _staticCacheManager = staticCacheManager;

    /// <summary>
    /// Deletes the task
    /// </summary>
    /// <param name="task">The task</param>
    public virtual async Task Delete(ScheduleTask task)
    {
        await _taskRepository.Delete(task);
    }

    /// <summary>
    /// Gets the by id using the specified task id
    /// </summary>
    /// <param name="taskId">The task id</param>
    /// <returns>The by id</returns>
    public virtual async Task<ScheduleTask> GetById(int taskId)
    {
        ScheduleTask byId = await _taskRepository.GetById(taskId, _ => null);
        return byId;
    }

    /// <summary>
    /// Gets the by type using the specified type
    /// </summary>
    /// <param name="type">The type</param>
    /// <returns>The by type</returns>
    public virtual async Task<ScheduleTask> GetByType(string type)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            return null;
        }
        CacheKey cacheKey = CachingKey.EntityKeyWithService<ScheduleTask>(
            nameof(GetByType),
            type
        );
        ScheduleTask byType = await _staticCacheManager.Get(
            cacheKey,
            async () =>
            {
                IQueryable<ScheduleTask> query = _taskRepository.Table;
                query = query.Where(st => st.Type == type);
                query = query.OrderByDescending(t => t.Id);
                ScheduleTask task = await query.FirstOrDefaultAsync();
                ScheduleTask byType1 = task;
                query = null;
                task = null;
                return byType1;
            }
        );
        return byType;
    }

    /// <summary>
    /// Gets the all using the specified show hidden
    /// </summary>
    /// <param name="showHidden">The show hidden</param>
    /// <returns>The all</returns>
    public virtual async Task<IList<ScheduleTask>> GetAll(bool showHidden = false)
    {
        IList<ScheduleTask> tasks = await _taskRepository.GetAll(query =>
        {
            if (!showHidden)
            {
                query = query.Where(t => t.Enabled);
            }
            query = query.OrderByDescending(t => t.Seconds);
            return query;
        });
        IList<ScheduleTask> all = tasks;
        tasks = null;
        return all;
    }

    /// <summary>
    /// Inserts the task
    /// </summary>
    /// <param name="task">The task</param>
    public virtual async Task Insert(ScheduleTask task)
    {
        ArgumentNullException.ThrowIfNull(task);
        if (task.Enabled && !task.LastEnabledUtc.HasValue)
        {
            task.LastEnabledUtc = new DateTime?(DateTime.UtcNow);
        }
        await _taskRepository.Insert(task);
    }

    /// <summary>
    /// Updates the task
    /// </summary>
    /// <param name="task">The task</param>
    public virtual async Task Update(ScheduleTask task)
    {
        await _taskRepository.Update(task);
    }
}
