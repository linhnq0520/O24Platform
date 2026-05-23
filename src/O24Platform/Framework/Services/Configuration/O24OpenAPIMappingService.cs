using O24OpenAPI.Core;
using O24OpenAPI.Core.Caching;
using O24OpenAPI.Core.SeedWork;
using O24OpenAPI.Data.System.Linq;
using O24OpenAPI.Framework.Domain;
using O24OpenAPI.Framework.Models;

namespace O24OpenAPI.Framework.Services.Configuration;

/// <summary>
/// Constructor
/// </summary>
/// <param name="mappingRepository"></param>
/// <param name="staticCacheManager"></param>
public class O24OpenAPIMappingService(
    IRepository<O24OpenAPIService> mappingRepository,
    IStaticCacheManager staticCacheManager
) : IO24OpenAPIMappingService
{
    private readonly IRepository<O24OpenAPIService> _mappingRepository = mappingRepository;
    private readonly IStaticCacheManager _staticCacheManager = staticCacheManager;

    /// <summary>
    /// Gets a Neptune mapping by identifier
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public virtual async Task<O24OpenAPIService> GetById(int id)
    {
        return await _mappingRepository.GetById(id, cache => null);
    }

    /// <summary>
    /// Get all Neptune mapping
    /// </summary>
    /// <returns></returns>
    public virtual async Task<IList<O24OpenAPIService>> GetAll()
    {
        return await _mappingRepository.GetAll(query => query);
    }

    /// <summary>
    /// Updates the o 24 open api service
    /// </summary>
    /// <param name="o24OpenAPIService">The 24 open api service</param>
    public async Task UpdateAsync(O24OpenAPIService o24OpenAPIService)
    {
        await _mappingRepository.Update(o24OpenAPIService);
    }

    /// <summary>
    /// Adds the o 24 open api service
    /// </summary>
    /// <param name="o24OpenAPIService">The 24 open api service</param>
    /// <returns>A task containing the 24 open api service</returns>
    public async Task<O24OpenAPIService> AddAsync(O24OpenAPIService o24OpenAPIService)
    {
        return await _mappingRepository.InsertAsync(o24OpenAPIService);
    }

    /// <summary>
    /// Deletes the o 24 open api service
    /// </summary>
    /// <param name="o24OpenAPIService">The 24 open api service</param>
    public async Task DeleteAsync(O24OpenAPIService o24OpenAPIService)
    {
        await _mappingRepository.Delete(o24OpenAPIService);
        CacheKey cacheKey = CachingKey.EntityKeyWithService<O24OpenAPIService>(
            o24OpenAPIService.StepCode
        );
        await _staticCacheManager.Remove(cacheKey);
    }

    public async Task<IPagedList<O24OpenAPIService>> SimpleSearch(SimpleSearchModel model)
    {
        IQueryable<O24OpenAPIService> query =
            from d in _mappingRepository.Table
            where
                (!string.IsNullOrEmpty(model.SearchText) && d.StepCode.Contains(model.SearchText))
                || true
            select d;
        return await query.ToPagedList(model.PageIndex, model.PageSize);
    }

    /// <summary>
    /// Get Neptune service mapping by step code
    /// </summary>
    /// <param name="stepCode"></param>
    /// <returns></returns>
    public virtual async Task<O24OpenAPIService> GetByStepCode(string stepCode)
    {
        CacheKey cacheKey = CachingKey.EntityKeyWithService<O24OpenAPIService>(stepCode);
        return await _staticCacheManager.Get(
            cacheKey,
            async delegate
            {
                IQueryable<O24OpenAPIService> query = _mappingRepository.Table.Where(n =>
                    n.StepCode == stepCode
                );
                return await query.FirstOrDefaultAsync();
            }
        );
    }

    /// <summary>
    /// Get neptune service mappings by array of step codes
    /// </summary>
    /// <param name="stepCodes"></param>
    /// <returns></returns>
    public virtual async Task<IList<O24OpenAPIService>> GetByStepCodes(string[] stepCodes)
    {
        if (stepCodes == null || stepCodes.Length == 0)
        {
            return [];
        }
        IOrderedQueryable<O24OpenAPIService> query =
            from n in _mappingRepository.Table
            where stepCodes.Contains(n.StepCode)
            orderby n.StepCode
            select n;
        return await query.ToListAsync();
    }
}
