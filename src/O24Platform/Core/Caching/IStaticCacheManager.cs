using StackExchange.Redis;

namespace O24OpenAPI.Core.Caching;

/// <summary>
/// The static cache manager interface
/// </summary>
/// <seealso cref="IDisposable"/>
public interface IStaticCacheManager : IDisposable
{
    Task<T?> Get<T>(CacheKey key, Func<Task<T>> acquire);

    Task<T?> Get<T>(CacheKey key, Func<T> acquire);

    Task<T?> Get<T>(CacheKey key, T defaultValue);

    Task<object?> Get(CacheKey key);

    Task<T?> Get<T>(CacheKey key);

    Task Remove(CacheKey cacheKey);

    Task Set(CacheKey key, object data);

    Task RemoveByPrefix(string prefix);

    Task Clear();
    Task ClearByWord(string word);
    Task ClearByKey(CacheKey cacheKey);

    void SetFieldsHash(string key, HashEntry hashData, int cacheTime = 0);

    T? GetFieldHash<T>(string key, string field)
        where T : class;

    Task RemoveHash(string key);

    Task<T?> GetOrSetAsync<T>(CacheKey key, Func<Task<T?>> acquire);

    Task ClearAll();
    Task RemoveAsync(CacheKey cacheKey);
}
