using System.Security.Claims;
using O24OpenAPI.Core.Domain.Users;
using O24OpenAPI.Framework.Models.JwtModels;

namespace O24OpenAPI.Framework.Services;

/// <summary>
/// The jwt token service interface
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Gets the new jwt token using the specified user
    /// </summary>
    /// <param name="user">The user</param>
    /// <param name="expireSeconds">The expire seconds</param>
    /// <returns>The string</returns>
    string GetNewJwtToken(User user, long expireSeconds = 0);

    /// <summary>
    /// Gets the new jwt token 1 using the specified user
    /// </summary>
    /// <param name="user">The user</param>
    /// <param name="expireSeconds">The expire seconds</param>
    /// <returns>The string</returns>
    string GetNewJwtToken1(User1 user, long expireSeconds = 0);

    /// <summary>
    /// Gets the value of the new secret key
    /// </summary>
    string NewSecretKey { get; }

    /// <summary>
    /// Gets the username from token using the specified token
    /// </summary>
    /// <param name="token">The token</param>
    /// <param name="ignoreExpiration">The ignore expiration</param>
    /// <returns>The string</returns>
    string GetUsernameFromToken(string token, bool ignoreExpiration = false);

    /// <summary>
    /// Gets the login name from token using the specified token
    /// </summary>
    /// <param name="token">The token</param>
    /// <param name="ignoreExpiration">The ignore expiration</param>
    /// <returns>The string</returns>
    string GetLoginNameFromToken(string token, bool ignoreExpiration = false);

    /// <summary>
    /// Gets the user code from token using the specified token
    /// </summary>
    /// <param name="token">The token</param>
    /// <param name="ignoreExpiration">The ignore expiration</param>
    /// <returns>The string</returns>
    string GetUserCodeFromToken(string token, bool ignoreExpiration = false);

    /// <summary>
    /// Gets the device id from token using the specified token
    /// </summary>
    /// <param name="token">The token</param>
    /// <param name="ignoreExpiration">The ignore expiration</param>
    /// <returns>The string</returns>
    string GetDeviceIdFromToken(string token, bool ignoreExpiration = false);

    /// <summary>
    /// Gets the branch code from token using the specified token
    /// </summary>
    /// <param name="token">The token</param>
    /// <param name="ignoreExpiration">The ignore expiration</param>
    /// <returns>The string</returns>
    string GetBranchCodeFromToken(string token, bool ignoreExpiration = false);

    /// <summary>
    /// Gets the generate time from token using the specified token
    /// </summary>
    /// <param name="token">The token</param>
    /// <param name="ignoreExpiration">The ignore expiration</param>
    /// <returns>The string</returns>
    string GetGenerateTimeFromToken(string token, bool ignoreExpiration = false);

    /// <summary>
    /// Gets the expire seconds from token using the specified token
    /// </summary>
    /// <param name="token">The token</param>
    /// <param name="ignoreExpiration">The ignore expiration</param>
    /// <returns>The string</returns>
    string GetExpireSecondsFromToken(string token, bool ignoreExpiration = false);

    /// <summary>
    /// Refreshes the token using the specified token
    /// </summary>
    /// <param name="token">The token</param>
    /// <returns>The string</returns>
    string RefreshToken(string token);

    ValidateTokenResponseModel ValidateToken(string token);
    string GenerateOpenAPIToken(string clientId, int tokenExpiresInSeconds);
    ClaimsPrincipal VerifyOpenApiToken(string token);
}
