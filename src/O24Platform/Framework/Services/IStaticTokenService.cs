namespace O24OpenAPI.Framework.Services;

/// <summary>
/// The static token service interface
/// </summary>
public interface IStaticTokenService
{
    /// <summary>
    /// Creates the static token using the specified identifier
    /// </summary>
    /// <param name="identifier">The identifier</param>
    /// <returns>A task containing the string</returns>
    Task<string> CreateStaticToken(string identifier);

    /// <summary>
    /// Revokes the static token using the specified token
    /// </summary>
    /// <param name="token">The token</param>
    /// <param name="reason">The reason</param>
    /// <returns>A task containing the bool</returns>
    Task<bool> RevokeStaticToken(string token, string reason);

    /// <summary>
    /// Validates the static token using the specified token
    /// </summary>
    /// <param name="token">The token</param>
    /// <returns>The bool is valid string error message</returns>
    (bool IsValid, string ErrorMessage) ValidateStaticToken(string token);
}
