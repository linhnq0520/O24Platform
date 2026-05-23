using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using StackExchange.Redis;

namespace O24OpenAPI.Core.Caching;

public class MemoryCacheManager(IMemoryCache memoryCache)
    : ILocker,
        IStaticCacheManager,
        IMemoryCacheService,
        IDisposable
{
    private bool _disposed;
    private readonly IMemoryCache _memoryCache = memoryCache;
    private static readonly CancellationTokenSource _clearToken = new();
    private static readonly ConcurrentDictionary<string, byte> _keys = new();

    private static MemoryCacheEntryOptions PrepareEntryOptions(CacheKey key)
    {
        MemoryCacheEntryOptions options = new()
        {
            AbsoluteExpirationRelativeToNow = new TimeSpan?(TimeSpan.FromMinutes(key.CacheTime)),
        };
        options.AddExpirationToken(new CancellationChangeToken(_clearToken.Token));
        return options;
    }

    public Task Remove(CacheKey cacheKey)
    {
        if (!TryGetCacheKey(cacheKey, out string key))
        {
            return Task.CompletedTask;
        }

        _memoryCache.Remove(key);
        _keys.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(CacheKey cacheKey)
    {
        if (!TryGetCacheKey(cacheKey, out string key))
        {
            return Task.CompletedTask;
        }

        _memoryCache.Remove(key);
        _keys.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    public Task Set(CacheKey key, object data)
    {
        ArgumentNullException.ThrowIfNull(key);
        if (key.CacheTime <= 0 || data == null)
        {
            return Task.CompletedTask;
        }
        _memoryCache.Set(key.Key, data, PrepareEntryOptions(key));
        _keys.TryAdd(key.Key, 1);
        return Task.CompletedTask;
    }

    public async Task<T?> Get<T>(CacheKey key, Func<Task<T>> acquire)
    {
        ArgumentNullException.ThrowIfNull(key);
        CacheKey cacheKey = key;
        if (cacheKey.CacheTime <= 0)
        {
            T obj = await acquire();
            return obj;
        }
        if (_memoryCache.TryGetValue<T>(key.Key, out T? result))
        {
            return result;
        }

        T obj1 = await acquire();
        result = obj1;
        if (result != null)
        {
            await Set(key, result);
        }

        return result;
    }

    public async Task<T?> Get<T>(CacheKey key, Func<T> acquire)
    {
        CacheKey cacheKey = key;
        if ((cacheKey != null ? cacheKey.CacheTime : 0) <= 0)
        {
            return acquire();
        }

        T? result = _memoryCache.GetOrCreate<T>(
            key.Key,
            entry =>
            {
                entry.SetOptions(PrepareEntryOptions(key));
                return acquire();
            }
        );
        if (result == null)
        {
            await Remove(key);
        }
        return result;
    }

    public async Task<T?> Get<T>(CacheKey key, T defaultValue)
    {
        Task<T>? value = _memoryCache.Get<Lazy<Task<T>>>(key.Key)?.Value;
        try
        {
            T obj1;
            if (value != null)
            {
                T? obj2 = await value;
                obj1 = obj2;
                obj2 = default;
            }
            else
            {
                obj1 = defaultValue;
            }

            return obj1;
        }
        catch (Exception)
        {
            await Remove(key);
            throw;
        }
    }

    public async Task<T?> Get<T>(CacheKey key)
    {
        await Task.CompletedTask;
        (bool isSet, T? item) = TryGetItem<T>(key);
        if (isSet)
        {
            return item;
        }

        return default;
    }

    public async Task<object?> Get(CacheKey key)
    {
        object? entry = _memoryCache.Get(key.Key);
        if (entry == null)
        {
            return null;
        }

        try
        {
            if (!(entry.GetType().GetProperty("Value")?.GetValue(entry) is Task task))
            {
                return null;
            }

            await task;
            return task.GetType().GetProperty("Result")?.GetValue(task);
        }
        catch (Exception)
        {
            await Remove(key);
            throw;
        }
    }

    public bool PerformActionWithLock(string key, TimeSpan expirationTime, Action action)
    {
        if (_memoryCache.TryGetValue(key, out object? _))
        {
            return false;
        }

        try
        {
            _memoryCache.Set<string>(key, key, expirationTime);
            action();
            return true;
        }
        finally
        {
            _memoryCache.Remove(key);
        }
    }

    public Task RemoveByPrefix(string prefix)
    {
        List<string> keysToRemove = _keys
            .Keys.Where(k => k.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase))
            .ToList();

        foreach (var key in keysToRemove)
        {
            _memoryCache.Remove(key);
            _keys.TryRemove(key, out _);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Clears this instance
    /// </summary>
    public Task Clear()
    {
        ClearByKeys(_keys.Keys);
        return Task.CompletedTask;
    }

    public void ClearByKeys(IEnumerable<string> keys)
    {
        foreach (var key in keys)
        {
            _memoryCache.Remove(key);
            _keys.TryRemove(key, out _);
        }
    }

    public Task ClearByWord(string word)
    {
        List<string> keysToRemove = _keys
            .Keys.Where(k => k.Contains(word, StringComparison.InvariantCultureIgnoreCase))
            .ToList();

        foreach (var key in keysToRemove)
        {
            _memoryCache.Remove(key);
            _keys.TryRemove(key, out _);
        }

        return Task.CompletedTask;
    }

    public void SetFieldsHash(string key, HashEntry hashData, int cacheTime = 0)
    {
        throw new NotImplementedException();
    }

    public T GetFieldHash<T>(string key, string field)
        where T : class
    {
        throw new NotImplementedException();
    }

    public Task RemoveHash(string key)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _memoryCache.Dispose();
        }

        _disposed = true;
    }

    private (bool isSet, T? item) TryGetItem<T>(CacheKey key)
    {
        object? entry = _memoryCache.Get(key.Key);
        if (entry == null)
        {
            return (false, default(T));
        }

        T item = (T)entry;
        return (true, item);
    }

    public async Task<T?> GetOrSetAsync<T>(CacheKey key, Func<Task<T?>> acquire)
    {
        (bool isSet, T? item) = TryGetItem<T>(key);
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

    public Task ClearAll()
    {
        return Clear();
    }

    public async Task<T?> GetOrSet<T>(string key, Func<Task<T>> acquire)
    {
        object? entry = _memoryCache.Get(key);
        if (entry != null)
        {
            return (T?)entry;
        }

        T? result = await acquire();
        if (result != null)
        {
            _memoryCache.Set(key, result, TimeSpan.FromMinutes(60));
        }

        return result;
    }

    public Task ClearByKey(CacheKey cacheKey)
    {
        if (!TryGetCacheKey(cacheKey, out string key))
        {
            return Task.CompletedTask;
        }

        _memoryCache.Remove(key);
        _keys.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    private static bool TryGetCacheKey(CacheKey? cacheKey, out string key)
    {
        key = cacheKey?.Key ?? string.Empty;
        return !string.IsNullOrWhiteSpace(key);
    }
}
