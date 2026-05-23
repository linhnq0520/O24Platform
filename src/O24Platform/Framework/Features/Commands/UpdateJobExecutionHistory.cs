using LinKit.Core.Cqrs;
using LinKit.Core.Endpoints;
using O24OpenAPI.Contracts.Constants;
using O24OpenAPI.Framework.Domain;
using O24OpenAPI.Framework.Exceptions;
using O24OpenAPI.Framework.Services;

namespace O24OpenAPI.Framework.Features.Commands;

[ApiEndpoint(
    ApiMethod.Post,
    "api/job-execution-history/update",
    Tag = "JobExecutionHistory",
    MediatorKey = "fw"
)]
public class UpdateJobExecutionHistoryCommand : ICommand<JobExecutionLog>
{
    public int Id { get; set; }
    public string JobName { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public string? EmbeddedData { get; set; }
}

[CqrsHandler]
public class UpdateJobExecutionHistoryHandler(IJobExecutionHistoryService jobExecutionHistory)
    : ICommandHandler<UpdateJobExecutionHistoryCommand, JobExecutionLog>
{
    public async Task<JobExecutionLog> HandleAsync(
        UpdateJobExecutionHistoryCommand request,
        CancellationToken cancellationToken = default
    )
    {
        JobExecutionLog entity =
            await jobExecutionHistory.GetById(request.Id)
            ?? throw await O24Exception.CreateAsync(ResourceCode.Common.NotExists, request.Id);
        entity = request.ToJobExecutionLog(entity);
        await jobExecutionHistory.UpdateAsync(entity);
        return entity;
    }
}
