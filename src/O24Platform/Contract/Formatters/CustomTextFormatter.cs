using Serilog.Events;
using Serilog.Formatting;
using System.IO;

namespace O24OpenAPI.Contracts.Formatters;

public class CustomTextFormatter : ITextFormatter
{
    public void Format(LogEvent logEvent, TextWriter output)
    {
        bool isBlockLog = logEvent.Properties.ContainsKey("Direction");

        if (isBlockLog)
        {
            FormatBlockLog(logEvent, output);
        }
        else
        {
            FormatSimpleLog(logEvent, output);
        }
    }

    private static void FormatSimpleLog(LogEvent logEvent, TextWriter output)
    {
        string serviceName = GetPropertyValue(logEvent, "ServiceName");
        string traceId = GetPropertyValue(logEvent, "CorrelationId");

        output.Write($"[{logEvent.Timestamp:yyyy-MM-dd HH:mm:ss.fff}] ");
        output.Write($"[{logEvent.Level.ToString().ToUpper()}] ");
        output.Write($"[{serviceName}] ");
        if (!string.IsNullOrEmpty(traceId))
        {
            output.Write($"[{traceId}] ");
        }
        logEvent.RenderMessage(output);

        if (logEvent.Exception != null)
        {
            output.WriteLine();
            WriteExceptionDetails(logEvent.Exception, output); // ← SỬA
        }
        output.WriteLine();
    }

    private static void FormatBlockLog(LogEvent logEvent, TextWriter output)
    {
        StringBuilder sb = new();
        sb.AppendLine("----------------------------------------");

        string serviceName = GetPropertyValue(logEvent, "ServiceName");
        string action = GetPropertyValue(logEvent, "Action");
        string traceId = GetPropertyValue(logEvent, "CorrelationId");
        string request = GetPropertyValue(logEvent, "Request");
        string response = GetPropertyValue(logEvent, "Response");
        var error = logEvent.Exception;
        string duration = GetPropertyValue(logEvent, "Duration");
        string headers = GetPropertyValue(logEvent, "Headers");

        sb.AppendLine($"[BEGIN] {serviceName} | {action}");
        sb.AppendLine($"[Time  ] {logEvent.Timestamp:yyyy-MM-dd HH:mm:ss.fff}");
        sb.AppendLine($"[Trace ] {traceId}");
        sb.AppendLine($"[Headers] {headers}");

        if (!string.IsNullOrEmpty(request))
        {
            sb.AppendLine();
            sb.AppendLine("[REQUEST]");
            sb.AppendLine(request);
        }

        if (error != null)
        {
            sb.AppendLine();
            sb.AppendLine("[ERROR]");

            using StringWriter stringWriter = new();
            WriteExceptionDetails(error, stringWriter);
            sb.Append(stringWriter.ToString());
        }
        else if (!string.IsNullOrEmpty(response))
        {
            sb.AppendLine();
            sb.AppendLine("[RESPONSE]");
            sb.AppendLine(response);
        }

        sb.AppendLine();
        if (error != null)
        {
            sb.AppendLine($"[END] Failed after {duration} ms");
        }
        else
        {
            sb.AppendLine($"[END] Duration: {duration} ms");
        }

        sb.AppendLine("----------------------------------------");
        output.Write(sb.ToString());
    }

    private static void WriteExceptionDetails(Exception exception, TextWriter output)
    {
        var exceptionLevel = 0;
        var currentException = exception;

        while (currentException != null)
        {
            var indent = new string(' ', exceptionLevel * 2);

            if (exceptionLevel == 0)
            {
                output.WriteLine($"{indent}Exception Type: {currentException.GetType().FullName}");
            }
            else
            {
                output.WriteLine(
                    $"{indent}└─ Inner Exception #{exceptionLevel}: {currentException.GetType().FullName}"
                );
            }

            output.WriteLine($"{indent}   Message: {currentException.Message}");

            if (currentException is System.Text.Json.JsonException jsonEx)
            {
                output.WriteLine($"{indent}   JSON Path: {jsonEx.Path ?? "N/A"}");
                output.WriteLine(
                    $"{indent}   Line Number: {jsonEx.LineNumber?.ToString() ?? "N/A"}"
                );
                output.WriteLine(
                    $"{indent}   Byte Position: {jsonEx.BytePositionInLine?.ToString() ?? "N/A"}"
                );
            }

            if (currentException is Microsoft.AspNetCore.Http.BadHttpRequestException badHttpEx)
            {
                output.WriteLine($"{indent}   Status Code: {badHttpEx.StatusCode}");
            }

            if (!string.IsNullOrEmpty(currentException.StackTrace))
            {
                output.WriteLine($"{indent}   Stack Trace:");
                var stackLines = currentException.StackTrace.Split('\n');
                foreach (var line in stackLines)
                {
                    output.WriteLine($"{indent}     {line.TrimEnd()}");
                }
            }

            output.WriteLine();
            currentException = currentException.InnerException;
            exceptionLevel++;
        }

        output.WriteLine($"Total exception levels: {exceptionLevel}");
    }

    private static string GetPropertyValue(LogEvent logEvent, string propertyName)
    {
        if (
            logEvent.Properties.TryGetValue(propertyName, out var propertyValue)
            && propertyValue is Serilog.Events.ScalarValue scalarValue
        )
        {
            return scalarValue.Value?.ToString() ?? string.Empty;
        }
        return string.Empty;
    }
}
