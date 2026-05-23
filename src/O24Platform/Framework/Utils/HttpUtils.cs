using Microsoft.AspNetCore.Http;

namespace O24OpenAPI.Framework.Utils;

public class HttpUtils
{
    public static Dictionary<string, string> GetHeaders(HttpContext httpContext)
    {
        Dictionary<string, string> requestHeaders = [];
        foreach (var header in httpContext.Request.Headers)
        {
            requestHeaders.Add(header.Key.ToLower(), header.Value);
            Console.WriteLine($"{header.Key}: {header.Value}");
        }

        return requestHeaders;
    }
}
