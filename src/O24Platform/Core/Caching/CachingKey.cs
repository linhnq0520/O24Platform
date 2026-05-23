using O24OpenAPI.Core.Configuration;
using O24OpenAPI.Core.Infrastructure;

namespace O24OpenAPI.Core.Caching;

public class CachingKey
{
    private const string GlobalPrefix = "o24";
    public const string SessionPrefix = $"o24:session";
    public const string ServicePrefix = $"o24:service";
    public const string EntityPrefix = $"o24:entity";
    public const string SettingPrefix = "o24:setting";
    public const string CDCPrefix = "o24:cdc";

    public static CacheKey SessionKey(string token) => new($"{SessionPrefix}:{token}");

    public static CacheKey SessionKeyNoHash(string token) => new($"{SessionPrefix}:{token}");

    public static CacheKey EntityKey<T>(params string?[] values) =>
        new(
            $"{EntityPrefix}:{NormalizeKeyPart(typeof(T).Name)}:{JoinKeyParts(values)}"
        );

    public static CacheKey EntityKeyWithService<T>(List<string?>? values) =>
        new(
            $"{EntityPrefix}:{GetServiceKeyPart()}:{NormalizeKeyPart(typeof(T).Name)}:{JoinKeyParts(values)}"
        );

    public static CacheKey EntityKeyWithService<T>(params string?[] values) =>
        new(
            $"{EntityPrefix}:{GetServiceKeyPart()}:{NormalizeKeyPart(typeof(T).Name)}:{JoinKeyParts(values)}"
        );

    public static CacheKey CreateKey(string prefix, params string?[] values) =>
        new($"{GlobalPrefix}:{NormalizeKeyPart(prefix)}:{JoinKeyParts(values)}");

    private static string GetServiceKeyPart()
    {
        return NormalizeKeyPart(Singleton<O24OpenAPIConfiguration>.Instance?.YourServiceID);
    }

    private static string JoinKeyParts(IEnumerable<string?>? values)
    {
        if (values == null)
        {
            return string.Empty;
        }

        return string.Join(":", values.Select(NormalizeKeyPart));
    }

    private static string NormalizeKeyPart(string? value)
    {
        return value?.Trim().ToLowerInvariant() ?? string.Empty;
    }
}
