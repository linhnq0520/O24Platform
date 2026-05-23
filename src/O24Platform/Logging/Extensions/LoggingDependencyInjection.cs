using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using O24OpenAPI.Contracts.Formatters;
using O24OpenAPI.Core.Abstractions;
using O24OpenAPI.Core.Configuration;
using O24OpenAPI.Core.Infrastructure;
using O24OpenAPI.Logging.Handlers;
using O24OpenAPI.Logging.Middlewares;
using O24OpenAPI.Logging.Sinks;
using Serilog;
using Serilog.Events;
using Serilog.Filters;
using Serilog.Sinks.PeriodicBatching;

namespace O24OpenAPI.Logging.Extensions;

public static class LoggingDependencyInjection
{
    public static WebApplicationBuilder AddO24Logging(this WebApplicationBuilder builder)
    {
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddTransient<HttpLoggingHandler>();
        builder.Logging.ClearProviders();
        builder.Host.UseSerilog(
            (context, serviceProvider, loggerConfiguration) =>
            {
                string serviceName =
                    context.Configuration.GetValue<string>("O24Logging:ServiceName")
                    ?? Singleton<O24OpenAPIConfiguration>.Instance!.YourServiceID;
                SinkOptions? sinkOptions = context
                    .Configuration.GetSection("O24Logging:Sink")
                    .Get<SinkOptions>();
                if (sinkOptions == null)
                {
                    try
                    {
                        sinkOptions =
                            Singleton<AppSettings>.Instance!.Get<SinkOptions>()
                            ?? new SinkOptions();
                    }
                    catch
                    {
                        sinkOptions = new SinkOptions();
                    }
                }

                loggerConfiguration
                    .ReadFrom.Configuration(context.Configuration)
                    .Enrich.FromLogContext()
                    .Enrich.WithProperty("ServiceName", serviceName);

                bool writeBusinessToFile =
                    sinkOptions.Mode.Equals("hybrid", StringComparison.CurrentCultureIgnoreCase)
                    || sinkOptions.Mode.Equals(
                        "fileonly",
                        StringComparison.CurrentCultureIgnoreCase
                    );
                bool sendRemote =
                    sinkOptions.Mode.Equals("hybrid", StringComparison.CurrentCultureIgnoreCase)
                    || sinkOptions.Mode.Equals(
                        "remoteonly",
                        StringComparison.CurrentCultureIgnoreCase
                    );

                #region System Logs

                if (sinkOptions.SystemLogToFile)
                {
                    loggerConfiguration.WriteTo.Logger(lc =>
                        lc.Filter.ByExcluding(Matching.WithProperty("LogType"))
                            .WriteTo.Map(
                                logEvent =>
                                {
                                    string? direction =
                                        (
                                            logEvent.Properties.TryGetValue(
                                                "Direction",
                                                out LogEventPropertyValue? d
                                            ) && d is ScalarValue svd
                                        )
                                            ? svd.Value?.ToString()
                                            : "default";
                                    return $"system/{direction}".ToLower();
                                },
                                (key, wt) =>
                                {
                                    string path = Path.Combine("logs", key, "log-.txt");
                                    wt.Async(a =>
                                        a.File(
                                            new CustomTextFormatter(),
                                            path,
                                            rollingInterval: RollingInterval.Day,
                                            retainedFileCountLimit: sinkOptions.RetainedFileCountLimit
                                        )
                                    );
                                }
                            )
                    );
                    Console.WriteLine("[O24Logging] System logs are written to file.");
                }
                else
                {
                    loggerConfiguration.WriteTo.Logger(lc =>
                        lc.Filter.ByExcluding(Matching.WithProperty("LogType"))
                            .WriteTo.Console(new CustomTextFormatter())
                    );
                    Console.WriteLine("[O24Logging] System logs are written to console.");
                }

                #endregion

                #region Business Logs

                if (writeBusinessToFile)
                {
                    loggerConfiguration.WriteTo.Logger(lc =>
                        lc.Filter.ByIncludingOnly(Matching.WithProperty("LogType"))
                            .WriteTo.Map(
                                logEvent =>
                                {
                                    string? logType =
                                        (
                                            logEvent.Properties.TryGetValue(
                                                "LogType",
                                                out LogEventPropertyValue? lt
                                            ) && lt is ScalarValue svlt
                                        )
                                            ? svlt.Value?.ToString()
                                            : "business";
                                    string? direction =
                                        (
                                            logEvent.Properties.TryGetValue(
                                                "Direction",
                                                out LogEventPropertyValue? d
                                            ) && d is ScalarValue svd
                                        )
                                            ? svd.Value?.ToString()
                                            : "default";
                                    return $"{logType}/{direction}".ToLower();
                                },
                                (key, wt) =>
                                {
                                    string path = Path.Combine("logs", key, "log-.txt");
                                    wt.Async(a =>
                                        a.File(
                                            new CustomTextFormatter(),
                                            path,
                                            rollingInterval: RollingInterval.Day,
                                            retainedFileCountLimit: sinkOptions.RetainedFileCountLimit
                                        )
                                    );
                                }
                            )
                    );
                    Console.WriteLine("[O24Logging] Business logs are written to file.");
                }

                #endregion

                #region Remote Logs

                if (sendRemote && !string.IsNullOrEmpty(sinkOptions.Type))
                {
                    ILogSubmitter? submitter = serviceProvider.GetKeyedService<ILogSubmitter>(
                        sinkOptions.Type
                    );
                    if (submitter != null)
                    {
                        loggerConfiguration.WriteTo.Logger(lc =>
                            lc.Filter.ByIncludingOnly(Matching.WithProperty("LogType"))
                                .WriteTo.Sink(
                                    new PeriodicBatchingSink(
                                        new BatchingSubmitterAdapter(submitter),
                                        new PeriodicBatchingSinkOptions
                                        {
                                            BatchSizeLimit = sinkOptions.BatchSizeLimit,
                                            Period = TimeSpan.FromSeconds(
                                                sinkOptions.PeriodSeconds
                                            ),
                                            EagerlyEmitFirstEvent = true,
                                        }
                                    )
                                )
                        );
                        Console.WriteLine(
                            $"[O24Logging] Remote sink is enabled for Business/Communication logs. Type: {sinkOptions.Type.ToUpper()}."
                        );
                    }
                    else
                    {
                        Console.WriteLine(
                            $"[ERROR][O24Logging] Remote sink mode enabled but no implementation for ILogSubmitter is registered. Skipping remote sink."
                        );
                    }
                }

                #endregion
            }
        );

        return builder;
    }

    public static IApplicationBuilder UseO24Logging(this IApplicationBuilder app)
    {
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseMiddleware<RestApiLoggingMiddleware>();
        return app;
    }
}

public class SinkOptions : IConfig
{
    public string Mode { get; set; } = "hybrid";
    public string Type { get; set; } = "rabbitmq";
    public bool SystemLogToFile { get; set; } = false;
    public int BatchSizeLimit { get; set; } = 50;
    public int PeriodSeconds { get; set; } = 2;
    public int RetainedFileCountLimit { get; set; } = 7;
}
