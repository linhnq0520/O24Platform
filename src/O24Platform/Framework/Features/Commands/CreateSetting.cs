using LinKit.Core.Cqrs;
using LinKit.Core.Endpoints;
using O24OpenAPI.Core.Configuration;
using O24OpenAPI.Framework.Models;
using O24OpenAPI.Framework.Services.Configuration;

namespace O24OpenAPI.Framework.Features.Commands;

[ApiEndpoint(ApiMethod.Post, "api/settings/create", Tag = "Setting", MediatorKey = "fw")]
public class CreateSettingCommand : SettingCreateModel, ICommand<Setting> { }

[CqrsHandler]
public class CreateSettingHandler(ISettingService settingService)
    : ICommandHandler<CreateSettingCommand, Setting>
{
    public Task<Setting> HandleAsync(
        CreateSettingCommand request,
        CancellationToken cancellationToken = default
    )
    {
        return settingService.Create(request);
    }
}
