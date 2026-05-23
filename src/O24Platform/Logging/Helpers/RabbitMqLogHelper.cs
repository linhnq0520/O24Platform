using System.Diagnostics;
using System.Text;
using O24OpenAPI.Core.Domain;
using O24OpenAPI.Core.Extensions;
using O24OpenAPI.Core.Infrastructure;
using O24OpenAPI.Logging.Enums;
using Serilog;

namespace O24OpenAPI.Logging.Helpers;

public static class RabbitMqLogHelper
{
    public static void LogPublish<T>(
        string serviceName,
        string destination,
        T message,
        IDictionary<string, object?> headers
    )
    {
        string correlationId = EngineContext.Current.ResolveRequired<WorkContext>().ExecutionLogId;

        using (LogContextHelper.Push(correlationId, serviceName))
        {
            string payload = message.WriteIndentedJson();

            Log.ForContext("LogType", LogType.RabbitMq)
                .ForContext("Direction", LogDirection.Out)
                .ForContext("Action", $"Publish to {destination}")
                .ForContext("Request", payload)
                .ForContext("Headers", headers.WriteIndentedJson())
                .ForContext(
                    "Flow",
                    headers.TryGetValue("Flow", out object? flowValue)
                        ? flowValue?.ToString()
                        : null
                )
                .Information("RabbitMQ Publish Log");
        }
    }

    public static async Task LogConsumeAsync(
        string serviceName,
        string source,
        byte[] body,
        IDictionary<string, object?>? headers,
        Func<Task> processAction
    )
    {
        string correlationId = EngineContext.Current.ResolveRequired<WorkContext>().ExecutionLogId;

        using (LogContextHelper.Push(correlationId, serviceName))
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            string payload = Encoding.UTF8.GetString(body);
            Exception? exception = null;
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
                Log.ForContext("LogType", LogType.RabbitMq)
                    .ForContext("Direction", LogDirection.In)
                    .ForContext("Action", $"Consume from {source}")
                    .ForContext("Request", payload)
                    .ForContext("Error", exception)
                    .ForContext("Duration", stopwatch.ElapsedMilliseconds)
                    .ForContext("Headers", headers.WriteIndentedJson())
                    .Information("RabbitMQ Consume Log");
            }
        }
    }
}
