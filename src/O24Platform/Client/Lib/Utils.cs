using System.Net.Http;
using System.Net.Http.Headers;

namespace O24OpenAPI.Client.Lib;

/// <summary>
/// The utils class
/// </summary>
public class Utils
{
    /// <summary>
    /// Gets the current date as long number
    /// </summary>
    /// <returns>The long</returns>
    public static long GetCurrentDateAsLongNumber()
    {
        DateTime dateTime = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        TimeSpan timeSpan = new(DateTime.UtcNow.Ticks - dateTime.Ticks);
        return (long)timeSpan.TotalMilliseconds;
    }

    /// <summary>
    /// Https the ping neptune using the specified p uri
    /// </summary>
    /// <param name="pURI">The uri</param>
    public static void HttpPingNeptune(string pURI)
    {
        using HttpClient httpClient = new();
        httpClient.BaseAddress = new Uri(pURI);
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json")
        );
        Task<HttpResponseMessage> async = httpClient.GetAsync("");
        async.Wait();
        HttpResponseMessage result = async.Result;
        result.EnsureSuccessStatusCode();
        if (result.IsSuccessStatusCode)
        {
            Console.WriteLine("Ping Neptune successfully at " + DateTime.Now.ToUniversalTime());
        }
    }
}
