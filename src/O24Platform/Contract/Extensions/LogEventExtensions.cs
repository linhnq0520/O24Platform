using O24OpenAPI.Contracts.Formatters;
using O24OpenAPI.Contracts.Models;
using Serilog.Events;
using Serilog.Formatting;

namespace O24OpenAPI.Contracts.Extensions;

public static class LogEventExtensions
{
    public static LogEntryModel ToLogEntryModel(this LogEvent logEvent)
    {
        CustomTextFormatter formatter = new();

        string renderedText = logEvent.RenderWithFormatter(formatter);
        LogEntryModel logEntry = new()
        {
            LogTimestamp = logEvent.Timestamp,
            LogLevel = logEvent.Level.ToString(),
            Message = renderedText,
            Exception = logEvent.Exception?.ToString(),
            LogType = GetStringValue(logEvent, "LogType"),
            Direction = GetStringValue(logEvent, "Direction"),
            ActionName = GetStringValue(logEvent, "Action"),
            DurationMs = GetLongValue(logEvent, "Duration"),
            ServiceName = GetStringValue(logEvent, "ServiceName"),
            CorrelationId = GetStringValue(logEvent, "CorrelationId"),
            RequestPayload = GetStringValue(logEvent, "Request"),
            ResponsePayload = GetStringValue(logEvent, "Response"),
            Headers = GetStringValue(logEvent, "Headers"),
            Flow = GetStringValue(logEvent, "Flow"),
        };

        return logEntry;
    }

    public static string RenderWithFormatter(this LogEvent logEvent, ITextFormatter formatter)
    {
        using StringWriter sw = new();
        formatter.Format(logEvent, sw);
        return sw.ToString();
    }

    private static string? GetStringValue(LogEvent logEvent, string key)
    {
        return logEvent.Properties.TryGetValue(key, out LogEventPropertyValue? prop) && prop is ScalarValue scalar
            ? scalar.Value?.ToString()
            : null;
    }

    private static long? GetLongValue(LogEvent logEvent, string key)
    {
        if (
            logEvent.Properties.TryGetValue(key, out LogEventPropertyValue? prop)
            && prop is ScalarValue scalar
            && scalar.Value is long longVal
        )
        {
            return longVal;
        }
        return null;
    }
}
