using LinKit.Core.Cqrs;
using LinKit.Core.Endpoints;
using O24OpenAPI.Framework.Helpers;
using O24OpenAPI.Framework.Models;
using O24OpenAPI.Framework.Services;

namespace O24OpenAPI.Framework.Features.Queries;

[ApiEndpoint(
    ApiMethod.Post,
    "api/job-execution-history/search",
    Tag = "JobExecutionHistory",
    MediatorKey = "fw"
)]
public class SimpleSearchJobExecutionHistory
    : SimpleSearchModel,
        IQuery<PagedListModel<JobExecutionHistorySearchResponse>>
{ }

[CqrsHandler]
public class SimpleSearchJobExecutionHistoryHandler(IJobExecutionHistoryService jobExecutionHistoryService)
    : IQueryHandler<SimpleSearchJobExecutionHistory, PagedListModel<JobExecutionHistorySearchResponse>>
{
    public async Task<PagedListModel<JobExecutionHistorySearchResponse>> HandleAsync(
        SimpleSearchJobExecutionHistory request,
        CancellationToken cancellationToken = default
    )
    {
        var pagedList = await jobExecutionHistoryService.SimpleSearch(request);
        var result = pagedList.ToPagedListModel(
            (items) => items.ToJobExecutionHistorySearchResponseList()
        );
        return result;
    }
}
