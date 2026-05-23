using LinKit.Core.Cqrs;
using LinKit.Core.Endpoints;
using O24OpenAPI.Core.Configuration;
using O24OpenAPI.Framework.Models;
using O24OpenAPI.Framework.Services.Configuration;

namespace O24OpenAPI.Framework.Features.Commands;

[ApiEndpoint(ApiMethod.Post, "api/settings/delete", Tag = "Setting", MediatorKey = "fw")]
public class DeleteSettingCommand : ModelWithId, ICommand<Setting> { }

[CqrsHandler]
public class DeleteSettingHandler(ISettingService settingService)
    : ICommandHandler<DeleteSettingCommand, Setting>
{
    public Task<Setting> HandleAsync(
        DeleteSettingCommand request,
        CancellationToken cancellationToken = default
    )
    {
        return settingService.Delete(request.Id);
    }
}
