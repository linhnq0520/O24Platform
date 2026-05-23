using O24OpenAPI.Core.Attributes;
using O24OpenAPI.Data.Migrations;
using O24OpenAPI.Framework.Domain;
using O24OpenAPI.Framework.Services.Queue;

namespace O24OpenAPI.Framework.Migrations;

/// <summary>
/// The data 24 migration class
/// </summary>
/// <seealso cref="BaseMigration"/>
[O24OpenAPIMigration(
    "2024/01/01 03:00:10:0000000",
    "3. Generate O24OpenAPIService data",
    MigrationProcessType.Installation
)]
[Environment([EnvironmentType.All])]
public class DataO24Migration : BaseMigration
{
    /// <summary>
    /// Ups this instance
    /// </summary>
    public override void Up()
    {
        var list = new List<O24OpenAPIService>
        {
            new()
            {
                StepCode = "CREATE_O24SERVICE",
                FullClassName = $"{typeof(O24OpenAPIServiceQueue).FullName}",
                MethodName = nameof(O24OpenAPIServiceQueue.Create),
                ShouldAwait = true,
                IsInquiry = true,
            },
            new()
            {
                StepCode = "UPDATE_O24SERVICE",
                FullClassName = $"{typeof(O24OpenAPIServiceQueue).FullName}",
                MethodName = nameof(O24OpenAPIServiceQueue.Update),
                ShouldAwait = true,
                IsInquiry = true,
            },
            new()
            {
                StepCode = "DELETE_O24SERVICE",
                FullClassName = $"{typeof(O24OpenAPIServiceQueue).FullName}",
                MethodName = nameof(O24OpenAPIServiceQueue.Delete),
                ShouldAwait = true,
                IsInquiry = true,
            },
            new()
            {
                StepCode = "SEARCH_O24SERVICE",
                FullClassName = $"{typeof(O24OpenAPIServiceQueue).FullName}",
                MethodName = nameof(O24OpenAPIServiceQueue.SimpleSearch),
                ShouldAwait = true,
                IsInquiry = true,
            },
            //Setting
            new()
            {
                StepCode = "CREATE_SETTING",
                FullClassName = $"{typeof(SettingQueue).FullName}",
                MethodName = nameof(SettingQueue.Create),
                ShouldAwait = true,
                IsInquiry = true,
            },
            new()
            {
                StepCode = "UPDATE_SETTING",
                FullClassName = $"{typeof(SettingQueue).FullName}",
                MethodName = nameof(SettingQueue.Update),
                ShouldAwait = true,
                IsInquiry = true,
            },
            new()
            {
                StepCode = "DELETE_SETTING",
                FullClassName = $"{typeof(SettingQueue).FullName}",
                MethodName = nameof(SettingQueue.Delete),
                ShouldAwait = true,
                IsInquiry = true,
            },
            new()
            {
                StepCode = "SEARCH_SETTING",
                FullClassName = $"{typeof(SettingQueue).FullName}",
                MethodName = nameof(SettingQueue.SimpleSearch),
                ShouldAwait = true,
                IsInquiry = true,
            },
            new()
            {
                StepCode = "GET_FULL_CLASS_NAME",
                FullClassName = $"{typeof(O24OpenAPIQueue).FullName}",
                MethodName = nameof(O24OpenAPIQueue.GetListQueueName),
                ShouldAwait = true,
                IsInquiry = true,
            },
        };
        SeedListData(list, [nameof(O24OpenAPIService.StepCode)]).Wait();
    }
}
