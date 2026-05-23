using LinKit.Core.BackgroundJobs;
using O24OpenAPI.Core.SeedWork;
using O24OpenAPI.Framework.Domain;

namespace O24OpenAPI.Framework.Services.Logging;

/// <summary>MyDbJobHistoryLogger</summary>
public class DbJobHistoryLogger(IRepository<JobExecutionLog> jobExecutionHistoryRepository)
    : IJobHistoryLogger
{
    public async Task LogAsync(
        JobExecutionHistory history,
        CancellationToken cancellationToken = default
    )
    {
        var jobExecutionLog = new JobExecutionLog
        {
            JobName = history.JobName,
            StartTime = history.StartTime,
            EndTime = history.EndTime,
            IsSuccess = history.IsSuccess,
            ErrorMessage = history.ErrorMessage,
            EmbeddedData = history.EmbeddedData,
        };
        await jobExecutionHistoryRepository.InsertAsync(jobExecutionLog, cancellationToken);
    }
}
