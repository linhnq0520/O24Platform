using LinqToDB.Async;
using Microsoft.IdentityModel.Tokens;
using O24OpenAPI.Core.SeedWork;
using O24OpenAPI.Framework.Domain;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace O24OpenAPI.Framework.Services;

/// <summary>
/// The static token service class
/// </summary>
/// <seealso cref="IStaticTokenService"/>
public class StaticTokenService(
    WebApiSettings setting,
    IRepository<SessionManager> sessionManagerRepository
) : IStaticTokenService
{
    /// <summary>
    /// The setting
    /// </summary>
    private readonly WebApiSettings _setting = setting;

    /// <summary>
    /// The private key
    /// </summary>
    private readonly string _privateKey = setting.PrivateKey;

    /// <summary>
    /// The session manager repository
    /// </summary>
    private readonly IRepository<SessionManager> _sessionManagerRepository =
        sessionManagerRepository;

    /// <summary>
    /// Creates the static token using the specified identifier
    /// </summary>
    /// <param name="identifier">The identifier</param>
    /// <exception cref="ArgumentException">Identifier cannot be null or empty </exception>
    /// <returns>The token</returns>
    public async Task<string> CreateStaticToken(string identifier)
    {
        if (string.IsNullOrEmpty(identifier))
        {
            throw new ArgumentException("Identifier cannot be null or empty", nameof(identifier));
        }

        (string token, long exp) = GenerateJwtToken(identifier);

        var entity = new SessionManager
        {
            Token = token,
            ExpireAt = DateTimeOffset.FromUnixTimeSeconds(exp),
            Identifier = identifier,
        };
        await _sessionManagerRepository.Insert(entity);

        return token;
    }

    /// <summary>
    /// Revokes the static token using the specified token
    /// </summary>
    /// <param name="token">The token</param>
    /// <param name="reason">The reason</param>
    /// <returns>A task containing the bool</returns>
    public async Task<bool> RevokeStaticToken(string token, string reason)
    {
        if (string.IsNullOrEmpty(token))
        {
            return false;
        }

        SessionManager tokenInfo = await _sessionManagerRepository
            .Table.Where(s => s.Token == token)
            .FirstOrDefaultAsync();

        if (tokenInfo == null)
        {
            return false;
        }

        tokenInfo.IsRevoked = true;
        tokenInfo.RevokeReason = reason;
        await _sessionManagerRepository.Update(tokenInfo);

        return true;
    }

    /// <summary>
    /// Generates the hmac token using the specified payload
    /// </summary>
    /// <param name="payload">The payload</param>
    /// <returns>The string</returns>
    private string GenerateHmacToken(string payload)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_privateKey));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return Convert.ToBase64String(hash).TrimEnd('=');
    }

    /// <summary>
    /// Generates the jwt token using the specified identifier
    /// </summary>
    /// <param name="identifier">The identifier</param>
    /// <param name="expireSeconds">The expire seconds</param>
    /// <returns>The string long</returns>
    private (string, long) GenerateJwtToken(string identifier, long expireSeconds = 0L)
    {
        DateTimeOffset now = DateTimeOffset.Now;
        long num = now.AddDays(_setting.StaticTokenLifetimeDays).ToUnixTimeSeconds();
        if (expireSeconds > 0L)
        {
            num = expireSeconds;
        }

        Claim[] claims = new[]
        {
            new Claim("nbf", now.ToUnixTimeSeconds().ToString()),
            new Claim("exp", num.ToString()),
            new Claim("identifier", identifier),
        };

        var token = new JwtSecurityToken(
            claims: claims,
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_privateKey)),
                SecurityAlgorithms.HmacSha256Signature
            )
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), num);
    }

    /// <summary>
    /// Gets the info from token using the specified token
    /// </summary>
    /// <param name="token">The token</param>
    /// <param name="claimType">The claim type</param>
    /// <param name="ignoreExpiration">The ignore expiration</param>
    /// <returns>The string</returns>
    private string GetInfoFromToken(string token, string claimType, bool ignoreExpiration = false)
    {
        if (string.IsNullOrEmpty(token))
        {
            return "";
        }

        try
        {
            if (ignoreExpiration)
            {
                JwtSecurityToken jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
                return jwtToken.Claims.FirstOrDefault(x => x.Type == claimType)?.Value ?? "";
            }

            JwtSecurityTokenHandler securityTokenHandler = new();
            byte[] bytes = Encoding.UTF8.GetBytes(_setting.PrivateKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(bytes),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero,
                ValidateLifetime = true,
            };

            SecurityToken securityToken;
            ClaimsPrincipal principal = securityTokenHandler.ValidateToken(
                token,
                validationParameters,
                out securityToken
            );
            return principal.Claims.First(x => x.Type == claimType).Value;
        }
        catch
        {
            return "";
        }
    }

    /// <summary>
    /// Validates the static token using the specified token
    /// </summary>
    /// <param name="token">The token</param>
    /// <returns>The bool is valid string error message</returns>
    public (bool IsValid, string ErrorMessage) ValidateStaticToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return (false, "Token is null or empty");
        }

        token = token.Trim();

        if (!token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return (false, $"Invalid Authorization header format. Expected 'Bearer {token}'");
        }

        token = token["Bearer ".Length..].Trim();

        var parts = token.Split('.');
        if (parts.Length != 3)
        {
            return (
                false,
                $"JWT is not well-formed. Expected 3 parts, found {parts.Length}. Token: '{token}'"
            );
        }

        foreach (var part in parts)
        {
            if (string.IsNullOrEmpty(part) || !IsBase64UrlString(part))
            {
                return (false, $"Invalid JWT part. Token: '{token}'");
            }
        }

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_privateKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
            };

            ClaimsPrincipal principal = tokenHandler.ValidateToken(
                token,
                validationParameters,
                out SecurityToken validatedToken
            );

            var identifier = principal.FindFirst("identifier")?.Value;
            if (string.IsNullOrEmpty(identifier))
            {
                return (false, "Identifier claim is missing or empty");
            }
            if (!identifier.Equals(_setting.SecretKey))
            {
                return (false, "Identifier does not match the expected secret key");
            }
            return (true, null);
        }
        catch (SecurityTokenExpiredException)
        {
            return (false, "Token has expired");
        }
        catch (SecurityTokenInvalidSignatureException)
        {
            return (false, "Invalid token signature");
        }
        catch (SecurityTokenException ex)
        {
            return (false, $"Token validation failed: {ex.Message}. Token: '{token}'");
        }
        catch (Exception ex)
        {
            return (false, $"Unexpected error: {ex.Message}. Token: '{token}'");
        }
    }

    /// <summary>
    /// Ises the base 64 url string using the specified input
    /// </summary>
    /// <param name="input">The input</param>
    /// <returns>The bool</returns>
    private bool IsBase64UrlString(string input)
    {
        try
        {
            string base64 = input.Replace('-', '+').Replace('_', '/');
            switch (base64.Length % 4)
            {
                case 2:
                    base64 += "==";
                    break;
                case 3:
                    base64 += "=";
                    break;
            }
            Convert.FromBase64String(base64);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
