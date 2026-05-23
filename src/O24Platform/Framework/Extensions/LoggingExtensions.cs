using Newtonsoft.Json;
using O24OpenAPI.Core.Domain.Logging;
using O24OpenAPI.Core.Extensions;
using O24OpenAPI.Core.Infrastructure;
using O24OpenAPI.Framework.Domain.Logging;
using O24OpenAPI.Framework.Models.Logging;
using O24OpenAPI.Framework.Services.Logging;

namespace O24OpenAPI.Framework.Extensions;

/// <summary>
/// The logging extensions class
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    /// Logs the info using the specified obj
    /// </summary>
    /// <param name="obj">The obj</param>
    /// <param name="message">The message</param>
    /// <param name="userId">The user id</param>
    public static async Task LogInfoAsync(
        this object obj,
        string message = null,
        string userId = null
    )
    {
        try
        {
            var request = CreateLogRequest(
                LogLevel.Information,
                message,
                null,
                obj,
                userId: userId
            );
            await DefaultLogger.CallLog(request);
        }
        catch (Exception e)
        {
            var loggerService = EngineContext.Current.Resolve<ILogger>();
            await loggerService.Error(e.Message, e, null, JsonConvert.SerializeObject(obj));
        }
    }

    /// <summary>
    /// Logs the error using the specified ex
    /// </summary>
    /// <param name="ex">The ex</param>
    /// <param name="obj">The obj</param>
    /// <param name="userId">The user id</param>
    public static async Task LogErrorAsync(
        this Exception ex,
        object obj = null,
        string userId = null
    )
    {
        try
        {
            var request = CreateLogRequest(
                LogLevel.Error,
                ex.Message,
                null,
                obj,
                ex.StackTrace,
                userId
            );
            await DefaultLogger.CallLog(request);
        }
        catch (Exception e)
        {
            var loggerService = EngineContext.Current.Resolve<ILogger>();
            if (loggerService != null)
            {
                await loggerService.Error(e.Message, e, null, JsonConvert.SerializeObject(obj));
            }
            else
            {
                var fileLogService = EngineContext.Current.Resolve<ILoggerService>();
                if (fileLogService != null)
                {
                    await fileLogService.LogErrorAsync(e.Message, e);
                }
            }
        }
    }

    public static async Task WriteErrorAsync(
        this string message,
        string code,
        object data = null,
        string stackTrace = null
    )
    {
        try
        {
            var request = CreateLogRequest(LogLevel.Error, message, code, data, stackTrace, null);
            await DefaultLogger.CallLog(request);
        }
        catch (Exception e)
        {
            var loggerService = EngineContext.Current.Resolve<ILogger>();
            await loggerService.Error(e.Message, e, null, JsonConvert.SerializeObject(data));
        }
    }

    /// <summary>
    /// Logs the error using the specified ex
    /// </summary>
    /// <param name="ex">The ex</param>
    /// <param name="obj">The obj</param>
    public static async Task LogError(this Exception ex, object obj = null)
    {
        var loggerService = EngineContext.Current.Resolve<ILogger>();
        await loggerService.Error(ex.Message, ex, null, JsonConvert.SerializeObject(obj));
    }

    /// <summary>
    /// Logs the http using the specified log
    /// </summary>
    /// <param name="log">The log</param>
    /// <param name="userId">The user id</param>
    public static async Task LogHttpAsync(this HttpLog log, string userId = null)
    {
        await DefaultLogger.CallLogHttp(log);
    }

    /// <summary>
    /// Creates the log request using the specified level
    /// </summary>
    /// <param name="level">The level</param>
    /// <param name="message">The message</param>
    /// <param name="obj">The obj</param>
    /// <param name="stackTrace">The stack trace</param>
    /// <param name="userId">The user id</param>
    /// <returns>The request</returns>
    public static CallLogRequest CreateLogRequest(
        LogLevel level,
        string message,
        string code,
        object obj = null,
        string stackTrace = null,
        string userId = null
    )
    {
        var request = new CallLogRequest
        {
            LogLevel = level,
            FullMessage = stackTrace ?? message,
            ShortMessage = message,
            Data = obj?.ToSerializeSystemText(),
            Code = code,
        };
        if (userId != null)
        {
            request.UserId = userId;
        }

        return request;
    }
}
