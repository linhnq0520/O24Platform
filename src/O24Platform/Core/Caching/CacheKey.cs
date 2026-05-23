using O24OpenAPI.Core.Configuration;
using O24OpenAPI.Core.Infrastructure;

namespace O24OpenAPI.Core.Caching;

public class CacheKey(string key)
{
    public string Key { get; protected set; } = key;
    public int CacheTime { get; set; } =
        Singleton<AppSettings>.Instance?.Get<CacheConfig>()?.DefaultCacheTime ?? 60;

    public bool IsForever { get; set; } = false;
}
