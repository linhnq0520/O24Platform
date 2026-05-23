using Microsoft.Extensions.DependencyInjection;

namespace O24OpenAPI.Framework.Services.ScheduleTasks;

/// <summary>
/// The schedule task interface
/// </summary>
public interface IScheduleTask
{
    /// <summary>
    /// Executes the last success
    /// </summary>
    /// <param name="lastSuccess">The last success</param>
    /// <param name="serviceScope">The service scope</param>
    Task Execute(DateTime? lastSuccess, IServiceScope serviceScope);
}
