namespace O24OpenAPI.Core.Caching;

public interface IMemoryCacheService
{
    Task<T?> GetOrSetAsync<T>(CacheKey key, Func<Task<T>> acquire);
    Task<T?> GetOrSet<T>(string key, Func<Task<T>> acquire);
}
