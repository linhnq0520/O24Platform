using LinKit.Core.Cqrs;
using LinKit.Core.Endpoints;
using O24OpenAPI.Contracts.Constants;
using O24OpenAPI.Core.Configuration;
using O24OpenAPI.Framework.Exceptions;
using O24OpenAPI.Framework.Models;
using O24OpenAPI.Framework.Services.Configuration;

namespace O24OpenAPI.Framework.Features.Commands;

[ApiEndpoint(ApiMethod.Post, "api/settings/update", Tag = "Setting", MediatorKey = "fw")]
public class UpdateSettingCommand : SettingUpdateModel, ICommand<Setting> { }

[CqrsHandler]
public class UpdateSettingHandler(ISettingService settingService)
    : ICommandHandler<UpdateSettingCommand, Setting>
{
    public async Task<Setting> HandleAsync(
        UpdateSettingCommand request,
        CancellationToken cancellationToken = default
    )
    {
        var setting =
            await settingService.GetById(request.Id)
            ?? throw await O24Exception.CreateAsync(ResourceCode.Common.NotExists, request.Id);
        setting = request.ToSetting(setting);
        await settingService.UpdateSetting(setting);
        return setting;
    }
}
