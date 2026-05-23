namespace O24OpenAPI.Framework;

/// <summary>
/// The web api common defaults class
/// </summary>
public class WebApiCommonDefaults
{
    /// <summary>
    /// The loginname
    /// </summary>
    public static string loginname = nameof(loginname);

    /// <summary>
    /// /// The username
    /// </summary>
    public static string username = nameof(username);

    /// <summary>
    /// The usercode
    /// </summary>
    public static string usercode = nameof(usercode);

    /// <summary>
    /// The branchcode
    /// </summary>
    public static string branchcode = nameof(branchcode);

    /// <summary>
    /// The deviceid
    /// </summary>
    public static string deviceid = nameof(deviceid);

    /// <summary>Gets user key of http context</summary>
    public static string UserKey => "O24OpenApiUser";

    /// <summary>Gets Claim type</summary>
    public static string ClaimTypeName => "UserId";

    /// <summary>Gets the name of the header to be used for security</summary>
    public static string SecurityHeaderName => "Authorization";

    /// <summary>Token lifetime in days</summary>
    public static int TokenLifeTime => 7;

    /// <summary>The JWT token signature algorithm</summary>
    public static string JwtSignatureAlgorithm => "HS256";

    /// <summary>
    /// The minimal length of secret key applied to signature algorithm
    /// <remarks>
    /// For HmacSha256 it may be at least 16 (128 bites)
    /// </remarks>
    /// </summary>
    public static int MinSecretKeyLength => 16;
}
