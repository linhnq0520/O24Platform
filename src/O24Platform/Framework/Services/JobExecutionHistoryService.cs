using O24OpenAPI.Core;
using O24OpenAPI.Core.Caching;
using O24OpenAPI.Core.SeedWork;
using O24OpenAPI.Data.System.Linq;
using O24OpenAPI.Framework.Domain;
using O24OpenAPI.Framework.Models;

namespace O24OpenAPI.Framework.Services;

/// <summary>
/// Constructor
/// </summary>
/// <param name="jobExecutionHistoryRepository"></param>
/// <param name="staticCacheManager"></param>
public class JobExecutionHistoryService(
    IRepository<JobExecutionLog> jobExecutionHistoryRepository,
    IStaticCacheManager staticCacheManager
) : IJobExecutionHistoryService
{
    private readonly IRepository<JobExecutionLog> _jobExecutionHistoryRepository =
        jobExecutionHistoryRepository;
    private readonly IStaticCacheManager _staticCacheManager = staticCacheManager;

    /// <summary>
    /// Gets a job execution history by identifier
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public virtual async Task<JobExecutionLog> GetById(int id)
    {
        return await _jobExecutionHistoryRepository.GetById(id, cache => null);
    }

    /// <summary>
    /// Get all job execution history
    /// </summary>
    /// <returns></returns>
    public virtual async Task<IList<JobExecutionLog>> GetAll()
    {
        return await _jobExecutionHistoryRepository.GetAll(query => query);
    }

    /// <summary>
    /// Updates the job execution history service
    /// </summary>
    /// <param name="JobExecutionHistory">The 24 open api service</param>
    public async Task UpdateAsync(JobExecutionLog JobExecutionHistory)
    {
        await _jobExecutionHistoryRepository.Update(JobExecutionHistory);
    }

    /// <summary>
    /// Adds the job execution history service
    /// </summary>
    /// <param name="JobExecutionHistory">The 24 open api service</param>
    /// <returns>A task containing the 24 open api service</returns>
    public async Task<JobExecutionLog> AddAsync(JobExecutionLog JobExecutionHistory)
    {
        return await _jobExecutionHistoryRepository.InsertAsync(JobExecutionHistory);
    }

    /// <summary>
    /// Deletes the job execution history service
    /// </summary>
    /// <param name="JobExecutionHistory">The 24 open api service</param>
    public async Task DeleteAsync(JobExecutionLog JobExecutionHistory)
    {
        await _jobExecutionHistoryRepository.Delete(JobExecutionHistory);
        CacheKey cacheKey = CachingKey.EntityKeyWithService<JobExecutionLog>(
            JobExecutionHistory.JobName
        );
        await _staticCacheManager.Remove(cacheKey);
    }

    public async Task<IPagedList<JobExecutionLog>> SimpleSearch(SimpleSearchModel model)
    {
        IQueryable<JobExecutionLog> query =
            from d in _jobExecutionHistoryRepository.Table
            where
                (!string.IsNullOrEmpty(model.SearchText) && d.JobName.Contains(model.SearchText))
                || true
            select d;
        return await query.ToPagedList(model.PageIndex, model.PageSize);
    }

    /// <summary>
    /// Get job execution history by job name
    /// </summary>
    /// <param name="stepCode"></param>
    /// <returns></returns>
    public virtual async Task<JobExecutionLog> GetByJobName(string jobName)
    {
        CacheKey cacheKey = CachingKey.EntityKeyWithService<JobExecutionLog>(jobName);
        return await _staticCacheManager.Get(
            cacheKey,
            async delegate
            {
                IQueryable<JobExecutionLog> query = _jobExecutionHistoryRepository.Table.Where(n =>
                    n.JobName == jobName
                );
                return await query.FirstOrDefaultAsync();
            }
        );
    }
}
