using Microsoft.Extensions.DependencyInjection;
using O24OpenAPI.Core.Caching;
using O24OpenAPI.Core.Domain;
using O24OpenAPI.Core.Extensions;
using O24OpenAPI.Core.Infrastructure;
using O24OpenAPI.Framework.Localization;
using O24OpenAPI.Framework.Services.Logging;

namespace O24OpenAPI.Framework.Services.ScheduleTasks;

/// <summary>
/// The schedule task runner class
/// </summary>
/// <seealso cref="IScheduleTaskRunner"/>
public class ScheduleTaskRunner : IScheduleTaskRunner
{
    /// <summary>
    /// The localization service
    /// </summary>
    protected ILocalizationService _localizationService;

    /// <summary>
    /// The locker
    /// </summary>
    protected ILocker _locker;

    /// <summary>
    /// The logger
    /// </summary>
    protected ILogger _logger;

    /// <summary>
    /// The schedule task service
    /// </summary>
    protected IScheduleTaskService _scheduleTaskService;

    /// <summary>
    /// Initializes the services using the specified service scope
    /// </summary>
    /// <param name="serviceScope">The service scope</param>
    protected void InitializeServices(IServiceScope serviceScope)
    {
        _logger = serviceScope.ServiceProvider.GetRequiredService<ILogger>();
        _locker = serviceScope.ServiceProvider.GetRequiredService<ILocker>();
        _localizationService =
            serviceScope.ServiceProvider.GetRequiredService<ILocalizationService>();
        _scheduleTaskService = serviceScope.ServiceProvider.GetService<IScheduleTaskService>();
    }

    /// <summary>
    /// Executes the task using the specified schedule task
    /// </summary>
    /// <param name="scheduleTask">The schedule task</param>
    /// <param name="serviceScope">The service scope</param>
    /// <exception cref="Exception"></exception>
    protected void ExecuteTask(ScheduleTask scheduleTask, IServiceScope serviceScope)
    {
        InitializeServices(serviceScope);
        Type type1 = Type.GetType(scheduleTask.Type);
        type1 ??= AppDomain
            .CurrentDomain.GetAssemblies()
            .Select(a => a.GetType(scheduleTask.Type))
            .FirstOrDefault(t => t != null);

        Type type2 =
            type1
            ?? throw new Exception(
                "Schedule task (" + scheduleTask.Type + ") cannot by instantiated Assemblies"
            );
        object obj = null;
        try
        {
            obj = EngineContext.Current.Resolve(type2);
        }
        catch { }
        obj ??= EngineContext.Current.ResolveUnregistered(type2);
        if (obj is not IScheduleTask scheduleTask1)
        {
            return;
        }
        scheduleTask.LastStartUtc = new DateTime?(DateTime.UtcNow);
        _scheduleTaskService.Update(scheduleTask).GetAsyncResult();
        scheduleTask1.Execute(scheduleTask.LastSuccessUtc, serviceScope).GetAsyncResult();
        ScheduleTask scheduleTask2 = scheduleTask;
        ScheduleTask scheduleTask3 = scheduleTask;
        DateTime? nullable1 = new DateTime?(DateTime.UtcNow);
        DateTime? nullable2 = nullable1;
        scheduleTask3.LastSuccessUtc = nullable2;
        DateTime? nullable3 = nullable1;
        scheduleTask2.LastEndUtc = nullable3;
        _scheduleTaskService.Update(scheduleTask).GetAsyncResult();
    }

    /// <summary>
    /// Is the task already running using the specified schedule task
    /// </summary>
    /// <param name="scheduleTask">The schedule task</param>
    /// <returns>The bool</returns>
    protected virtual bool IsTaskAlreadyRunning(ScheduleTask scheduleTask)
    {
        DateTime? nullable;
        int num1;
        if (!scheduleTask.LastStartUtc.HasValue)
        {
            nullable = scheduleTask.LastEnabledUtc;
            num1 = !nullable.HasValue ? 1 : 0;
        }
        else
        {
            num1 = 0;
        }

        if (num1 != 0)
        {
            return false;
        }

        nullable = scheduleTask.LastStartUtc;
        DateTime dateTime1 = nullable ?? DateTime.UtcNow;
        nullable = scheduleTask.LastEnabledUtc;
        int num2;
        if (nullable.HasValue)
        {
            DateTime dateTime2 = dateTime1;
            nullable = scheduleTask.LastEnabledUtc;
            num2 = nullable.HasValue ? (dateTime2 < nullable.GetValueOrDefault() ? 1 : 0) : 0;
        }
        else
        {
            num2 = 0;
        }

        return num2 == 0 && !(dateTime1.AddSeconds(scheduleTask.Seconds) <= DateTime.UtcNow);
    }

    /// <summary>
    /// Executes the schedule task
    /// </summary>
    /// <param name="scheduleTask">The schedule task</param>
    /// <param name="serviceScope">The service scope</param>
    /// <param name="forceRun">The force run</param>
    /// <param name="throwException">The throw exception</param>
    /// <param name="ensureRunOncePerPeriod">The ensure run once per period</param>
    public async Task Execute(
        ScheduleTask scheduleTask,
        IServiceScope serviceScope,
        bool forceRun = false,
        bool throwException = false,
        bool ensureRunOncePerPeriod = true
    )
    {
        InitializeServices(serviceScope);
        int num1;
        if (!forceRun)
        {
            ScheduleTask scheduleTask1 = scheduleTask;
            num1 = scheduleTask1 != null ? (scheduleTask1.Enabled ? 1 : 0) : 0;
        }
        else
        {
            num1 = 1;
        }
        bool enabled = num1 != 0;
        if (scheduleTask == null || !enabled) { }
        else
        {
            if (ensureRunOncePerPeriod)
            {
                if (IsTaskAlreadyRunning(scheduleTask))
                {
                    return;
                }
                DateTime? lastStartUtc = scheduleTask.LastStartUtc;
                int num2;
                if (lastStartUtc.HasValue)
                {
                    DateTime utcNow = DateTime.UtcNow;
                    lastStartUtc = scheduleTask.LastStartUtc;
                    num2 =
                        (
                            lastStartUtc.HasValue
                                ? new TimeSpan?(utcNow - lastStartUtc.GetValueOrDefault())
                                : new TimeSpan?()
                        )
                            .Value
                            .TotalSeconds < scheduleTask.Seconds
                            ? 1
                            : 0;
                }
                else
                {
                    num2 = 0;
                }

                if (num2 != 0)
                {
                    return;
                }
            }
            try
            {
                int expirationInSeconds = Math.Min(scheduleTask.Seconds, 300) - 1;
                TimeSpan expiration = TimeSpan.FromSeconds(expirationInSeconds);

                ExecuteTask(scheduleTask, serviceScope);
                expiration = new TimeSpan();
            }
            catch (Exception ex)
            {
                scheduleTask.Enabled = !scheduleTask.StopOnError;
                scheduleTask.LastEndUtc = new DateTime?(DateTime.UtcNow);
                await _scheduleTaskService.Update(scheduleTask);
                string message = ex.Message;
                await _logger.Error(message, ex);
                if (throwException)
                {
                    throw;
                }

                message = null;
            }
        }
    }
}
