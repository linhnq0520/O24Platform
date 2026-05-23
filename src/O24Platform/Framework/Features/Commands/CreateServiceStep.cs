using LinKit.Core.Cqrs;
using LinKit.Core.Endpoints;
using O24OpenAPI.Framework.Domain;
using O24OpenAPI.Framework.Services.Configuration;

namespace O24OpenAPI.Framework.Features.Commands;

[ApiEndpoint(
    ApiMethod.Post,
    "api/o24openapi-services/create",
    Tag = "O24OpenAPIService",
    MediatorKey = "fw"
)]
public class CreateServiceStepCommand : ICommand<O24OpenAPIService>
{
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
public class CreateServiceStepHandler(IO24OpenAPIMappingService o24OpenAPIMappingService)
    : ICommandHandler<CreateServiceStepCommand, O24OpenAPIService>
{
    public Task<O24OpenAPIService> HandleAsync(
        CreateServiceStepCommand request,
        CancellationToken cancellationToken = default
    )
    {
        O24OpenAPIService entity = request.ToO24OpenAPIService();
        return o24OpenAPIMappingService.AddAsync(entity);
    }
}
