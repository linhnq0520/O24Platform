using System.Reflection;
using Linh.CodeEngine.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;
using O24OpenAPI.Client.EventBus.Extensions;
using O24OpenAPI.Client.EventBus.Submitters;
using O24OpenAPI.Client.Infisical;
using O24OpenAPI.Contracts.Configuration.Client;
using O24OpenAPI.Core.Configuration;
using O24OpenAPI.Core.Domain;
using O24OpenAPI.Core.Helper;
using O24OpenAPI.Core.Infrastructure;
using O24OpenAPI.Data.Configuration;
using O24OpenAPI.Framework.Services;
using O24OpenAPI.Logging.Extensions;
using StackExchange.Redis;

namespace O24OpenAPI.Framework.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static void ConfigureApplicationServices(
        this IServiceCollection services,
        WebApplicationBuilder builder
    )
    {
        services.AddScoped<WorkContext>();
        JObject appSettings;
        if (builder.Environment.IsDevelopment())
        {
            builder.Configuration.AddJsonFile(
                path: "App_Data/appsettings.json",
                optional: false,
                reloadOnChange: true
            );
            string json = File.ReadAllText("App_Data/appsettings.json");
            appSettings = JObject.Parse(json);
        }
        else
        {
            string currentAssembly = AppDomain.CurrentDomain.FriendlyName;
            Console.WriteLine("Getting secret value ...");
            appSettings = InfisicalManager.GetSecretByKey<JObject>(currentAssembly);
            Console.WriteLine("Getting secret value done");
        }
        if (appSettings == null)
        {
            throw new Exception("AppSettings could not be loaded");
        }

        CommonHelper.DefaultFileProvider = new O24OpenAPIFileProvider(builder.Environment);
        services.AddHttpContextAccessor();

        WebAppTypeFinder typeFinder = new();
        Singleton<ITypeFinder>.Instance = typeFinder;
        services.AddSingleton<ITypeFinder>(typeFinder);

        List<IConfig> lists = [];
        IEnumerable<Type> listConfigTypes = typeFinder.FindClassesOfType<IConfig>();
        List<IConfig> list =
        [
            .. listConfigTypes.Select(configType => (IConfig)Activator.CreateInstance(configType)),
        ];

        foreach (IConfig instance in list)
        {
            if (appSettings.TryGetValue(instance.Name, out JToken tokenSetting))
            {
                JObject jObject = tokenSetting is JObject settingObject
                    ? settingObject
                    : JObject.FromObject(tokenSetting);

                Type configType = instance.GetType();

                IConfig config = (IConfig)jObject.ToObject(configType);

                if (config != null)
                {
                    lists.Add(config);
                }
            }
        }

        AppSettings implementationInstance2 = AppSettingsHelper.SaveAppSettings(
            lists,
            CommonHelper.DefaultFileProvider,
            false
        );
        services.AddSingleton(implementationInstance2);
        Singleton<DataConfig>.Instance = implementationInstance2.Get<DataConfig>();
        EngineContext.Create().ConfigureServices(services, builder.Configuration);
        Singleton<O24OpenAPIClientConfiguration>.Instance = new(
            Singleton<O24OpenAPIConfiguration>.Instance
        );

        services.AddDistributedCache();
        builder.AddO24Logging();
        services.AddScoped<IStaticTokenService, StaticTokenService>();
        services.AddDynamicCodeEngineWithFileSystem(AppContext.BaseDirectory);
        services.AddKeyedSingleton<ILogSubmitter, RabbitMqLogSubmitter>("rabbitmq");
        services.AddLinKitCqrs("fw");
        Assembly assembly =
            AppDomain
                .CurrentDomain.GetAssemblies()
                .FirstOrDefault(a =>
                    !a.IsDynamic
                    && a.FullName?.StartsWith(
                        Singleton<O24OpenAPIConfiguration>.Instance.AssemblyMigration
                    ) == true
                )
            ?? throw new Exception(
                $"Assembly start with {Singleton<O24OpenAPIConfiguration>.Instance.AssemblyMigration} not found"
            );
        builder.AddRabbitMqEventBus().AddSubscriptionsFromAssemblies(assembly);
        builder.AddBackgroundJobs("StaticConfig/BackgroundJobsConfig.json");
        // builder.AddBackgroundJobs("BackgroundJobsConfig.json");
    }

    public static void AddHttpContextAccessor(this IServiceCollection services)
    {
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
    }

    private static void AddDistributedCache(this IServiceCollection services)
    {
        DistributedCacheConfig distributedCacheConfig =
            Singleton<AppSettings>.Instance.Get<DistributedCacheConfig>();

        if (!distributedCacheConfig.Enabled)
        {
            services.AddMemoryCache();
            return;
        }

        switch ((int)distributedCacheConfig.DistributedCacheType)
        {
            case 0:
                services.AddDistributedMemoryCache();
                break;
            case 1:
                services.AddDistributedSqlServerCache(options =>
                {
                    options.ConnectionString = distributedCacheConfig.ConnectionString;
                    options.SchemaName = distributedCacheConfig.SchemaName;
                    options.TableName = distributedCacheConfig.TableName;
                });
                break;
            case 2:
                services.AddStackExchangeRedisCache(distributedCacheConfig.ConnectionString);
                break;
        }
    }

    private static void AddStackExchangeRedisCache(
        this IServiceCollection services,
        string connectionString
    )
    {
        ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(
            new ConfigurationOptions { EndPoints = { connectionString } }
        );

        services.AddSingleton<IConnectionMultiplexer>(multiplexer);
    }

    // private static void RegisterInfisical(this IServiceCollection services)
    // {
    //     services.AddScoped<InfisicalClient>();
    // }
}
