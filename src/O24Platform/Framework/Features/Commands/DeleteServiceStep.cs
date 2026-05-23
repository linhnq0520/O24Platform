using LinKit.Core.Cqrs;
using LinKit.Core.Endpoints;
using O24OpenAPI.Contracts.Constants;
using O24OpenAPI.Framework.Domain;
using O24OpenAPI.Framework.Exceptions;
using O24OpenAPI.Framework.Models;
using O24OpenAPI.Framework.Services.Configuration;

namespace O24OpenAPI.Framework.Features.Commands;

[ApiEndpoint(
    ApiMethod.Post,
    "api/o24openapi-services/delete",
    Tag = "O24OpenAPIService",
    MediatorKey = "fw"
)]
public class DeleteServiceStepCommand : ModelWithId, ICommand<O24OpenAPIService> { }

[CqrsHandler]
public class DeleteServiceStepHandler(IO24OpenAPIMappingService o24OpenAPIMappingService)
    : ICommandHandler<DeleteServiceStepCommand, O24OpenAPIService>
{
    public async Task<O24OpenAPIService> HandleAsync(
        DeleteServiceStepCommand request,
        CancellationToken cancellationToken = default
    )
    {
        O24OpenAPIService entity =
            await o24OpenAPIMappingService.GetById(request.Id)
            ?? throw await O24Exception.CreateAsync(ResourceCode.Common.NotExists, request.Id);
        await o24OpenAPIMappingService.DeleteAsync(entity);
        return entity;
    }
}
