using O24OpenAPI.Client.Scheme.Workflow;
using O24OpenAPI.Core.Configuration;
using O24OpenAPI.Core.Infrastructure;
using O24OpenAPI.Framework.Extensions;
using O24OpenAPI.Framework.Helpers;
using O24OpenAPI.Framework.Models;
using O24OpenAPI.Framework.Services.Configuration;

namespace O24OpenAPI.Framework.Services.Queue;

public class SettingQueue : BaseQueue
{
    private readonly ISettingService _service = EngineContext.Current.Resolve<ISettingService>();

    /// <summary>
    /// Create
    /// </summary>
    /// /// <param name="workflow"></param>
    /// <returns></returns>
    public async Task<WFScheme> Create(WFScheme workflow)
    {
        var model = await workflow.ToModel<SettingCreateModel>();
        return await Invoke<SettingCreateModel>(
            workflow,
            async () =>
            {
                var value = await _service.Create(model);
                return value;
            }
        );
    }

    /// <summary>
    /// Update
    /// </summary>
    /// <param name="workflow"></param>
    /// <returns></returns>
    public async Task<WFScheme> Update(WFScheme workflow)
    {
        var model = await workflow.ToModel<SettingUpdateModel>();
        return await Invoke<SettingUpdateModel>(
            workflow,
            async () =>
            {
                var rs = await _service.Update(model, model.ReferenceId);
                return rs;
            }
        );
    }

    /// <summary>
    /// Delete
    /// </summary>
    /// <param name="workflow"></param>
    /// <returns></returns>
    public async Task<WFScheme> Delete(WFScheme workflow)
    {
        var model = await workflow.ToModel<ModelWithId>();
        return await Invoke<ModelWithId>(
            workflow,
            async () =>
            {
                var rs = await _service.Delete(model.Id, model.ReferenceId);
                return rs;
            }
        );
    }

    /// <summary>
    /// View
    /// </summary>
    /// <param name="workflow"></param>
    /// <returns></returns>View
    public async Task<WFScheme> View(WFScheme workflow)
    {
        var model = await workflow.ToModel<ModelWithId>();
        return await Invoke<ModelWithId>(
            workflow,
            async () =>
            {
                var group = await _service.View(model.Id);
                return group;
            }
        );
    }

    /// <summary>
    /// SimpleSearch
    /// </summary>
    /// <param name="workflow"></param>
    /// <returns></returns>
    public async Task<WFScheme> SimpleSearch(WFScheme workflow)
    {
        var model = await workflow.ToModel<SimpleSearchModel>();

        return await Invoke<SimpleSearchModel>(
            workflow,
            async () =>
            {
                var value = await _service.Search(model);
                return value.ToPagedListModel<Setting, SettingSearchResponse>();
            }
        );
    }

    /// <summary>
    /// AdvanceSearch
    /// </summary>
    /// <param name="workflow"></param>
    /// <returns></returns>
    public async Task<WFScheme> AdvanceSearch(WFScheme workflow)
    {
        var model = await workflow.ToModel<SettingSearchModel>();
        return await Invoke<SettingSearchModel>(
            workflow,
            async () =>
            {
                var value = await _service.Search(model);
                return value.ToPagedListModel<Setting, SettingSearchResponse>();
            }
        );
    }

    /// <summary>
    /// View
    /// </summary>
    /// <param name="workflow"></param>
    /// <returns></returns>View
    public async Task<WFScheme> GetByName(WFScheme workflow)
    {
        var model = await workflow.ToModel<SettingViewByPrimaryKey>();
        return await Invoke<SettingViewByPrimaryKey>(
            workflow,
            async () =>
            {
                var setting = await _service.GetByPrimaryKey(model.Name);
                return setting;
            }
        );
    }
}
