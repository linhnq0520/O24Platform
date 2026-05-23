using O24OpenAPI.KeyVault.Model;
using O24OpenAPI.KeyVault.Option;
using O24OpenAPI.KeyVault.Provider;
using O24OpenAPI.KeyVault.Sdk;
using O24OpenAPI.KeyVault.Utility;

namespace O24OpenAPI.Client.Infisical;

/// <summary>
/// The infisical client class
/// </summary>
public class InfisicalClient
{
    /// <summary>
    /// The base url
    /// </summary>
    private readonly string _baseUrl;

    /// <summary>
    /// The api token
    /// </summary>
    private readonly string _apiToken;

    // /// <summary>
    // /// The api project id
    // /// /// </summary>
    // private readonly string _apiProjectId;

    /// <summary>
    /// The 24 key vault client
    /// </summary>
    private readonly O24KeyVaultClient _o24KeyVaultClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="InfisicalClient"/> class
    /// </summary>
    /// <exception cref="ArgumentException">O24OPENAPI_KEYVAULT_API_TOKEN is not set in environment variables.</exception>
    /// <exception cref="ArgumentException">O24OPENAPI_KEYVAULT_API_URL is not set in environment variables.</exception>
    /// <exception cref="ArgumentException">O24OPENAPI_KEYVAULT_CLIENT_ID is not set in environment variables.</exception>
    /// <exception cref="ArgumentException">O24OPENAPI_KEYVAULT_CLIENT_SECRET is not set in environment variables.</exception>
    public InfisicalClient()
    {
        _baseUrl =
            Environment.GetEnvironmentVariable("O24OPENAPI_KEYVAULT_API_URL")
            ?? throw new ArgumentException(
                "O24OPENAPI_KEYVAULT_API_URL is not set in environment variables."
            );
        _apiToken =
            Environment.GetEnvironmentVariable("O24OPENAPI_KEYVAULT_API_TOKEN")
            ?? throw new ArgumentException(
                "O24OPENAPI_KEYVAULT_API_TOKEN is not set in environment variables."
            );
        var clientId =
            Environment.GetEnvironmentVariable("O24OPENAPI_KEYVAULT_CLIENT_ID")
            ?? throw new ArgumentException(
                "O24OPENAPI_KEYVAULT_CLIENT_ID is not set in environment variables."
            );
        var clientSecret =
            Environment.GetEnvironmentVariable("O24OPENAPI_KEYVAULT_CLIENT_SECRET")
            ?? throw new ArgumentException(
                "O24OPENAPI_KEYVAULT_CLIENT_SECRET is not set in environment variables."
            );

        ClientSettings settings = new ClientSettings
        {
            Auth = new AuthenticationOptions
            {
                UniversalAuth = new UniversalAuthMethod
                {
                    ClientId = clientId,
                    ClientSecret = clientSecret,
                },
            },
            SiteUrl = _baseUrl,
        };

        _o24KeyVaultClient = new O24KeyVaultClient(settings);
    }

    /// <summary>
    /// Gets the secrets using the specified key
    /// </summary>
    /// <param name="key">The key</param>
    /// <exception cref="ArgumentException">O24OPENAPI_ENVIRONMENT is not set in environment variables.</exception>
    /// <exception cref="ArgumentException">O24OPENAPI_KEYVAULT_PROJECT_ID is not set in environment variables.</exception>
    /// <returns>The secret</returns>
    public GetSecretResponseSecret GetSecretsAsync(string key)
    {
        try
        {
            var projectId =
                Environment.GetEnvironmentVariable("O24OPENAPI_KEYVAULT_PROJECT_ID")
                ?? throw new ArgumentException(
                    "O24OPENAPI_KEYVAULT_PROJECT_ID is not set in environment variables."
                );
            // var env = "Linh";
            var env =
                Environment.GetEnvironmentVariable("O24OPENAPI_ENVIRONMENT")
                ?? throw new ArgumentException(
                    "O24OPENAPI_ENVIRONMENT is not set in environment variables."
                );
            var getSecretOptions = new GetSecretOptions
            {
                SecretName = key,
                ProjectId = projectId,
                Environment = env.ToLower(),
            };

            var secret = _o24KeyVaultClient.GetSecret(getSecretOptions);

            return secret;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return null;
        }
    }
}
