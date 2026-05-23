using LinKit.Core.Mapping;
using O24OpenAPI.Core.Configuration;
using O24OpenAPI.Framework.Domain;
using O24OpenAPI.Framework.Features.Commands;
using O24OpenAPI.Framework.Features.Queries;
using O24OpenAPI.Framework.Models;

namespace O24OpenAPI.Framework.Mapping;

[MapperContext]
public class FrameworkMappingConfigurator : IMappingConfigurator
{
    public void Configure(IMapperConfigurationBuilder builder)
    {
        #region Setting
        builder.CreateMap<Setting, SettingSearchResponse>();
        builder.CreateMap<UpdateSettingCommand, Setting>();
        builder.CreateMap<Setting, GetSettingByIdResponse>();
        #endregion

        #region O24OpenAPIService
        builder.CreateMap<O24OpenAPIService, O24OpenAPIServiceSearchResponse>();
        builder.CreateMap<CreateServiceStepCommand, O24OpenAPIService>();
        builder.CreateMap<UpdateServiceStepCommand, O24OpenAPIService>();
        builder.CreateMap<O24OpenAPIService, GetServiceStepByIdResponse>();
        #endregion

        #region JobExecutionHistory
        builder.CreateMap<JobExecutionLog, JobExecutionHistorySearchResponse>();
        builder.CreateMap<JobExecutionLog, GetJobExecutionHistoryByIdResponse>();
        builder.CreateMap<CreateJobExecutionHistoryCommand, JobExecutionLog>();
        builder.CreateMap<UpdateJobExecutionHistoryCommand, JobExecutionLog>();
        #endregion
    }
}
