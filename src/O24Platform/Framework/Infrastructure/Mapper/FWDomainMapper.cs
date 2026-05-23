using O24OpenAPI.Core.Configuration;
using O24OpenAPI.Core.Domain;
using O24OpenAPI.Framework.Domain;
using O24OpenAPI.Framework.Models;
using O24OpenAPI.Framework.Models.O24OpenAPI;

namespace O24OpenAPI.Framework.Infrastructure.Mapper;

/// <summary>
/// The fw domain mapper class
/// </summary>
/// <seealso cref="BaseMapperConfiguration"/>
public class FWDomainMapper : BaseMapperConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FWDomainMapper"/> class
    /// </summary>
    public FWDomainMapper()
    {
        CreateModelMap<O24OpenAPIService, O24OpenAPIServiceCreateModel>();
        CreateModelMap<O24OpenAPIService, O24OpenAPIServiceUpdateModel>();
        CreateModelMap<O24OpenAPIService, O24OpenAPIServiceSearchResponse>();
        CreateModelMap<Setting, SettingUpdateModel>();
        CreateModelMap<Setting, SettingCreateModel>();
        CreateModelMap<Setting, SettingSearchResponse>();
        CreateModelMap<Setting, SettingSearchModel>();
        CreateModelMap<StoredCommand, StoredCommandResponse>();
        CreateModelMap<C_CODELIST, CodeListResponseModel>();
        CreateModelMap<JobExecutionLog, JobExecutionHistorySearchResponse>();
    }
}
