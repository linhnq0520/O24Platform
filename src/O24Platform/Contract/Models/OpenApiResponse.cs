using O24OpenAPI.Contracts.Constants;
using O24OpenAPI.Core.Constants;

namespace O24OpenAPI.Contracts.Models;

public class OpenApiResponse<T>
{
    public bool Success { get; set; }

    public string? Code { get; set; }

    public string? Message { get; set; }

    public T? Data { get; set; }

    public long Timestamp { get; set; }

    public static OpenApiResponse<T> Unauthorized(string message = "Unauthorized")
    {
        return new OpenApiResponse<T>
        {
            Success = false,
            Message = message,
            Code = ResourceCode.OpenApi.Unauthorized,
        };
    }

    // ===== 400 =====
    public static OpenApiResponse<T> BadRequest(string message)
    {
        return new OpenApiResponse<T>
        {
            Success = false,
            Message = message,
            Code = ResourceCode.OpenApi.BadRequest,
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
        };
    }

    // ===== 500 =====
    public static OpenApiResponse<T> SystemError(string message = "System error")
    {
        return new OpenApiResponse<T>
        {
            Success = false,
            Message = message,
            Code = ResourceCode.OpenApi.SystemError,
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
        };
    }
}
