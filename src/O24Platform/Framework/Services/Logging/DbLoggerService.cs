using O24OpenAPI.Core.Domain;
using O24OpenAPI.Core.Domain.Logging;
using O24OpenAPI.Core.SeedWork;

namespace O24OpenAPI.Framework.Services.Logging;

public class DbLoggerService(IRepository<Log> logRepository, WorkContext workContext)
    : ILoggerService
{
    private readonly IRepository<Log> _logRepository = logRepository;
    private readonly WorkContext _workContext = workContext;

    public async Task InsertAsync(Log log)
    {
        ArgumentNullException.ThrowIfNull(log);
        await _logRepository.InsertAsync(log);
    }

    public async Task LogErrorAsync(string message, Exception? ex = null)
    {
        var log = new Log
        {
            LogLevelId = (int)LogLevel.Error,
            ShortMessage = message,
            FullMessage = ex?.ToString() ?? message,
            PageUrl = _workContext.ExecutionLogId,
            ReferredUrl = _workContext.ExecutionId,
        };
        await InsertAsync(log);
    }

    public async Task LogInfoAsync(string message)
    {
        var log = new Log
        {
            LogLevelId = (int)LogLevel.Information,
            ShortMessage = message,
            FullMessage = message,
            PageUrl = _workContext.ExecutionLogId,
            ReferredUrl = _workContext.ExecutionId,
        };
        await InsertAsync(log);
    }

    public async Task LogWarningAsync(string message)
    {
        var log = new Log
        {
            LogLevelId = (int)LogLevel.Warning,
            ShortMessage = message,
            FullMessage = message,
            PageUrl = _workContext.ExecutionLogId,
            ReferredUrl = _workContext.ExecutionId,
        };
        await InsertAsync(log);
    }
}
