using LinKit.Core.Abstractions;
using System.Net.Http;
using System.Text.Json;

namespace O24OpenAPI.Framework.Services;

public interface IApiService
{
    Task<T> CallAsync<T>(
        string url,
        HttpMethod method,
        object requestBody = null,
        Dictionary<string, string> headers = null
    );
    Task<string> CallRawAsync(
        string url,
        HttpMethod method,
        object requestBody = null,
        Dictionary<string, string> headers = null
    );
}

[RegisterService(Lifetime.Scoped)]
public class ApiService : IApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonSerializerOptions _jsonOptions;

    public ApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            PropertyNameCaseInsensitive = true,
        };
    }

    public async Task<T> CallAsync<T>(
        string url,
        HttpMethod method,
        object requestBody = null,
        Dictionary<string, string> headers = null
    )
    {
        string raw = await CallRawAsync(url, method, requestBody, headers);
        return string.IsNullOrEmpty(raw)
            ? default
            : JsonSerializer.Deserialize<T>(raw, _jsonOptions);
    }

    public async Task<string> CallRawAsync(
        string url,
        HttpMethod method,
        object requestBody = null,
        Dictionary<string, string> headers = null
    )
    {
        var client = _httpClientFactory.CreateClient("DefaultClient");
        var request = new HttpRequestMessage(method, url);
        if (headers != null)
        {
            foreach (var (key, value) in headers)
                request.Headers.TryAddWithoutValidation(key, value);
        }
        if (
            requestBody != null
            && (method == HttpMethod.Post || method == HttpMethod.Put || method == HttpMethod.Patch)
        )
        {
            string json = JsonSerializer.Serialize(requestBody, _jsonOptions);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }
}
