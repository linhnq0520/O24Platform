using System.Collections.Concurrent;
using Newtonsoft.Json;
using O24OpenAPI.Core.Configuration;
using StackExchange.Redis;

namespace O24OpenAPI.Core.Caching;

public class DistributedCacheManager : ILocker, IStaticCacheManager, IDisposable
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _distributedCache;
    private static readonly ConcurrentDictionary<string, byte> _keys;
    private readonly CacheConfig? _config;

    static DistributedCacheManager()
    {
        _keys = new ConcurrentDictionary<string, byte>();
    }

    public DistributedCacheManager(AppSettings appSettings, IConnectionMultiplexer redis)
    {
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
        _distributedCache = _redis.GetDatabase();
        _config = appSettings.Get<CacheConfig>();
    }

    public async Task Set(CacheKey key, object data)
    {
        ArgumentNullException.ThrowIfNull(key);

        if (key.CacheTime <= 0 || data == null)
        {
            return;
        }

        TimeSpan? expiration = key.IsForever ? null : TimeSpan.FromMinutes(key.CacheTime);

        await _distributedCache.StringSetAsync(
            key.Key,
            JsonConvert.SerializeObject(data),
            expiration
        );

        _keys.TryAdd(key.Key, 1);
    }

    public void SetFieldsHash(string key, HashEntry hashData, int cacheTime = 0)
    {
        _distributedCache.HashSet((RedisKey)key, hashData.Name, hashData.Value);
        _distributedCache.KeyExpire(
            key,
            TimeSpan.FromSeconds(cacheTime <= 0 ? _config?.DefaultCacheTime ?? 0 : cacheTime)
        );
        _keys.TryAdd(key, 1);
    }

    public T? GetFieldHash<T>(string key, string field)
        where T : class
    {
        RedisValue redisValue = _distributedCache.HashGet((RedisKey)key, field);
        if (redisValue.IsNull)
        {
            return null;
        }

        return JsonConvert.DeserializeObject<T>(redisValue.ToString());
    }

    public async Task RemoveHash(string key)
    {
        await _redis.GetDatabase().KeyDeleteAsync((RedisKey)key);
        _keys.TryRemove(key, out _);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public async Task<T?> Get<T>(CacheKey key, Func<Task<T>> acquire)
    {
        T result = await acquire();
        if (result != null)
        {
            await Set(key, result);
        }

        return result;
    }

    public async Task<T?> Get<T>(CacheKey key, Func<T> acquire)
    {
        T result = acquire();
        if (result != null)
        {
            await Set(key, result);
        }

        return result;
    }

    public async Task<T?> Get<T>(CacheKey key)
    {
        (bool isSet, T? item) = await TryGetItem<T>(key);
        if (isSet)
        {
            return item;
        }

        return default;
    }

    public async Task<T?> Get<T>(CacheKey key, T defaultValue)
    {
        RedisValue redisValue = await _distributedCache.StringGetAsync((RedisKey)key.Key);
        if (string.IsNullOrEmpty(redisValue))
        {
            return defaultValue;
        }

        return JsonConvert.DeserializeObject<T>(redisValue.ToString());
    }

    public async Task<object?> Get(CacheKey key)
    {
        return await Get(key, (object?)null);
    }

    public async Task Remove(CacheKey cacheKey)
    {
        if (!TryGetCacheKey(cacheKey, out string key))
        {
            return;
        }

        await _distributedCache.KeyDeleteAsync((RedisKey)key);
        _keys.TryRemove(key, out _);
    }

    public async Task RemoveByPrefix(string prefix)
    {
        List<string> keysToRemove = _keys
            .Keys.Where(k => k.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase))
            .ToList();

        foreach (var key in keysToRemove)
        {
            await _distributedCache.KeyDeleteAsync((RedisKey)key);
            _keys.TryRemove(key, out _);
        }
    }

    public Task Clear()
    {
        return ClearByKeys(_keys.Keys);
    }

    public Task ClearByWord(string word)
    {
        List<string> keysToRemove = _keys
            .Keys.Where(k => k.Contains(word, StringComparison.InvariantCultureIgnoreCase))
            .ToList();

        return ClearByKeys(keysToRemove);
    }

    public async Task ClearByKeys(IEnumerable<string> keys)
    {
        foreach (var key in keys)
        {
            await _distributedCache.KeyDeleteAsync((RedisKey)key);
            _keys.TryRemove(key, out _);
        }
    }

    public bool PerformActionWithLock(string resource, TimeSpan expirationTime, Action action)
    {
        if (!string.IsNullOrEmpty(_distributedCache.StringGet((RedisKey)resource)))
        {
            return false;
        }

        try
        {
            _distributedCache.StringSet((RedisKey)resource, resource);
            action();
            return true;
        }
        finally
        {
            _distributedCache.KeyDelete((RedisKey)resource);
        }
    }

    public async Task<T?> GetOrSetAsync<T>(CacheKey key, Func<Task<T?>> acquire)
    {
        (bool isSet, T? item) = await TryGetItem<T>(key);
        if (isSet)
        {
            return item;
        }

        T? result = await acquire();
        if (result != null)
        {
            await Set(key, result);
        }

        return result;
    }

    public async Task ClearAll()
    {
        IServer server = _redis.GetServer(_redis.GetEndPoints().First());
        RedisKey[] keys = server.Keys(pattern: "*").ToArray();

        // Delete in batches
        const int batchSize = 1000;
        for (int i = 0; i < keys.Length; i += batchSize)
        {
            IEnumerable<RedisKey> batch = keys.Skip(i).Take(batchSize);
            IEnumerable<Task<bool>> tasks = batch.Select(key =>
                _distributedCache.KeyDeleteAsync(key)
            );
            await Task.WhenAll(tasks);
        }

        _keys.Clear();
    }

    public async Task RemoveAsync(CacheKey cacheKey)
    {
        if (!TryGetCacheKey(cacheKey, out string key))
        {
            return;
        }

        await _distributedCache.KeyDeleteAsync((RedisKey)key);
        _keys.TryRemove(key, out _);
    }

    public async Task<(bool isSet, T? item)> TryGetItem<T>(CacheKey key)
    {
        RedisValue redisValue = await _distributedCache.StringGetAsync((RedisKey)key.Key);

        if (redisValue.IsNullOrEmpty)
        {
            return (false, default);
        }
        T? item = JsonConvert.DeserializeObject<T>(redisValue!);

        return (true, item);
    }

    public Task ClearByKey(CacheKey cacheKey)
    {
        if (!TryGetCacheKey(cacheKey, out string key))
        {
            return Task.CompletedTask;
        }

        _keys.TryRemove(key, out _);
        return _distributedCache.KeyDeleteAsync((RedisKey)key);
    }

    private static bool TryGetCacheKey(CacheKey? cacheKey, out string key)
    {
        key = cacheKey?.Key ?? string.Empty;
        return !string.IsNullOrWhiteSpace(key);
    }
}
