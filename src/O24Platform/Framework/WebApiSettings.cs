using O24OpenAPI.Core.Configuration;

namespace O24OpenAPI.Framework;

/// <summary>
/// The web api settings class
/// </summary>
/// /// <seealso cref="ISettings"/>
public class WebApiSettings : ISettings
{
    /// <summary>Developer mode</summary>
    /// <value></value>
    public bool DeveloperMode { get; set; }

    /// <summary>Secret key</summary>
    /// <value></value>
    public string SecretKey { get; set; }

    /// <summary>Issuer</summary>
    /// <value></value>
    /// public string Issuer { get; set; }
    public string Issuer { get; set; }

    /// <summary>Audience</summary>
    /// /// <value></value>
    public string Audience { get; set; } = "";

    /// <summary>Token life time in days</summary>
    /// <value></value>
    public int TokenLifetimeDays { get; set; }

    /// <summary>
    /// Gets or sets the value of the token lifetime minutes
    /// </summary>
    public int TokenLifetimeMinutes { get; set; }

    /// <summary>
    /// Gets the value of the private key
    /// </summary>
    internal string PrivateKey { get; } =
        @"MIIBVQIBADANBgkqhkiG9w0BAQEFAASCAT8wggE7AgEAAkEA9PqJnoMQR6KbVcPUHFzxXGqPgwE3
            L5P4jhNg7JkgPdxHlqHfkp5oCRVPMFmW8GF2I6JffE1wKD7QN+uSPIAsvwIDAQABAkAgNkf9Yg/2
            QEl3abrb4tFhpH3GTcYxQeY4lH6VKEn6kZ9AzHqYXRH8vZrY78j0D5gY8ErTKbwm5Q93eLd9J6Vh
            AiEA/8VZF3hL4QAfHW8i5C4y4yXb2QwNA/KJ9k9/yjC5pLUCIQD1MZ8JPx2JFONtkF4JKblC6vEy";

    /// <summary>
    /// Gets or sets the value of the static token lifetime days
    /// </summary>
    public int StaticTokenLifetimeDays { get; set; } = 7;

    /// <summary>Time buffer (in milliseconds)</summary>
    public long BufferTime { get; set; } = 0;

    /// <summary>
    ///
    /// </summary>
    public int SynchronizeLimit { get; set; } = 1000;

    /// <summary>Token life time in days</summary>
    /// <value></value>
    public int TimeoutValidateOtp { get; set; }

    /// <summary>Token life time in days</summary>
    /// <value></value>
    public int PinCodeLength { get; set; }

    /// <summary>Secret key</summary>
    /// <value></value>
    public string LogoBankHeader { get; set; }

    /// <summary>Secret key</summary>
    /// <value></value>
    public string LogoBankFooter { get; set; }

    /// <summary>
    ///
    /// </summary>
    /// <value></value>
    public string Hostport { get; set; }

    /// <summary>
    ///
    /// </summary>
    /// <value></value>
    public string ClientRsaPublicKey { get; set; }

    /// <summary>
    ///
    /// </summary>
    /// <value></value>
    public string TemplateHostDev { get; set; }

    /// <summary>
    ///
    /// </summary>
    /// <value></value>
    public bool Isdev { get; set; }
}
