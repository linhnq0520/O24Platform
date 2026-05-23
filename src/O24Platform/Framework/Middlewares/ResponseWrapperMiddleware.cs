using LinKit.Json.Runtime;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using O24OpenAPI.Contracts.Models;
using O24OpenAPI.Core;
using O24OpenAPI.Core.Extensions;
using O24OpenAPI.Framework.Exceptions;
using O24OpenAPI.Framework.Extensions;
using O24OpenAPI.Logging.Helpers;
using System.Diagnostics;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace O24OpenAPI.Framework.Middlewares;

#region ApplicationBuilderExtensions
public static partial class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseResponseWrapperExceptGrpc(this IApplicationBuilder app)
    {
        app.UseMiddleware<WorkContextPropagationMiddleware>();
        app.UseWhen(
            ctx => !IsGrpcRequest(ctx) && !IsOpenApi(ctx),
            b => b.UseMiddleware<ResponseWrapperMiddleware>()
        );

        app.UseWhen(
            ctx => !IsGrpcRequest(ctx) && IsOpenApi(ctx),
            b => b.UseMiddleware<OpenApiResponseWrapperMiddleware>()
        );

        return app;
    }

    private static readonly Regex ApiVersionRegex = OpenApiRegex();

    private static bool IsOpenApi(HttpContext ctx)
    {
        return ApiVersionRegex.IsMatch(ctx.Request.Path);
    }

    private static bool IsGrpcRequest(HttpContext ctx)
    {
        return ctx.Request.Protocol == "HTTP/2"
            && ctx.Request.ContentType?.StartsWith("application/grpc") == true;
    }

    [GeneratedRegex(@"^/api/v\d+(/|$)", RegexOptions.Compiled)]
    private static partial Regex OpenApiRegex();
}
#endregion

#region InternalApi
public sealed class ResponseWrapperMiddleware
{
    private readonly RequestDelegate _next;

    public ResponseWrapperMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (
            context.Request.ContentType?.StartsWith("application/grpc") == true
            || context.ShouldSkipResponseWrapper()
        )
        {
            await _next(context);
            return;
        }

        Stopwatch stopwatch = Stopwatch.StartNew();
        Stream originalBody = context.Response.Body;
        WorkContext workContext = context.RequestServices.GetRequiredService<WorkContext>();
        await using MemoryStream buffer = new();
        context.Response.Body = buffer;

        try
        {
            await _next(context);
            stopwatch.Stop();

            if (context.Response.HasStarted)
            {
                buffer.Position = 0;
                await buffer.CopyToAsync(originalBody);
                return;
            }

            if (context.Response.StatusCode == StatusCodes.Status204NoContent)
            {
                context.Response.Body = originalBody;
                return;
            }

            if (context.Response.ContentType?.Contains("application/json") != true)
            {
                buffer.Position = 0;
                context.Response.Body = originalBody;
                await buffer.CopyToAsync(originalBody);
                return;
            }

            buffer.Position = 0;
            string bodyText = await new StreamReader(buffer, Encoding.UTF8).ReadToEndAsync();

            JsonElement? data = null;

            if (!string.IsNullOrWhiteSpace(bodyText))
            {
                try
                {
                    using JsonDocument doc = JsonDocument.Parse(bodyText);
                    data = doc.RootElement.Clone();
                }
                catch
                {
                    buffer.Position = 0;
                    context.Response.Body = originalBody;
                    await buffer.CopyToAsync(originalBody);
                    return;
                }
            }

            string executionId = context
                .RequestServices.GetRequiredService<WorkContext>()
                .ExecutionId;

            BaseResponse<JsonElement?> response = new()
            {
                Success = context.Response.StatusCode is >= 200 and < 300,
                ExecutionId = executionId,
                TimeInMilliseconds = stopwatch.ElapsedMilliseconds,
                Status = context.Response.StatusCode.ToString(),
                Data = data,
            };
            if (!response.Success)
            {
                response.ErrorCode = "exception";
                response.ErrorMessage = response.Data.ToJson();
            }

            context.Response.Body = originalBody;
            context.Response.ContentType = "application/json";
            context.Response.ContentLength = null;
            context.Response.StatusCode = StatusCodes.Status200OK;

            await JsonSerializer.SerializeAsync(context.Response.Body, response);
        }
        catch (Exception ex)
        {
            await WriteWrappedErrorAsync(context, originalBody, stopwatch, ex, workContext);
        }
    }

    private static async Task WriteWrappedErrorAsync(
        HttpContext context,
        Stream originalBody,
        Stopwatch stopwatch,
        Exception ex,
        WorkContext workContext
    )
    {
        BusinessLogHelper.Error(ex, ex.Message);
        stopwatch.Stop();

        context.Response.Body = originalBody;

        if (context.Response.HasStarted)
            ExceptionDispatchInfo.Capture(ex).Throw();

        context.Response.Clear();
        context.Response.StatusCode = StatusCodes.Status200OK;
        context.Response.ContentType = "application/json";

        string errorCode = null;
        string message;
        string stackTrace;

        switch (ex)
        {
            case O24Exception o24:
                O24OpenAPIException o24ex = await o24.ToO24OpenAPIExceptionAsync();
                errorCode = o24ex.ErrorCode;
                message = o24ex.Message;
                stackTrace = o24.StackTrace;
                break;
            case O24OpenAPIException o24Api:
                errorCode = o24Api.ErrorCode;
                message = o24Api.Message;
                stackTrace = o24Api.StackTrace;
                break;
            default:
                Exception detail = ex.InnerException ?? ex;
                message = detail.Message;
                stackTrace = detail.StackTrace;
                break;
        }

        BaseResponse<object> error = new()
        {
            Success = false,
            ErrorCode = errorCode,
            ExecutionId = workContext.ExecutionId,
            TimeInMilliseconds = stopwatch.ElapsedMilliseconds,
            Status = "ERROR",
            ErrorMessage = message,
            StackTrace = stackTrace,
            Description = "Unhandled exception occurred",
        };

        await JsonSerializer.SerializeAsync(context.Response.Body, error);
    }
}
#endregion

#region OpenApi
public sealed class OpenApiResponseWrapperMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.ShouldSkipResponseWrapper())
        {
            await _next(context);
            return;
        }

        Stream originalBody = context.Response.Body;

        await using MemoryStream buffer = new();
        context.Response.Body = buffer;

        Stopwatch sw = Stopwatch.StartNew();

        try
        {
            await _next(context);

            sw.Stop();

            if (context.Response.HasStarted)
                return;

            buffer.Position = 0;
            string body = await new StreamReader(buffer).ReadToEndAsync();

            JsonElement? data = null;

            if (!string.IsNullOrWhiteSpace(body))
            {
                using JsonDocument doc = JsonDocument.Parse(body);
                data = doc.RootElement.Clone();
            }

            OpenApiResponse<JsonElement?> response = new()
            {
                Success = context.Response.StatusCode is >= 200 and < 300,
                Code = context.Response.StatusCode.ToString(),
                Message = "OK",
                Data = data,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            };

            context.Response.Body = originalBody;
            context.Response.ContentType = "application/json";

            await context.Response.WriteJsonSnakeAsync(response);
        }
        catch (O24OpenAPIException o24Ex)
        {
            sw.Stop();

            context.Response.Body = originalBody;

            OpenApiResponse<object> error = new()
            {
                Success = false,
                Code = o24Ex.ErrorCode.Coalesce("exception"),
                Message = o24Ex.Message,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            };

            await context.Response.WriteJsonSnakeAsync(error);
        }
        catch (Exception ex)
        {
            sw.Stop();

            context.Response.Body = originalBody;

            OpenApiResponse<object> error = new()
            {
                Success = false,
                Code = "exception",
                Message = ex.Message,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            };

            await context.Response.WriteJsonSnakeAsync(error);
        }
    }
}
#endregion
