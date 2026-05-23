using O24OpenAPI.Client.Scheme.Workflow;
using O24OpenAPI.Core.Infrastructure;
using O24OpenAPI.Data.System.Linq;
using O24OpenAPI.Framework.Extensions;
using O24OpenAPI.Framework.Helpers;
using O24OpenAPI.Framework.Models.O24OpenAPI;

namespace O24OpenAPI.Framework.Services.Queue;

public class O24OpenAPIQueue : BaseQueue
{
    private readonly ITypeFinder _typeFinder = EngineContext.Current.Resolve<ITypeFinder>();

    public async Task<WFScheme> GetListQueueName(WFScheme wfScheme)
    {
        var model = await wfScheme.ToModel<GetListQueueNameRequest>();
        return await Invoke<GetListQueueNameRequest>(
            wfScheme,
            async () =>
            {
                var list = new List<GetQueueNameResponse>();
                var listType = _typeFinder.FindClassesOfType<BaseQueue>();
                foreach (var type in listType)
                {
                    var fullClassName = type.FullName;
                    if (fullClassName != null && fullClassName.Contains(model.AssemblyName))
                    {
                        list.Add(new GetQueueNameResponse { FullClassName = fullClassName });
                    }
                }
                await Task.CompletedTask;
                var pageList = await list.AsQueryable()
                    .ToPagedList(model.PageIndex, model.PageSize);
                return pageList.ToPagedListModel<GetQueueNameResponse, GetQueueNameResponse>();
            }
        );
    }

    public async Task<WFScheme> GetListMethod(WFScheme wfScheme)
    {
        var model = await wfScheme.ToModel<GetListMethodRequest>();
        return await Invoke<GetListMethodRequest>(
            wfScheme,
            async () =>
            {
                await Task.CompletedTask;
                var listModel = new List<MethodResponse>();
                var typeName = $"{model.FullClassName}, {model.AssemblyName}";
                var type = Type.GetType(typeName);
                if (type != null)
                {
                    var methods = type.GetMethods()
                        .Where(m => m.IsPublic && !m.IsStatic)
                        .Select(m => new MethodResponse { MethodName = m.Name })
                        .ToList();
                    listModel.AddRange(methods);
                }
                var pageList = await listModel
                    .AsQueryable()
                    .ToPagedList(model.PageIndex, model.PageSize);
                var response = pageList.ToPagedListModel<MethodResponse, MethodResponse>();
                return response;
            }
        );
    }
}
