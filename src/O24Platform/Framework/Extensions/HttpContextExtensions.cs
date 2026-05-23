using LinKit.Json.Runtime;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace O24OpenAPI.Framework.Extensions;

public static class HttpContextExtensions
{
    public static Dictionary<string, string> GetHeaders(this HttpContext httpContext)
    {
        Dictionary<string, string> requestHeaders = [];

        foreach (KeyValuePair<string, StringValues> header in httpContext.Request.Headers)
        {
            requestHeaders[header.Key.ToLowerInvariant()] = header.Value.ToString();
        }

        return requestHeaders;
    }

    public static T GetHeaderValue<T>(this HttpContext context, string key)
    {
        if (!context.Request.Headers.TryGetValue(key, out var raw))
            return default;

        var str = raw.ToString();
        if (string.IsNullOrWhiteSpace(str))
        {
            return default;
        }

        if (typeof(T) == typeof(string))
            return (T)(object)str;

        if (typeof(T).IsPrimitive || typeof(T) == typeof(decimal))
            return (T)Convert.ChangeType(str, typeof(T));

        return str.FromJson<T>();
    }

    public static Dictionary<string, string> GetCookies(this HttpContext httpContext)
    {
        Dictionary<string, string> requestCookies = [];

        foreach (KeyValuePair<string, string> header in httpContext.Request.Cookies)
        {
            requestCookies[header.Key.ToLowerInvariant()] = header.Value;
        }

        return requestCookies;
    }

    public static string GetClientIPAddress(this HttpContext httpContext)
    {
        return httpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
    }

    public static string GetClientOs(this HttpContext httpContext)
    {
        string ua = httpContext.Request.Headers["User-Agent"].ToString();

        if (ua.Contains("Android"))
            return $"Android {GetMobileVersion(ua, "Android")}";

        if (ua.Contains("iPad"))
            return $"iPad OS {GetMobileVersion(ua, "OS")}";

        if (ua.Contains("iPhone"))
            return $"iPhone OS {GetMobileVersion(ua, "OS")}";

        if (ua.Contains("Linux") && ua.Contains("KFAPWI"))
            return "Kindle Fire";

        if (ua.Contains("RIM Tablet") || (ua.Contains("BB") && ua.Contains("Mobile")))
            return "Black Berry";

        if (ua.Contains("Windows Phone"))
            return $"Windows Phone {GetMobileVersion(ua, "Windows Phone")}";

        if (ua.Contains("Mac OS"))
            return "Mac OS";

        if (ua.Contains("Windows NT 5.1") || ua.Contains("Windows NT 5.2"))
            return "Windows XP";

        if (ua.Contains("Windows NT 6.0"))
            return "Windows Vista";

        if (ua.Contains("Windows NT 6.1"))
            return "Windows 7";

        if (ua.Contains("Windows NT 6.2"))
            return "Windows 8";

        if (ua.Contains("Windows NT 6.3"))
            return "Windows 8.1";

        if (ua.Contains("Windows NT 10"))
            return "Windows 10";

        return string.Empty;
    }

    public static string GetClientBrowser(this HttpContext httpContext)
    {
        string browserDetails = httpContext.Request.Headers["User-Agent"].ToString();
        string user = browserDetails.ToLower();

        if (user.Contains("msie"))
        {
            var substring = browserDetails.Substring(browserDetails.IndexOf("MSIE")).Split(";")[0];
            return substring.Split(" ")[0].Replace("MSIE", "IE") + "-" + substring.Split(" ")[1];
        }

        if (user.Contains("safari") && user.Contains("version"))
        {
            return "Safari-"
                + browserDetails
                    .Substring(browserDetails.IndexOf("Version"))
                    .Split(" ")[0]
                    .Split("/")[1];
        }

        if (user.Contains("opr") || user.Contains("opera"))
        {
            if (user.Contains("opera"))
            {
                return "Opera-"
                    + browserDetails
                        .Substring(browserDetails.IndexOf("Version"))
                        .Split(" ")[0]
                        .Split("/")[1];
            }

            return browserDetails
                .Substring(browserDetails.IndexOf("OPR"))
                .Split(" ")[0]
                .Replace("/", "-")
                .Replace("OPR", "Opera");
        }

        if (user.Contains("edg"))
        {
            return browserDetails
                .Substring(browserDetails.IndexOf("Edg"))
                .Split(" ")[0]
                .Replace("/", "-");
        }

        if (user.Contains("firefox"))
        {
            return browserDetails
                .Substring(browserDetails.IndexOf("Firefox"))
                .Split(" ")[0]
                .Replace("/", "-");
        }

        if (user.Contains("chrome"))
        {
            return browserDetails
                .Substring(browserDetails.IndexOf("Chrome"))
                .Split(" ")[0]
                .Replace("/", "-");
        }

        return "Unknown";
    }

    private static string GetMobileVersion(string userAgent, string device)
    {
        string temp = userAgent[(userAgent.IndexOf(device) + device.Length)..].TrimStart();
        string version = string.Empty;

        foreach (char character in temp)
        {
            if (char.IsDigit(character) || character == '.' || character == '_')
            {
                version += character == '_' ? '.' : character;
            }
            else
            {
                break;
            }
        }

        return version;
    }
}
