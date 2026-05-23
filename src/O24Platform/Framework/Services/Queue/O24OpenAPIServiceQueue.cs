using O24OpenAPI.Client.Scheme.Workflow;
using O24OpenAPI.Core.Infrastructure;
using O24OpenAPI.Framework.Domain;
using O24OpenAPI.Framework.Extensions;
using O24OpenAPI.Framework.Helpers;
using O24OpenAPI.Framework.Infrastructure.Mapper.Extensions;
using O24OpenAPI.Framework.Models;
using O24OpenAPI.Framework.Models.O24OpenAPI;
using O24OpenAPI.Framework.Services.Configuration;

namespace O24OpenAPI.Framework.Services.Queue;

/// <summary>
/// The 24 open api service queue class
/// </summary>
/// <seealso cref="BaseQueue"/>
public class O24OpenAPIServiceQueue : BaseQueue
{
    /// <summary>
    /// The io 24 open api mapping service
    /// </summary>
    private readonly IO24OpenAPIMappingService _mappingService =
        EngineContext.Current.Resolve<IO24OpenAPIMappingService>();

    /// <summary>
    /// Creates the w f scheme
    /// </summary>
    /// <param name="wFScheme">The scheme</param>
    /// <returns>A task containing the wf scheme</returns>
    public async Task<WFScheme> Create(WFScheme wFScheme)
    {
        var model = await wFScheme.ToModel<O24OpenAPIServiceCreateModel>();
        return await Invoke<O24OpenAPIServiceCreateModel>(
            wFScheme,
            async () =>
            {
                var entity = model.FromModel<O24OpenAPIService>();

                var response = await _mappingService.AddAsync(entity);
                return response;
            }
        );
    }

    public async Task<WFScheme> Update(WFScheme wFScheme)
    {
        var model = await wFScheme.ToModel<O24OpenAPIServiceUpdateModel>();
        return await Invoke<O24OpenAPIServiceUpdateModel>(
            wFScheme,
            async () =>
            {
                var entity = await _mappingService.GetById(model.Id);

                entity = model.ToEntity(entity);

                await _mappingService.UpdateAsync(entity);
                return entity;
            }
        );
    }

    public async Task<WFScheme> Delete(WFScheme wFScheme)
    {
        var model = await wFScheme.ToModel<ModelWithId>();
        return await Invoke<ModelWithId>(
            wFScheme,
            async () =>
            {
                var entity = await _mappingService.GetById(model.Id);
                await _mappingService.DeleteAsync(entity);
                return entity;
            }
        );
    }

    public async Task<WFScheme> SimpleSearch(WFScheme wFScheme)
    {
        var model = await wFScheme.ToModel<SimpleSearchModel>();
        return await Invoke<SimpleSearchModel>(
            wFScheme,
            async () =>
            {
                var entity = await _mappingService.SimpleSearch(model);
                return entity.ToPagedListModel<
                    O24OpenAPIService,
                    O24OpenAPIServiceSearchResponse
                >();
            }
        );
    }
}
