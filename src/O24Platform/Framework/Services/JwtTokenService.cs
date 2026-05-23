using Microsoft.IdentityModel.Tokens;
using O24OpenAPI.Core.Domain.Users;
using O24OpenAPI.Core.Infrastructure;
using O24OpenAPI.Framework.Models.JwtModels;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace O24OpenAPI.Framework.Services;

/// <summary>
/// The jwt token service class
/// </summary>
/// <seealso cref="IJwtTokenService"/>
public class JwtTokenService(WebApiSettings webApiSettings) : IJwtTokenService
{
    /// <summary>
    /// The web api settings
    /// </summary>
    private readonly WebApiSettings _webApiSettings = webApiSettings;

    /// <summary>
    /// Gets the new jwt token using the specified user
    /// </summary>
    /// <param name="user">The user</param>
    /// <param name="expireSeconds">The expire seconds</param>
    /// <returns>The string</returns>
    private const string DeviceIdClaimUri =
        "http://schemas.microsoft.com/2012/01/devicecontext/claims/identifier";



    private static void AddClaimIfNotNull(List<Claim> claims, string type, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            claims.Add(new Claim(type, value));
        }
    }

    public virtual string GetNewJwtToken(User user, long expireSeconds = 0)
    {
        ArgumentNullException.ThrowIfNull(user);

        DateTimeOffset now = DateTimeOffset.UtcNow;

        long exp = expireSeconds > 0
            ? expireSeconds
            : now.AddDays(_webApiSettings.TokenLifetimeDays).ToUnixTimeSeconds();

        var claims = new List<Claim>();

        claims.Add(new Claim("nbf", now.ToUnixTimeSeconds().ToString()));
        claims.Add(new Claim("exp", exp.ToString()));
        claims.Add(new Claim(WebApiCommonDefaults.ClaimTypeName, user.Id.ToString()));

        // Optional claims (null-safe)
        AddClaimIfNotNull(claims, WebApiCommonDefaults.loginname, user.LoginName);
        AddClaimIfNotNull(claims, WebApiCommonDefaults.username, user.Username);
        AddClaimIfNotNull(claims, WebApiCommonDefaults.usercode, user.UserCode);
        AddClaimIfNotNull(claims, WebApiCommonDefaults.branchcode, user.BranchCode);
        AddClaimIfNotNull(claims, WebApiCommonDefaults.deviceid, user.DeviceId);

        var token = new JwtSecurityToken(
            new JwtHeader(
                new SigningCredentials(
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(_webApiSettings.SecretKey)
                    ),
                    WebApiCommonDefaults.JwtSignatureAlgorithm
                )
            ),
            new JwtPayload(claims)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Gets the new jwt token 1 using the specified user
    /// </summary>
    /// <param name="user">The user</param>
    /// <param name="expireSeconds">The expire seconds</param>
    /// <returns>The string</returns>
    public virtual string GetNewJwtToken1(User1 user, long expireSeconds = 0)
    {
        DateTimeOffset now = DateTimeOffset.Now;
        long num = now.AddDays(_webApiSettings.TokenLifetimeDays).ToUnixTimeSeconds();
        if (expireSeconds > 0L)
        {
            num = expireSeconds;
        }

        List<Claim> claimList =
        [
            new Claim("nbf", now.ToUnixTimeSeconds().ToString()),
            new Claim("exp", num.ToString()),
            new Claim(WebApiCommonDefaults.ClaimTypeName, user.UserId),
            new Claim(WebApiCommonDefaults.loginname, user.LoginName),
            new Claim(WebApiCommonDefaults.username, user.UserName),
            new Claim(WebApiCommonDefaults.usercode, user.UserCode),
            new Claim(WebApiCommonDefaults.branchcode, user.BranchCode),
            new Claim(WebApiCommonDefaults.deviceid, user.DeviceId),
        ];
        return new JwtSecurityTokenHandler().WriteToken(
            new JwtSecurityToken(
                new JwtHeader(
                    new SigningCredentials(
                        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_webApiSettings.SecretKey)),
                        WebApiCommonDefaults.JwtSignatureAlgorithm
                    )
                ),
                new JwtPayload(claimList)
            )
        );
    }

    /// <summary>
    /// Gets the value of the new secret key
    /// </summary>
    public virtual string NewSecretKey
    {
        get
        {
            using RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create();
            byte[] numArray = new byte[WebApiCommonDefaults.MinSecretKeyLength];
            randomNumberGenerator.GetBytes(numArray);
            return Convert.ToBase64String(numArray).TrimEnd('=');
        }
    }

    /// <summary>
    /// Gets the username from token using the specified token
    /// </summary>
    /// <param name="token">The token</param>
    /// <param name="ignoreExpiration">The ignore expiration</param>
    /// <returns>The string</returns>
    public virtual string GetUsernameFromToken(string token, bool ignoreExpiration = false)
    {
        return GetInfoFromToken(token, WebApiCommonDefaults.username, ignoreExpiration);
    }

    /// <summary>
    /// Gets the login name from token using the specified token
    /// </summary>
    /// <param name="token">The token</param>
    /// <param name="ignoreExpiration">The ignore expiration</param>
    /// <returns>The string</returns>
    public virtual string GetLoginNameFromToken(string token, bool ignoreExpiration = false)
    {
        return GetInfoFromToken(token, WebApiCommonDefaults.loginname, ignoreExpiration);
    }

    /// <summary>
    /// Gets the user code from token using the specified token
    /// </summary>
    /// <param name="token">The token</param>
    /// <param name="ignoreExpiration">The ignore expiration</param>
    /// <returns>The string</returns>
    public virtual string GetUserCodeFromToken(string token, bool ignoreExpiration = false)
    {
        return GetInfoFromToken(token, WebApiCommonDefaults.usercode, ignoreExpiration);
    }

    /// <summary>
    /// Gets the branch code from token using the specified token
    /// </summary>
    /// <param name="token">The token</param>
    /// <param name="ignoreExpiration">The ignore expiration</param>
    /// <returns>The string</returns>
    public virtual string GetBranchCodeFromToken(string token, bool ignoreExpiration = false)
    {
        return GetInfoFromToken(token, WebApiCommonDefaults.branchcode, ignoreExpiration);
    }

    /// <summary>
    /// Gets the device id from token using the specified token
    /// </summary>
    /// <param name="token">The token</param>
    /// <param name="ignoreExpiration">The ignore expiration</param>
    /// <returns>The string</returns>
    public virtual string GetDeviceIdFromToken(string token, bool ignoreExpiration = false)
    {
        return GetInfoFromToken(token, WebApiCommonDefaults.deviceid, ignoreExpiration);
    }

    /// <summary>
    /// Gets the generate time from token using the specified token
    /// </summary>
    /// <param name="token">The token</param>
    /// <param name="ignoreExpiration">The ignore expiration</param>
    /// <returns>The string</returns>
    public virtual string GetGenerateTimeFromToken(string token, bool ignoreExpiration = false)
    {
        return GetInfoFromToken(token, "nbf", ignoreExpiration);
    }

    /// <summary>
    /// Gets the expire seconds from token using the specified token
    /// </summary>
    /// <param name="token">The token</param>
    /// <param name="ignoreExpiration">The ignore expiration</param>
    /// <returns>The string</returns>
    public virtual string GetExpireSecondsFromToken(string token, bool ignoreExpiration = false)
    {
        return GetInfoFromToken(token, "exp", ignoreExpiration);
    }

    /// <summary>
    /// Refreshes the token using the specified token
    /// </summary>
    /// <param name="token">The token</param>
    /// <returns>The string</returns>
    public virtual string RefreshToken(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return "";
        }

        try
        {
            var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
            var userId = jwtToken
                .Claims.First(x => x.Type == WebApiCommonDefaults.ClaimTypeName)
                .Value;
            var loginName = jwtToken
                .Claims.First(x => x.Type == WebApiCommonDefaults.loginname)
                .Value;
            var username = jwtToken
                .Claims.First(x => x.Type == WebApiCommonDefaults.username)
                .Value;
            var userCode = jwtToken
                .Claims.First(x => x.Type == WebApiCommonDefaults.usercode)
                .Value;
            var branchCode = jwtToken
                .Claims.First(x => x.Type == WebApiCommonDefaults.branchcode)
                .Value;
            var deviceId = jwtToken
                .Claims.First(x => x.Type == WebApiCommonDefaults.deviceid)
                .Value;
            var user = new User
            {
                Id = int.Parse(userId),
                LoginName = loginName,
                Username = username,
                UserCode = userCode,
                BranchCode = branchCode,
                DeviceId = deviceId,
            };
            return GetNewJwtToken(user);
        }
        catch
        {
            return "";
        }
    }

    /// <summary>
    /// Gets the info from token using the specified token
    /// </summary>
    /// <param name="token">The token</param>
    /// <param name="claimType">The claim type</param>
    /// <param name="ignoreExpiration">The ignore expiration</param>
    /// <returns>The string</returns>
    private static string GetInfoFromToken(
        string token,
        string claimType,
        bool ignoreExpiration = false
    )
    {
        if (string.IsNullOrEmpty(token))
        {
            return "";
        }

        try
        {
            Claim claim = null;
            if (ignoreExpiration)
            {
                var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
                claim = jwtToken.Claims.FirstOrDefault(x =>
                    x.Type == claimType || x.Type == DeviceIdClaimUri
                );
                return claim?.Value ?? "";
            }

            // Validate token khi ignoreExpiration = false
            JwtSecurityTokenHandler securityTokenHandler = new();
            byte[] bytes = Encoding.UTF8.GetBytes(
                EngineContext.Current.Resolve<WebApiSettings>().SecretKey
            );

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(bytes),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero,
                ValidateLifetime = true,
            };

            var principal = securityTokenHandler.ValidateToken(
                token,
                validationParameters,
                out SecurityToken securityToken
            );

            claim = principal.Claims.FirstOrDefault(x =>
                x.Type == claimType || x.Type == DeviceIdClaimUri
            );

            return claim?.Value ?? "";
        }
        catch
        {
            return "";
        }
    }

    /// <summary>
    /// Generates the refresh token
    /// </summary>
    /// <returns>The refresh token</returns>
    public static string GenerateRefreshToken()
    {
        var refreshToken = Guid.NewGuid().ToString("N");
        return refreshToken;
    }

    public ValidateTokenResponseModel ValidateToken(string token)
    {
        var jwtTokenHandler = new JwtSecurityTokenHandler();
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_webApiSettings.SecretKey)
            ),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero,
            ValidateLifetime = true,
        };

        try
        {
            jwtTokenHandler.ValidateToken(token, tokenValidationParameters, out var validatedToken);
            if (validatedToken is not JwtSecurityToken jwtToken)
            {
                throw new SecurityTokenException("Invalid token");
            }

            var userId = jwtToken.Claims.First(x => x.Type == "UserId").Value;
            return new ValidateTokenResponseModel { IsValid = true, UserId = userId };
        }
        catch (Exception ex)
        {
            return new ValidateTokenResponseModel { IsValid = false, Message = ex.Message };
        }
    }

    public string GenerateOpenAPIToken(string clientId, int tokenExpiresInSeconds)
    {
        var claims = new List<Claim>
        {
            new("client_id", clientId),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_webApiSettings.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _webApiSettings.Issuer,
            audience: _webApiSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddSeconds(tokenExpiresInSeconds),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public ClaimsPrincipal VerifyOpenApiToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_webApiSettings.SecretKey);

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = _webApiSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = _webApiSettings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero, // Không cho phép sai lệch thời gian
        };

        try
        {
            var principal = tokenHandler.ValidateToken(
                token,
                validationParameters,
                out SecurityToken validatedToken
            );
            return principal;
        }
        catch (SecurityTokenException)
        {
            return null; // Token invalid
        }
    }

    public static string GetClaimValue(ClaimsPrincipal principal, string claimType)
    {
        return principal?.FindFirst(claimType)?.Value;
    }
}
