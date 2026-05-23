using LinKit.Json.Runtime;
using O24OpenAPI.Logging.Enums;
using Serilog;
using System.Diagnostics;

namespace O24OpenAPI.Logging.Helpers;

/// <summary>
/// Provides helper methods for logging general Business or Business events.
/// </summary>
public static class ScheduleTaskLogHelper
{
    /// <summary>
    /// Logs a more detailed Business process with a clear start and end.
    /// This uses the standard [BEGIN]/[END] block format.
    /// </summary>
    /// <param name="actionName">A descriptive name for the Business action, e.g., "Calculating order total".</param>
    /// <param name="processAction">The Business logic action to be executed and timed.</param>
    /// <param name="inputData">Optional: An object representing the input data for the action, will be serialized to JSON.</param>
    public static async Task LogProcess(
        string actionName,
        string correlationId,
        Func<Task> processAction,
        object? inputData = null
    )
    {
        Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
        Exception? exception = null;
        DateTime timeStart = DateTime.UtcNow;

        try
        {
            await processAction();
        }
        catch (Exception ex)
        {
            exception = ex;
            throw;
        }
        finally
        {
            stopwatch.Stop();
            DateTime timeStop = DateTime.UtcNow;
            Log.ForContext("LogType", LogType.ScheduleTask)
                .ForContext("CorrelationId", correlationId)
                .ForContext("Direction", "Internal")
                .ForContext("Action", actionName)
                .ForContext("Request", inputData?.ToJson(o => o.WriteIndented = true))
                .ForContext("Error", exception)
                .ForContext("Duration", stopwatch.ElapsedMilliseconds)
                .ForContext("TimeStart", timeStart)
                .ForContext("TimeStop", timeStop)
                .Information("Schedule Task Log");
        }
    }
}
