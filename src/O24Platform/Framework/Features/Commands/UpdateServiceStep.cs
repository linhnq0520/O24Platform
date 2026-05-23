using LinKit.Core.Cqrs;
using LinKit.Core.Endpoints;
using O24OpenAPI.Contracts.Constants;
using O24OpenAPI.Framework.Domain;
using O24OpenAPI.Framework.Exceptions;
using O24OpenAPI.Framework.Services.Configuration;

namespace O24OpenAPI.Framework.Features.Commands;

[ApiEndpoint(
    ApiMethod.Post,
    "api/o24openapi-services/update",
    Tag = "O24OpenAPIService",
    MediatorKey = "fw"
)]
public class UpdateServiceStepCommand : ICommand<O24OpenAPIService>
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
public class UpdateServiceStepHandler(IO24OpenAPIMappingService o24OpenAPIMappingService)
    : ICommandHandler<UpdateServiceStepCommand, O24OpenAPIService>
{
    public async Task<O24OpenAPIService> HandleAsync(
        UpdateServiceStepCommand request,
        CancellationToken cancellationToken = default
    )
    {
        var entity =
            await o24OpenAPIMappingService.GetById(request.Id)
            ?? throw await O24Exception.CreateAsync(ResourceCode.Common.NotExists, request.Id);
        entity = request.ToO24OpenAPIService(entity);
        await o24OpenAPIMappingService.UpdateAsync(entity);
        return entity;
    }
}
