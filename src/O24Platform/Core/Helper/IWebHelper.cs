using Microsoft.AspNetCore.Http;

namespace O24OpenAPI.Core.Helper;

/// <summary>
/// The web helper interface
/// </summary>
public interface IWebHelper
{
    /// <summary>
    /// Gets the url referrer
    /// </summary>
    /// <returns>The string</returns>
    string? GetUrlReferrer();

    /// <summary>
    /// Gets the current ip address
    /// </summary>
    /// <returns>The string</returns>
    string GetCurrentIpAddress();

    /// <summary>
    /// Describes whether this instance is current connection secured
    /// </summary>
    /// <returns>The bool</returns>
    bool IsCurrentConnectionSecured();

    /// <summary>
    /// Restarts the app domain
    /// </summary>
    void RestartAppDomain();

    /// <summary>
    /// Gets the current request protocol
    /// </summary>
    /// <returns>The string</returns>
    string GetCurrentRequestProtocol();

    /// <summary>
    /// Describes whether this instance is local request
    /// </summary>
    /// <param name="req">The req</param>
    /// <returns>The bool</returns>
    bool IsLocalRequest(HttpRequest req);

    /// <summary>
    /// Gets the raw url using the specified request
    /// </summary>
    /// <param name="request">The request</param>
    /// <returns>The string</returns>
    string GetRawUrl(HttpRequest request);

    /// <summary>
    /// Describes whether this instance is ajax request
    /// </summary>
    /// <param name="request">The request</param>
    /// <returns>The bool</returns>
    bool IsAjaxRequest(HttpRequest request);
}
