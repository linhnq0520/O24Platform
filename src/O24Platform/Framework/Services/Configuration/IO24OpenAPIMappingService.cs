using O24OpenAPI.Core;
using O24OpenAPI.Framework.Domain;
using O24OpenAPI.Framework.Models;

namespace O24OpenAPI.Framework.Services.Configuration;

public interface IO24OpenAPIMappingService
{
    Task<O24OpenAPIService> GetById(int id);
    Task<IList<O24OpenAPIService>> GetAll();
    Task<O24OpenAPIService> AddAsync(O24OpenAPIService o24OpenAPIService);
    Task UpdateAsync(O24OpenAPIService o24OpenAPIService);
    Task DeleteAsync(O24OpenAPIService o24OpenAPIService);
    Task<IPagedList<O24OpenAPIService>> SimpleSearch(SimpleSearchModel model);
    Task<O24OpenAPIService> GetByStepCode(string stepCode);
    Task<IList<O24OpenAPIService>> GetByStepCodes(string[] stepCodes);
}
