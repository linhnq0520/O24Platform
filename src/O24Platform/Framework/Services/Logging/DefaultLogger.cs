using O24OpenAPI.Client;
using O24OpenAPI.Client.Events;
using O24OpenAPI.Client.Events.EventData;
using O24OpenAPI.Client.Lib;
using O24OpenAPI.Core;
using O24OpenAPI.Core.Domain.Logging;
using O24OpenAPI.Core.Domain.Users;
using O24OpenAPI.Core.Extensions;
using O24OpenAPI.Core.Infrastructure;
using O24OpenAPI.Core.SeedWork;
using O24OpenAPI.Framework.Domain.Logging;
using O24OpenAPI.Framework.Models.Logging;

namespace O24OpenAPI.Framework.Services.Logging;

/// <summary>Default logger</summary>
public class DefaultLogger(IRepository<Log> logRepository, IWorkContext workContext) : ILogger
{
    /// <summary>
    /// The log repository
    /// </summary>
    private readonly IRepository<Log> _logRepository = logRepository;

    /// <summary>
    /// The work context
    /// </summary>
    private readonly IWorkContext _workContext = workContext;

    /// <summary>
    /// Clears the older than
    /// </summary>
    /// <param name="olderThan">The older than</param>
    /// <exception cref="NotImplementedException"></exception>
    public Task Clear(DateTime? olderThan = null)
    {
        throw new NotImplementedException();
    }

    /// <summary>Deletes a log</summary>
    /// <param name="log"></param>
    /// <returns></returns>
    public virtual async Task Delete(Log log)
    {
        ArgumentNullException.ThrowIfNull(log);
        await _logRepository.Delete(log);
    }

    /// <summary>Error</summary>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    /// <param name="user"></param>
    /// <param name="pageUrl"></param>
    /// <returns></returns>
    public virtual async Task Error(
        string message,
        Exception exception = null,
        User user = null,
        string pageUrl = ""
    )
    {
        if (exception is ThreadAbortException)
        {
            return;
        }

        if (!IsEnabled((LogLevel)40))
        {
            return;
        }

        try
        {
            string shortMessage = message;
            Exception exception1 = exception;
            string fullMessage =
                (exception1 != null ? ExceptionExtensions.DebugMessage(exception1, -1) : null)
                ?? string.Empty;
            User user1 = user;
            string pageUrl1 = pageUrl;
            Log log = await Insert((LogLevel)40, shortMessage, fullMessage, user1, pageUrl1);
        }
        catch { }
    }

    /// <summary>
    /// Gets the all using the specified from utc
    /// </summary>
    /// <param name="fromUtc">The from utc</param>
    /// <param name="toUtc">The to utc</param>
    /// <param name="message">The message</param>
    /// <param name="logLevel">The log level</param>
    /// <param name="pageIndex">The page index</param>
    /// <param name="pageSize">The page size</param>
    /// <exception cref="NotImplementedException"></exception>
    /// <returns>A task containing a paged list of log</returns>
    public Task<IPagedList<Log>> GetAll(
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        string message = "",
        LogLevel? logLevel = null,
        int pageIndex = 0,
        int pageSize = int.MaxValue
    )
    {
        throw new NotImplementedException();
    }

    /// <summary>Gets a log item</summary>
    /// <param name="logId">Log item identifier</param>
    /// <returns>Log item</returns>
    public virtual async Task<Log> GetById(int logId)
    {
        Log byId = await _logRepository.GetById(logId, null);
        return byId;
    }

    /// <summary>Information</summary>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    public virtual async Task Information(
        string message,
        Exception exception = null,
        User user = null
    )
    {
        if (exception is ThreadAbortException || !IsEnabled(LogLevel.Information))
        {
            return;
        }

        string shortMessage = message;
        Exception ex = exception;
        string fullMessage = (ex?.DebugMessage()) ?? string.Empty;
        User user1 = user;
        Log log = await Insert(LogLevel.Information, shortMessage, fullMessage, user1, "");
    }

    /// <summary>
    /// Inserts the log level
    /// </summary>
    /// <param name="logLevel">The log level</param>
    /// <param name="shortMessage">The short message</param>
    /// <param name="fullMessage">The full message</param>
    /// <param name="user">The user</param>
    /// <param name="pageUrl">The page url</param>
    /// <returns>The log</returns>
    public virtual async Task<Log> Insert(
        LogLevel logLevel,
        string shortMessage,
        string fullMessage = "",
        User user = null,
        string pageUrl = ""
    )
    {
        Log log1 = new()
        {
            LogLevel = logLevel,
            ShortMessage = shortMessage,
            FullMessage = fullMessage,
            UserId = user?.Id,
            CreatedOnUtc = DateTime.UtcNow,
        };
        Log log2 = log1;
        string str = await _workContext.GetCurrentRefId();
        log2.ReferredUrl = str;
        log1.PageUrl = pageUrl;
        Log log3 = log1;
        log2 = null;
        str = null;
        log1 = null;
        try
        {
            await _logRepository.Insert(log3);
        }
        catch (Exception ex)
        {
            // Console.BackgroundColor = ConsoleColor.Blue;
            // Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(ex.Message);
        }
        Log log = log3;
        log3 = null;
        return log;
    }

    /// <summary>Warning</summary>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    public virtual async Task Warning(string message, Exception exception = null, User user = null)
    {
        if (exception is ThreadAbortException || !IsEnabled((LogLevel)30))
        {
            return;
        }

        string shortMessage = message;
        Exception exception1 = exception;
        string fullMessage =
            (exception1 != null ? ExceptionExtensions.DebugMessage(exception1, -1) : null)
            ?? string.Empty;
        User user1 = user;
        Log log = await Insert((LogLevel)30, shortMessage, fullMessage, user1, "");
    }

    /// <summary>Determines whether a log level is enabled</summary>
    /// <param name="level">Log level</param>
    /// <returns>Result</returns>
    public virtual bool IsEnabled(LogLevel level)
    {
        if (true)
        {
            ;
        }

        bool flag = level != LogLevel.Debug;
        if (true)
        {
            ;
        }

        return flag;
    }

    /// <summary>
    /// Calls the log generic using the specified request
    /// </summary>
    /// <typeparam name="T">The </typeparam>
    /// <param name="request">The request</param>
    /// <param name="serviceId">The service id</param>
    /// <param name="logType">The log type</param>
    public static async Task CallLogGeneric<T>(T request, string serviceId, string logType)
    {
        try
        {
            LogEventData eventData = new()
            {
                log_type = logType,
                from_service_code = serviceId,
                to_service_code = "LOG",
                event_type = O24OpenAPIWorkflowEventTypeEnum.EventLog.ToString(),
                text_data = System.Text.Json.JsonSerializer.Serialize(request),
            };

            O24OpenAPIEvent<LogEventData> o24Event = new(O24OpenAPIWorkflowEventTypeEnum.EventLog);
            o24Event.EventData.data = eventData;

            QueueClient queueClient = Singleton<QueueClient>.Instance;
            if (queueClient is not null)
            {
                await queueClient.SendMessage(
                    QueueUtils.GetEventQueueName(eventData.to_service_code),
                    o24Event,
                    "event"
                );
            }
        }
        catch (NullReferenceException ex)
        {
            throw new Exception($"Null reference encountered: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error in CallLogGeneric: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Calls the log using the specified request
    /// </summary>
    /// <param name="request">The request</param>
    public static async Task CallLog(CallLogRequest request) =>
        await CallLogGeneric(request, request.ServiceId, "SERVICE_LOG");

    /// <summary>
    /// Calls the log http using the specified request
    /// </summary>
    /// <param name="request">The request</param>
    public static async Task CallLogHttp(HttpLog request) =>
        await CallLogGeneric(request, request.ServiceId, "HTTP_LOG");
}
