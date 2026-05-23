using LinKit.Core.Cqrs;
using LinKit.Core.Endpoints;
using O24OpenAPI.Contracts.Constants;
using O24OpenAPI.Framework.Domain;
using O24OpenAPI.Framework.Exceptions;
using O24OpenAPI.Framework.Models;
using O24OpenAPI.Framework.Services;

namespace O24OpenAPI.Framework.Features.Commands;

[ApiEndpoint(
    ApiMethod.Post,
    "api/job-execution-history/delete",
    Tag = "JobExecutionHistory",
    MediatorKey = "fw"
)]
public class DeleteJobExecutionHistoryCommand : ModelWithId, ICommand<JobExecutionLog> { }

[CqrsHandler]
public class DeleteJobExecutionHistoryHandler(
    IJobExecutionHistoryService JobExecutionHistoryService
) : ICommandHandler<DeleteJobExecutionHistoryCommand, JobExecutionLog>
{
    public async Task<JobExecutionLog> HandleAsync(
        DeleteJobExecutionHistoryCommand request,
        CancellationToken cancellationToken = default
    )
    {
        JobExecutionLog entity =
            await JobExecutionHistoryService.GetById(request.Id)
            ?? throw await O24Exception.CreateAsync(ResourceCode.Common.NotExists, request.Id);
        await JobExecutionHistoryService.DeleteAsync(entity);
        return entity;
    }
}
