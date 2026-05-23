using LinKit.Core.Cqrs;
using LinKit.Core.Endpoints;
using O24OpenAPI.Contracts.Constants;
using O24OpenAPI.Framework.Exceptions;
using O24OpenAPI.Framework.Models;
using O24OpenAPI.Framework.Services;

namespace O24OpenAPI.Framework.Features.Queries;

public class GetJobExecutionHistoryByIdQuery : ModelWithId, IQuery<GetJobExecutionHistoryByIdResponse> { }

[ApiEndpoint(
    ApiMethod.Post,
    "api/job-execution-history/get",
    Tag = "JobExecutionHistory",
    MediatorKey = "fw"
)]
public class GetJobExecutionHistoryByIdResponse : BaseO24OpenAPIModel
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
public class GetJobExecutionHistoryByIdHandler(IJobExecutionHistoryService jobExecutionHistoryService)
    : IQueryHandler<GetJobExecutionHistoryByIdQuery, GetJobExecutionHistoryByIdResponse>
{
    public async Task<GetJobExecutionHistoryByIdResponse> HandleAsync(
        GetJobExecutionHistoryByIdQuery request,
        CancellationToken cancellationToken = default
    )
    {
        var entity =
            await jobExecutionHistoryService.GetById(request.Id)
            ?? throw await O24Exception.CreateAsync(ResourceCode.Common.NotFound, request.Id);
        var result = entity.ToGetJobExecutionHistoryByIdResponse();
        return result;
    }
}
