using O24OpenAPI.Core.Domain;

namespace O24OpenAPI.Framework.Domain;

/// <summary>
/// /// The session manager class
/// </summary>
/// <seealso cref="BaseEntity"/>
public partial class SessionManager : BaseEntity
{
    /// <summary>
    /// Gets or sets the value of the token
    /// </summary>
    public string Token { get; set; }

    /// <summary>
    /// Gets or sets the value of the expire at
    /// </summary>
    public DateTimeOffset ExpireAt { get; set; }

    /// <summary>
    /// Gets or sets the value of the identifier
    /// </summary>
    public string Identifier { get; set; }

    /// <summary>
    /// Gets or sets the value of the is revoked
    /// </summary>
    public bool IsRevoked { get; set; } = false;

    /// <summary>
    /// Gets or sets the value of the revoke reason
    /// </summary>
    public string RevokeReason { get; set; }
}
