using System.Collections.Concurrent;
using System.Text;
using LinKit.Json.Runtime;
using O24OpenAPI.KeyVault.Utility;

namespace O24OpenAPI.Client.Infisical;

/// <summary>
/// The infisical manager class
/// </summary>
public class InfisicalManager
{
    /// <summary>
    /// The secrets
    /// </summary>
    private static readonly ConcurrentDictionary<string, object> _secrets = [];

    /// <summary>
    /// Gets the secret by key using the specified key
    /// </summary>
    /// <typeparam name="T">The </typeparam>
    /// <param name="key">The key</param>
    /// <returns>The</returns>
    public static T? GetSecretByKey<T>(string key)
    {
        var secret = _secrets.GetOrAdd(
            key,
            key =>
            {
                var client = new InfisicalClient();
                GetSecretResponseSecret secret =
                    client.GetSecretsAsync(key)
                    ?? throw new Exception($"Cannot find secret with key [{key}]");
                byte[] data = Convert.FromBase64String(secret.SecretValue);
                string decodedString = Encoding.UTF8.GetString(data);
                var result = decodedString.FromJson<T>();
                return result;
            }
        );
        return (T?)secret;
    }
}
