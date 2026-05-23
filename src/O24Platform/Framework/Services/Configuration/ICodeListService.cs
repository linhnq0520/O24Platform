using O24OpenAPI.Core;
using O24OpenAPI.Framework.Domain;
using O24OpenAPI.Framework.Models;

namespace O24OpenAPI.Framework.Services.Configuration;

public interface ICodeListService
{
    Task<IPagedList<C_CODELIST>> GetByGroupAndName(CodeListGroupAndNameRequestModel model);
    Task<string> GetCaption(string codeId, string codeName, string codeGroup, string? language = null);
}
