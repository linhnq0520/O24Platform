using System.Net;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;

namespace O24OpenAPI.Core.Helper;

public class WebHelper(
    IHostApplicationLifetime hostApplicationLifetime,
    IHttpContextAccessor httpContextAccessor,
    IUrlHelperFactory urlHelperFactory
) : IWebHelper
{
    protected readonly IHostApplicationLifetime _hostApplicationLifetime = hostApplicationLifetime;

    protected readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    protected readonly IUrlHelperFactory _urlHelperFactory = urlHelperFactory;

    protected virtual bool IsRequestAvailable()
    {
        if (this._httpContextAccessor?.HttpContext == null)
        {
            return false;
        }

        try
        {
            if (this._httpContextAccessor.HttpContext?.Request == null)
            {
                return false;
            }
        }
        catch (Exception)
        {
            return false;
        }
        return true;
    }

    protected virtual bool IsIpAddressSet(IPAddress? address)
    {
        return address != null && address.ToString() != IPAddress.IPv6Loopback.ToString();
    }

    public virtual string? GetUrlReferrer()
    {
        return !this.IsRequestAvailable()
            ? string.Empty
            : (string?)this._httpContextAccessor.HttpContext?.Request.Headers[HeaderNames.Referer];
    }

    public virtual string GetCurrentIpAddress()
    {
        if (!this.IsRequestAvailable())
        {
            return string.Empty;
        }

        IPAddress? remoteIpAddress = this._httpContextAccessor
            .HttpContext
            ?.Connection
            ?.RemoteIpAddress;
        if (remoteIpAddress == null)
        {
            return "";
        }

        return remoteIpAddress.Equals(IPAddress.IPv6Loopback)
            ? IPAddress.Loopback.ToString()
            : remoteIpAddress.MapToIPv4().ToString();
    }

    /// <summary>
    /// Describes whether this instance is current connection secured
    /// </summary>
    /// <returns>The bool</returns>
    public virtual bool IsCurrentConnectionSecured()
    {
        return this.IsRequestAvailable()
            && this._httpContextAccessor.HttpContext?.Request.IsHttps == true;
    }

    /// <summary>
    /// Restarts the app domain
    /// </summary>
    public virtual void RestartAppDomain() => this._hostApplicationLifetime.StopApplication();

    /// <summary>
    /// Gets the current request protocol
    /// </summary>
    /// <returns>The string</returns>
    public virtual string GetCurrentRequestProtocol()
    {
        return this.IsCurrentConnectionSecured() ? Uri.UriSchemeHttps : Uri.UriSchemeHttp;
    }

    /// <summary>
    /// Describes whether this instance is local request
    /// </summary>
    /// <param name="req">The req</param>
    /// <returns>The bool</returns>
    public virtual bool IsLocalRequest(HttpRequest req)
    {
        ConnectionInfo connection = req.HttpContext.Connection;
        return !this.IsIpAddressSet(connection.RemoteIpAddress)
            || (
                this.IsIpAddressSet(connection.LocalIpAddress)
                    ? connection.RemoteIpAddress == connection.LocalIpAddress
                    : connection.RemoteIpAddress is not null
                        && IPAddress.IsLoopback(connection.RemoteIpAddress)
            );
    }

    /// <summary>
    /// Gets the raw url using the specified request
    /// </summary>
    /// <param name="request">The request</param>
    /// <returns>The raw url</returns>
    public virtual string GetRawUrl(HttpRequest request)
    {
        string? rawUrl = request.HttpContext.Features.Get<IHttpRequestFeature>()?.RawTarget;
        if (string.IsNullOrWhiteSpace(rawUrl))
        {
            DefaultInterpolatedStringHandler interpolatedStringHandler = new(0, 3);
            interpolatedStringHandler.AppendFormatted<PathString>(request.PathBase);
            interpolatedStringHandler.AppendFormatted<PathString>(request.Path);
            interpolatedStringHandler.AppendFormatted<QueryString>(request.QueryString);
            rawUrl = interpolatedStringHandler.ToStringAndClear();
        }
        return rawUrl;
    }

    /// <summary>
    /// Describes whether this instance is ajax request
    /// </summary>
    /// <param name="request">The request</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <returns>The bool</returns>
    public virtual bool IsAjaxRequest(HttpRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return request.Headers != null && request.Headers.XRequestedWith == "XMLHttpRequest";
    }
}
