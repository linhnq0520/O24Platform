namespace O24OpenAPI.Contracts.Models;

public sealed class ErrorInfo
{
    public string Message { get; init; } = default!;
    public string? StackTrace { get; init; }
    public string? Type { get; init; }
}

public static class ErrorInfoExtensions
{
    public static ErrorInfo ToErrorInfo(this Exception ex, bool includeStackTrace = true)
    {
        return new ErrorInfo
        {
            Message = ex.Message,
            StackTrace = includeStackTrace ? ex.StackTrace : null,
            Type = ex.GetType().FullName,
        };
    }
}
