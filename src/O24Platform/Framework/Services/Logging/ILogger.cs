using O24OpenAPI.Core;
using O24OpenAPI.Core.Domain.Logging;
using O24OpenAPI.Core.Domain.Users;

namespace O24OpenAPI.Framework.Services.Logging;

/// <summary>
/// The logger interface
/// </summary>
public interface ILogger
{
    /// <summary>
    /// Deletes the log
    /// </summary>
    /// <param name="log">The log</param>
    Task Delete(Log log);

    /// <summary>
    /// Clears the older than
    /// </summary>
    /// <param name="olderThan">The older than</param>
    Task Clear(DateTime? olderThan = null);

    /// <summary>
    /// Gets the all using the specified from utc
    /// </summary>
    /// <param name="fromUtc">The from utc</param>
    /// <param name="toUtc">The to utc</param>
    /// <param name="message">The message</param>
    /// <param name="logLevel">The log level</param>
    /// <param name="pageIndex">The page index</param>
    /// <param name="pageSize">The page size</param>
    /// <returns>A task containing a paged list of log</returns>
    Task<IPagedList<Log>> GetAll(
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        string message = "",
        LogLevel? logLevel = null,
        int pageIndex = 0,
        int pageSize = 2147483647
    );

    /// <summary>
    /// Gets the by id using the specified log id
    /// </summary>
    /// <param name="logId">The log id</param>
    /// <returns>A task containing the log</returns>
    Task<Log> GetById(int logId);

    /// <summary>
    /// Inserts the log level
    /// </summary>
    /// <param name="logLevel">The log level</param>
    /// <param name="shortMessage">The short message</param>
    /// <param name="fullMessage">The full message</param>
    /// <param name="user">The user</param>
    /// <param name="pageUrl">The page url</param>
    /// <returns>A task containing the log</returns>
    Task<Log> Insert(
        LogLevel logLevel,
        string shortMessage,
        string fullMessage = "",
        User user = null,
        string pageUrl = ""
    );

    /// <summary>
    /// Informations the message
    /// </summary>
    /// <param name="message">The message</param>
    /// <param name="exception">The exception</param>
    /// <param name="user">The user</param>
    Task Information(string message, Exception exception = null, User user = null);

    /// <summary>
    /// Warnings the message
    /// </summary>
    /// <param name="message">The message</param>
    /// <param name="exception">The exception</param>
    /// <param name="user">The user</param>
    Task Warning(string message, Exception exception = null, User user = null);

    /// <summary>
    /// Errors the message
    /// </summary>
    /// <param name="message">The message</param>
    /// <param name="exception">The exception</param>
    /// <param name="user">The user</param>
    /// <param name="pageUrl">The page url</param>
    Task Error(string message, Exception exception = null, User user = null, string pageUrl = "");
}
