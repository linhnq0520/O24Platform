using System.Diagnostics;
using System.Runtime.ExceptionServices;
using LinKit.Json.Runtime;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using O24OpenAPI.Contracts.Configuration;
using O24OpenAPI.Core.Domain;
using O24OpenAPI.Core.Extensions;
using O24OpenAPI.Core.Infrastructure;
using O24OpenAPI.Logging.Enums;
using Serilog;

namespace O24OpenAPI.Logging.Middlewares;

/// <summary>
/// Middleware to log details of incoming REST API requests and their responses.
/// </summary>
public class RestApiLoggingMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        LoggingConfig loggingConfig = context
            .RequestServices.GetRequiredService<IOptionsMonitor<LoggingConfig>>()
            .CurrentValue;
        if (
            IsGrpcRequest(context)
            || !context.Request.Path.StartsWithSegments("/api")
            || IsPathNoLog(loggingConfig, context.Request.Path)
        )
        {
            await _next(context);
            return;
        }

        string correlationId =
            context.Request.Headers["X-Correlation-ID"].FirstOrDefault()
            ?? EngineContext.Current.ResolveRequired<WorkContext>().ExecutionLogId;
        context.Items["CorrelationId"] = correlationId;

        Stopwatch stopwatch = Stopwatch.StartNew();
        context.Request.EnableBuffering();
        string requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();
        context.Request.Body.Position = 0;
        var originalBodyStream = context.Response.Body;
        using MemoryStream responseBodyStream = new();
        context.Response.Body = responseBodyStream;
        Exception? exception = null;
        Dictionary<string, string> headers = context.Request.Headers.ToDictionary(
            h => h.Key,
            h => h.Value.ToString()
        );
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            exception = ex;
        }
        finally
        {
            stopwatch.Stop();
            responseBodyStream.Seek(0, SeekOrigin.Begin);
            string responseBody = await new StreamReader(responseBodyStream).ReadToEndAsync();
            responseBodyStream.Seek(0, SeekOrigin.Begin);
            await responseBodyStream.CopyToAsync(originalBodyStream);
            LogApiCall(
                context,
                requestBody,
                responseBody,
                exception,
                stopwatch.ElapsedMilliseconds,
                headers
            );
        }

        if (exception is not null)
        {
            ExceptionDispatchInfo.Capture(exception).Throw();
        }
    }

    private static void LogApiCall(
        HttpContext context,
        string requestBody,
        string? responseBody,
        Exception? exception,
        long duration,
        IDictionary<string, string>? headers
    )
    {
        var request = context.Request;

        string fullUrl = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}";
        string? prettyRequest = TryPrettifyJson(requestBody);
        string? prettyResponse = TryPrettifyJson(responseBody);

        var logger = Log.ForContext("LogType", LogType.RestApi)
            .ForContext("Direction", LogDirection.In)
            .ForContext("Action", fullUrl)
            .ForContext("Request", prettyRequest)
            .ForContext("Response", prettyResponse)
            .ForContext("Error", exception)
            .ForContext("Duration", duration)
            .ForContext("Headers", headers.WriteIndentedJson())
            .ForContext(
                "Flow",
                headers is not null && headers.TryGetValue("Flow", out string? flowValue)
                    ? flowValue.ToString()
                    : null
            );
        if (exception is not null)
        {
            logger.Error(exception, exception.Message);
        }
        else
        {
            logger.Information("REST API Call Log");
        }
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
            return dataObject.WriteIndentedJson();
        }
        catch
        {
            return jsonString;
        }
    }

    private static bool IsGrpcRequest(HttpContext context)
    {
        return context.Request.ContentType?.StartsWith(
                "application/grpc",
                StringComparison.OrdinalIgnoreCase
            ) == true;
    }

    private static bool IsPathNoLog(LoggingConfig loggingConfig, PathString path)
    {
        return loggingConfig.PathNoLogRestApi.Contains(path.Value ?? string.Empty);
    }
}
