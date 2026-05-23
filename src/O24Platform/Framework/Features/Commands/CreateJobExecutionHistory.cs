using LinKit.Core.Cqrs;
using LinKit.Core.Endpoints;
using O24OpenAPI.Framework.Domain;
using O24OpenAPI.Framework.Services;

namespace O24OpenAPI.Framework.Features.Commands;

[ApiEndpoint(
    ApiMethod.Post,
    "api/job-execution-history/create",
    Tag = "JobExecutionHistory",
    MediatorKey = "fw"
)]
public class CreateJobExecutionHistoryCommand : ICommand<JobExecutionLog>
{
    public string JobName { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; }
    public string EmbeddedData { get; set; }
}

[CqrsHandler]
public class CreateJobExecutionHistoryHandler(
    IJobExecutionHistoryService jobExecutionHistoryService
) : ICommandHandler<CreateJobExecutionHistoryCommand, JobExecutionLog>
{
    public Task<JobExecutionLog> HandleAsync(
        CreateJobExecutionHistoryCommand request,
        CancellationToken cancellationToken = default
    )
    {
        JobExecutionLog entity = request.ToJobExecutionLog();
        return jobExecutionHistoryService.AddAsync(entity);
    }
}
