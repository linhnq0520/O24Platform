using LinKit.Core.Cqrs;
using LinKit.Core.Endpoints;
using O24OpenAPI.Contracts.Constants;
using O24OpenAPI.Framework.Exceptions;
using O24OpenAPI.Framework.Models;
using O24OpenAPI.Framework.Services.Configuration;

namespace O24OpenAPI.Framework.Features.Queries;

public class GetServiceStepByIdQuery : ModelWithId, IQuery<GetServiceStepByIdResponse> { }

[ApiEndpoint(
    ApiMethod.Post,
    "api/o24openapi-service/get",
    Tag = "O24OpenAPIService",
    MediatorKey = "fw"
)]
public class GetServiceStepByIdResponse : BaseO24OpenAPIModel
{
    public int Id { get; set; }
    public string StepCode { get; set; }
    public string FullClassName { get; set; }
    public string MethodName { get; set; }
    public string MediatorKey { get; set; }
    public bool ShouldAwait { get; set; }
    public bool IsInquiry { get; set; }
    public bool IsModuleExecute { get; set; }
    public bool? IsAutoReverse { get; set; }
}

[CqrsHandler]
public class GetServiceStepByIdHandler(IO24OpenAPIMappingService o24OpenAPIMappingService)
    : IQueryHandler<GetServiceStepByIdQuery, GetServiceStepByIdResponse>
{
    public async Task<GetServiceStepByIdResponse> HandleAsync(
        GetServiceStepByIdQuery request,
        CancellationToken cancellationToken = default
    )
    {
        var entity =
            await o24OpenAPIMappingService.GetById(request.Id)
            ?? throw await O24Exception.CreateAsync(ResourceCode.Common.NotFound, request.Id);
        var result = entity.ToGetServiceStepByIdResponse();
        return result;
    }
}
