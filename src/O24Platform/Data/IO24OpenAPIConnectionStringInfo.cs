namespace O24OpenAPI.Data;

/// <summary>
/// The io 24 open api connection string info interface
/// </summary>
public interface IO24OpenAPIConnectionStringInfo
{
    /// <summary>
    /// Gets or sets the value of the database name
    /// </summary>
    string DatabaseName { get; set; }

    /// <summary>
    /// Gets or sets the value of the server name
    /// </summary>
    string ServerName { get; set; }

    /// <summary>
    /// Gets or sets the value of the integrated security
    /// </summary>
    bool IntegratedSecurity { get; set; }

    /// <summary>
    /// Gets or sets the value of the username
    /// </summary>
    string Username { get; set; }

    /// <summary>
    /// Gets or sets the value of the password
    /// </summary>
    string Password { get; set; }

    /// <summary>
    /// Gets or sets the value of the port
    /// </summary>
    uint Port { get; set; }
}
