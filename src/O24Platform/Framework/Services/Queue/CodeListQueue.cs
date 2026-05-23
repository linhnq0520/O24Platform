using O24OpenAPI.Client.Scheme.Workflow;
using O24OpenAPI.Core.Infrastructure;
using O24OpenAPI.Framework.Domain;
using O24OpenAPI.Framework.Extensions;
using O24OpenAPI.Framework.Helpers;
using O24OpenAPI.Framework.Models;
using O24OpenAPI.Framework.Services.Configuration;

namespace O24OpenAPI.Framework.Services.Queue;

public class CodeListQueue : BaseQueue
{
    private readonly ICodeListService _codeListService =
        EngineContext.Current.Resolve<ICodeListService>();

    public async Task<WFScheme> GetByGroupAndName(WFScheme workflow)
    {
        var model = await workflow.ToModel<CodeListGroupAndNameRequestModel>();
        return await Invoke<CodeListGroupAndNameRequestModel>(
            workflow,
            async () =>
            {
                var value = await _codeListService.GetByGroupAndName(model);
                return value.ToPagedListModel<C_CODELIST, CodeListResponseModel>();
            }
        );
    }
}
