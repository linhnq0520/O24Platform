using System.Text.Json;
using Microsoft.AspNetCore.Http;
using O24OpenAPI.Core.Helper;

namespace O24OpenAPI.Core.Extensions;

public static class HttpResponseExtensions
{
    /// <summary>
    /// Ghi object ra response body với snake_case JSON
    /// </summary>
    public static async Task WriteJsonSnakeAsync<T>(
        this HttpResponse response,
        T data,
        int? statusCode = null
    )
    {
        if (response.HasStarted)
            return;

        if (statusCode.HasValue)
            response.StatusCode = statusCode.Value;

        response.ContentType = "application/json; charset=utf-8";

        // Reset content-length để tránh lỗi truncate
        response.ContentLength = null;

        await JsonSerializer.SerializeAsync(response.Body, data, JsonHelper.SnakeCaseOptions);
    }
}
