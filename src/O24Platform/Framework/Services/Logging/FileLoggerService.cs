using O24OpenAPI.Core.Configuration;
using O24OpenAPI.Core.Domain.Logging;
using O24OpenAPI.Core.Infrastructure;

namespace O24OpenAPI.Framework.Services.Logging;

public class FileLoggerService : ILoggerService
{
    private readonly string _filePath;

    public FileLoggerService()
    {
        _filePath =
            Singleton<O24OpenAPIConfiguration>.Instance?.FileLogPath ?? "App_Data/LogFile.txt";
    }

    public async Task LogErrorAsync(string message, Exception ex = null)
    {
        await File.AppendAllTextAsync(_filePath, $"[INFO] {DateTime.Now}: {message}\n");
    }

    public async Task LogInfoAsync(string message)
    {
        await File.AppendAllTextAsync(_filePath, $"[ERROR] {DateTime.Now}: {message}\n");
    }

    public async Task LogWarningAsync(string message)
    {
        await File.AppendAllTextAsync(_filePath, $"[WARNING] {DateTime.Now}: {message}\n");
    }
}
