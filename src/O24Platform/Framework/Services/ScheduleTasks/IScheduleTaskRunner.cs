using Microsoft.Extensions.DependencyInjection;
using O24OpenAPI.Core.Domain;

namespace O24OpenAPI.Framework.Services.ScheduleTasks;

/// <summary>
/// The schedule task runner interface
/// </summary>
public interface IScheduleTaskRunner
{
    /// <summary>
    /// Executes the schedule task
    /// </summary>
    /// <param name="scheduleTask">The schedule task</param>
    /// <param name="serviceScope">The service scope</param>
    /// <param name="forceRun">The force run</param>
    /// <param name="throwException">The throw exception</param>
    /// <param name="ensureRunOncePerPeriod">The ensure run once per period</param>
    Task Execute(
        ScheduleTask scheduleTask,
        IServiceScope serviceScope,
        bool forceRun = false,
        bool throwException = false,
        bool ensureRunOncePerPeriod = true
    );
}
