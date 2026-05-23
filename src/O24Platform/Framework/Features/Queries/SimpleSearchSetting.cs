using LinKit.Core.Cqrs;
using LinKit.Core.Endpoints;
using O24OpenAPI.Framework.Helpers;
using O24OpenAPI.Framework.Models;
using O24OpenAPI.Framework.Services.Configuration;

namespace O24OpenAPI.Framework.Features.Queries;

[ApiEndpoint(ApiMethod.Post, "api/settings/search", Tag = "Setting", MediatorKey = "fw")]
public class SimpleSearchSettingQuery
    : SimpleSearchModel,
        IQuery<PagedListModel<SettingSearchResponse>> { }

[CqrsHandler]
public class SimpleSearchSettingHandler(ISettingService settingService)
    : IQueryHandler<SimpleSearchSettingQuery, PagedListModel<SettingSearchResponse>>
{
    public async Task<PagedListModel<SettingSearchResponse>> HandleAsync(
        SimpleSearchSettingQuery request,
        CancellationToken cancellationToken = default
    )
    {
        var pagedList = await settingService.Search(request);
        var result = pagedList.ToPagedListModel((items) => items.ToSettingSearchResponseList());
        return result;
    }
}
