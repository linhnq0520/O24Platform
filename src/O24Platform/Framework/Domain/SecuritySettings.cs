using O24OpenAPI.Core.Configuration;

namespace O24OpenAPI.Framework.Domain;

/// <summary>
/// The security settings class
/// </summary>
/// <seealso cref="ISettings"/>
public class SecuritySettings : ISettings
{
    /// <summary>
    /// Gets or sets the value of the encryption key
    /// </summary>
    public string EncryptionKey { get; set; } = "273ece6f97dd844d";
}
