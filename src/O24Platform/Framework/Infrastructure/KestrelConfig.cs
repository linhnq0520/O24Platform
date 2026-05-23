using O24OpenAPI.Core.Configuration;

namespace O24OpenAPI.Framework.Infrastructure;

/// <summary>
/// The kestrel class
/// </summary>
/// <seealso cref="IConfig"/>
public class Kestrel : IConfig
{
    /// <summary>
    /// Gets or sets the value of the endpoints
    /// </summary>
    public Dictionary<string, KestrelEndpoint> Endpoints { get; set; } = [];
}

/// <summary>
/// The kestrel endpoint class
/// </summary>
public class KestrelEndpoint
{
    /// <summary>
    /// Gets or sets the value of the protocols
    /// </summary>
    public string Protocols { get; set; } = "Http1AndHttp2";

    /// <summary>
    /// Gets or sets the value of the url
    /// </summary>
    public string Url { get; set; } = "";

    /// <summary>
    /// Gets or sets the value of the certificate
    /// </summary>
    public KestrelCertificate Certificate { get; set; }
}

/// <summary>
/// The kestrel certificate class
/// </summary>
public class KestrelCertificate
{
    /// <summary>
    /// Gets or sets the value of the path
    /// </summary>
    public string Path { get; set; } = "";

    /// <summary>
    /// Gets or sets the value of the password
    /// </summary>
    public string Password { get; set; } = "";
}
