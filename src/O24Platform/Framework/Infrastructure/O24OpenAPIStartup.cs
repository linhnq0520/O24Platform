using LinKit.Core.BackgroundJobs;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using O24OpenAPI.Core.Caching;
using O24OpenAPI.Core.Configuration;
using O24OpenAPI.Core.Domain.Logging;
using O24OpenAPI.Core.Events;
using O24OpenAPI.Core.Extensions;
using O24OpenAPI.Core.Helper;
using O24OpenAPI.Core.Infrastructure;
using O24OpenAPI.Core.SeedWork;
using O24OpenAPI.Data;
using O24OpenAPI.Framework.DBContext;
using O24OpenAPI.Framework.Localization;
using O24OpenAPI.Framework.Services;
using O24OpenAPI.Framework.Services.CDC;
using O24OpenAPI.Framework.Services.Configuration;
using O24OpenAPI.Framework.Services.Events;
using O24OpenAPI.Framework.Services.Logging;
using O24OpenAPI.Framework.Services.Mapping;
using O24OpenAPI.Framework.Services.ScheduleTasks;
using O24OpenAPI.Framework.Services.Security;
using System.Net.Http;
using TaskScheduler = O24OpenAPI.Framework.Services.ScheduleTasks.TaskScheduler;

namespace O24OpenAPI.Framework.Infrastructure;

/// <summary>
/// The 24 open api startup class
/// </summary>
/// <seealso cref="IO24OpenAPIStartup"/>
internal class O24OpenAPIStartup : IO24OpenAPIStartup
{
    /// <summary>
    /// Gets the value of the order
    /// </summary>
    public int Order => 2000;

    /// <summary>
    /// Configures the application
    /// </summary>
    /// <param name="application">The application</param>
    public void Configure(IApplicationBuilder application) { }

    /// <summary>Add and configure any of the middleware</summary>
    /// <param name="services">Collection of service descriptors</param>
    /// <param name="configuration">Configuration of the application</param>
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ServiceDBContext>();
        services.AddScoped<IO24OpenAPIFileProvider, O24OpenAPIFileProvider>();
        services.AddScoped<IWebHelper, WebHelper>();
        services.AddTransient<IDataProviderManager, DataProviderManager>();
        services.AddTransient(serviceProvider =>
            serviceProvider.GetRequiredService<IDataProviderManager>().DataProvider
        );
        services.AddScoped<IDataConnectionFactory, DataConnectionFactory>();
        services.AddScoped(typeof(IRepository<>), typeof(EntityRepository<>));
        DistributedCacheConfig distributedCacheConfig =
            Singleton<AppSettings>.InstanceRequired.Get<DistributedCacheConfig>();
        if (distributedCacheConfig.Enabled)
        {
            services.AddScoped<ILocker, DistributedCacheManager>();
            services.AddScoped<IStaticCacheManager, DistributedCacheManager>();
        }
        else
        {
            services.AddSingleton<ILocker, MemoryCacheManager>();
            services.AddSingleton<IStaticCacheManager, MemoryCacheManager>();
        }
        services.AddScoped<IWorkContext, WebWorkContext>();
        services.AddScoped<ILocalizationService, LocalizationService>();
        services.AddScoped<ILanguageService, LanguageService>();
        services.AddScoped<ILogger, DefaultLogger>();
        services.AddSingleton<IEventPublisher, EventPublisher>();
        services.AddScoped<ISettingService, SettingService>();
        services.AddScoped<IScheduleTaskService, ScheduleTaskService>();
        ITypeFinder typeFinder = Singleton<ITypeFinder>.Instance;
        foreach (Type type in typeFinder.FindClassesOfType<ISettings>(false).ToList())
        {
            Type setting = type;
            services.AddScoped(
                setting,
                serviceProvider =>
                {
                    ISettingService service = serviceProvider.GetService<ISettingService>();
                    ISettings ob = service.LoadSetting(setting).GetAsyncResult();
                    return ob;
                }
            );
        }
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddSingleton<ITaskScheduler, TaskScheduler>();
        services.AddTransient<IScheduleTaskRunner, ScheduleTaskRunner>();
        foreach (
            Type implementationType in typeFinder.FindClassesOfType(typeof(IConsumer<>)).ToList()
        )
        {
            foreach (
                Type serviceType in implementationType.FindInterfaces(
                    (type, criteria) =>
                        type.IsGenericType
                        && ((Type)criteria).IsAssignableFrom(type.GetGenericTypeDefinition()),
                    typeof(IConsumer<>)
                )
            )
            {
                services.AddScoped(serviceType, implementationType);
            }
        }
        services.AddScoped<ISQLAuditLogService, SQLAuditLogService>();
        services.AddScoped<IExecuteQueryService, ExecuteQueryService>();
        services.AddScoped<IStoredCommandService, StoredCommandService>();
        services.AddScoped<IEncryptionService, EncryptionService>();
        services.AddScoped<IO24OpenAPIMappingService, O24OpenAPIMappingService>();
        services.AddScoped<IDataMapper, DataMapper>();
        //services.AddScoped<IEntityAuditService, EntityAuditService>();
        services.AddScoped<WorkContext>();
        services.AddScoped<ILoggerService, FileLoggerService>();
        services.AddKeyedScoped<ILoggerService, DbLoggerService>("Db");
        services.AddScoped<ICodeListService, CodeListService>();
        services.AddScoped<IStoreFunctionService, StoreFunctionService>();
        services.AddScoped<ITransactionActionService, TransactionActionService>();
        services.AddScoped<IMasterMappingService, MasterMappingService>();
        services.AddScoped<ICdcKeyConfigService, CdcKeyConfigService>();
        services
            .AddHttpClient(
                "DefaultClient",
                client =>
                {
                    client.DefaultRequestHeaders.Add("Accept", "application/json");
                }
            )
            .ConfigurePrimaryHttpMessageHandler(() =>
                new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (sender, cert, chain, errors) =>
                        true,
                }
            );

        services.AddLinKitDependency();
        if (Singleton<O24OpenAPIConfiguration>.InstanceRequired.JobExecutionHistoryEnabled)
        {
            services.AddScoped<IJobHistoryLogger, DbJobHistoryLogger>();
            services.AddScoped<IJobExecutionHistoryService, JobExecutionHistoryService>();
        }
    }
}
