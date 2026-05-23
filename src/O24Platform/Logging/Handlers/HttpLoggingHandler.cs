using LinKit.Json.Runtime;
using O24OpenAPI.Core.Configuration;
using O24OpenAPI.Core.Infrastructure;
using O24OpenAPI.Logging.Enums;
using Serilog;
using System.Diagnostics;
using System.Net.Http;

namespace O24OpenAPI.Logging.Handlers;

/// <summary>
/// A delegating handler for HttpClient that logs outgoing API requests and their responses.
/// </summary>
public class HttpLoggingHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        string serviceNameProp = Singleton<O24OpenAPIConfiguration>.Instance.YourServiceID;
        string? correlationIdProp = EngineContext.Current.Resolve<WorkContext>()?.ExecutionLogId;
        string serviceName = serviceNameProp ?? "UnknownService";
        string correlationId = correlationIdProp ?? Guid.NewGuid().ToString("N");

        if (!request.Headers.Contains("X-Correlation-ID"))
        {
            request.Headers.Add("X-Correlation-ID", correlationId);
        }

        var stopwatch = Stopwatch.StartNew();
        string? requestBody = null;
        if (request.Content != null)
        {
            requestBody = await request.Content.ReadAsStringAsync(cancellationToken);
        }

        HttpResponseMessage? response = null;
        Exception? exception = null;
        try
        {
            response = await base.SendAsync(request, cancellationToken);
            return response;
        }
        catch (Exception ex)
        {
            exception = ex;
            throw;
        }
        finally
        {
            stopwatch.Stop();
            string? responseBody = null;
            if (response?.Content != null)
            {
                responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            }

            LogOutgoingApiCall(
                serviceName,
                request,
                requestBody,
                response,
                responseBody,
                exception,
                stopwatch.ElapsedMilliseconds
            );
        }
    }

    private static void LogOutgoingApiCall(
        string serviceName,
        HttpRequestMessage request,
        string? requestBody,
        HttpResponseMessage? response,
        string? responseBody,
        Exception? exception,
        long duration
    )
    {
        string action = $"{request.Method} {request.RequestUri}";

        Log.ForContext("LogType", LogType.RestApi)
            .ForContext("Direction", LogDirection.Out)
            .ForContext("Action", action)
            .ForContext("Request", TryPrettifyJson(requestBody))
            .ForContext("Response", TryPrettifyJson(responseBody))
            .ForContext("Error", exception)
            .ForContext("Duration", duration)
            .Information("Outgoing REST API Call Log");
    }

    private static string? TryPrettifyJson(string? jsonString)
    {
        if (string.IsNullOrWhiteSpace(jsonString))
        {
            return jsonString;
        }

        try
        {
            object? dataObject = jsonString.FromJson<object>();
            return dataObject.ToJson(o => o.WriteIndented = true);
        }
        catch
        {
            return jsonString;
        }
    }
}
