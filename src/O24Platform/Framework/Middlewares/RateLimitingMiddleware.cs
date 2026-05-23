using Microsoft.AspNetCore.Http;
using O24OpenAPI.Core.Caching;
using O24OpenAPI.Core.Infrastructure;

namespace O24OpenAPI.Framework.Middlewares;

public class RateLimitingMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next ?? throw new ArgumentNullException(nameof(next));
    private readonly IStaticCacheManager _staticCacheManager =
        EngineContext.Current.Resolve<IStaticCacheManager>();
    private static readonly int CacheTimeMinutes = 1;
    private readonly TimeSpan _period = TimeSpan.FromMinutes(CacheTimeMinutes);

    private static readonly Dictionary<string, int> RateLimitRules = new()
    {
        { "/api/auth/loginopenapi", 5 },
        { "/api/health/ping", 20 },
        { "default", 500 },
    };

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.ToString().ToLower();

        if (!path.Contains("/api/"))
        {
            await _next(context);
            return;
        }

        var ipAddress = context.Connection.RemoteIpAddress?.MapToIPv4().ToString();
        var userId =
            context.User?.Identity?.IsAuthenticated == true
                ? context.User.Identity.Name
                : ipAddress;

        if (string.IsNullOrEmpty(userId))
        {
            await _next(context);
            return;
        }

        int limit = GetLimitForPath(path);
        var key = $"RateLimit:{path}:{userId}";
        var cacheKey = new CacheKey(key) { CacheTime = (int)_period.TotalMinutes };

        var entry = await _staticCacheManager.GetOrSetAsync(
            cacheKey,
            () =>
            {
                return Task.FromResult(
                    new RateLimitEntry { Count = 0, Expiry = DateTime.UtcNow.Add(_period) }
                );
            }
        );

        if (entry.Count >= limit)
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.Headers.RetryAfter = (
                (int)(entry.Expiry - DateTime.UtcNow).TotalSeconds
            ).ToString();
            await context.Response.WriteAsync("[CMS] Too many requests. Please try again later.");
            return;
        }

        lock (entry)
        {
            entry.Count++;
        }

        var remainingSeconds = (int)Math.Ceiling((entry.Expiry - DateTime.UtcNow).TotalSeconds);
        cacheKey.CacheTime = Math.Max(1, remainingSeconds / 60);
        await _staticCacheManager.Set(cacheKey, entry);

        await _next(context);
    }

    private static int GetLimitForPath(string path)
    {
        foreach (var rule in RateLimitRules)
        {
            if (rule.Key != "default" && path.StartsWith(rule.Key))
            {
                return rule.Value;
            }
        }
        return RateLimitRules["default"];
    }

    private class RateLimitEntry
    {
        public int Count { get; set; }
        public DateTime Expiry { get; set; }
    }
}
