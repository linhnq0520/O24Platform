namespace O24OpenAPI.Core.Domain.Logging;

public interface ILoggerService
{
    Task LogInfoAsync(string message);
    Task LogWarningAsync(string message);
    Task LogErrorAsync(string message, Exception? ex = null);
}
