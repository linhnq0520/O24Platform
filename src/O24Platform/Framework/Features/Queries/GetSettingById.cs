using LinKit.Core.Cqrs;
using LinKit.Core.Endpoints;
using O24OpenAPI.Framework.Models;
using O24OpenAPI.Framework.Services.Configuration;

namespace O24OpenAPI.Framework.Features.Queries;

public class GetSettingByIdQuery : ModelWithId, IQuery<GetSettingByIdResponse> { }

[ApiEndpoint(ApiMethod.Post, "api/settings/get", Tag = "Setting", MediatorKey = "fw")]
public class GetSettingByIdResponse : BaseO24OpenAPIModel
{
    public string Name { get; set; }
    public string Value { get; set; }
    public int OrganizationId { get; set; }
}

[CqrsHandler]
public class GetSettingByIdHandler(ISettingService settingService)
    : IQueryHandler<GetSettingByIdQuery, GetSettingByIdResponse>
{
    public async Task<GetSettingByIdResponse> HandleAsync(
        GetSettingByIdQuery request,
        CancellationToken cancellationToken = default
    )
    {
        var setting = await settingService.GetById(request.Id);
        var result = setting.ToGetSettingByIdResponse();
        return result;
    }
}
