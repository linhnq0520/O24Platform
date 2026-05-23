using LinKit.Core.Cqrs;
using LinKit.Core.Endpoints;
using O24OpenAPI.Framework.Helpers;
using O24OpenAPI.Framework.Models;
using O24OpenAPI.Framework.Services.Configuration;

namespace O24OpenAPI.Framework.Features.Queries;

[ApiEndpoint(
    ApiMethod.Post,
    "api/o24openapi-services/search",
    Tag = "O24OpenAPIService",
    MediatorKey = "fw"
)]
public class SimpleSearchServiceStepQuery
    : SimpleSearchModel,
        IQuery<PagedListModel<O24OpenAPIServiceSearchResponse>> { }

[CqrsHandler]
public class SimpleSearchServiceStepHandler(IO24OpenAPIMappingService o24OpenAPIMappingService)
    : IQueryHandler<SimpleSearchServiceStepQuery, PagedListModel<O24OpenAPIServiceSearchResponse>>
{
    public async Task<PagedListModel<O24OpenAPIServiceSearchResponse>> HandleAsync(
        SimpleSearchServiceStepQuery request,
        CancellationToken cancellationToken = default
    )
    {
        var pagedList = await o24OpenAPIMappingService.SimpleSearch(request);
        var result = pagedList.ToPagedListModel(
            (items) => items.ToO24OpenAPIServiceSearchResponseList()
        );
        return result;
    }
}
