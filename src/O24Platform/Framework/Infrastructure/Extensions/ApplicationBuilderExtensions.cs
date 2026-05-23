using System.Reflection;
using Linh.CodeEngine.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using O24OpenAPI.Core.Attributes;
using O24OpenAPI.Core.Configuration;
using O24OpenAPI.Core.Extensions;
using O24OpenAPI.Core.Infrastructure;
using O24OpenAPI.Data;
using O24OpenAPI.Data.Configuration;
using O24OpenAPI.Data.Migrations;
using O24OpenAPI.Framework.Services.Logging;
using O24OpenAPI.Logging.Extensions;

namespace O24OpenAPI.Framework.Infrastructure.Extensions;

/// <summary>
/// The application builder extensions class
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Creates the database
    /// </summary>
    /// <exception cref="Exception">ConnectionStrings is null</exception>
    /// <exception cref="Exception">DefaultConnection is null</exception>
    private static async Task CreateDatabaseAsync()
    {
        var appSettings = Singleton<AppSettings>.Instance;
        var dataConfig =
            appSettings.Get<DataConfig>() ?? throw new Exception("ConnectionStrings is null");

        if (string.IsNullOrEmpty(dataConfig.DefaultConnection))
        {
            throw new Exception("DefaultConnection is null");
        }

        var config = Singleton<O24OpenAPIConfiguration>.Instance;

        var connectionString = dataConfig.DefaultConnection;
        var providerType = dataConfig.DataProvider;
        var nameToCheck =
            providerType == DataProviderType.SqlServer ? config.YourDatabase : config.YourSchema;

        if (await SqlExecutor.DatabaseExistsAsync(connectionString, nameToCheck, providerType))
        {
            return;
        }

        var scriptPath = providerType switch
        {
            DataProviderType.SqlServer => config.CreateDatabaseScriptPath,
            DataProviderType.Oracle => config.CreateSchemaScriptPath,
            _ => throw new NotSupportedException($"Provider {providerType} is not supported"),
        };

        await SqlExecutor.ExecuteSqlFromFileAsync(connectionString, scriptPath, providerType);
    }

    /// <summary>
    /// Enables the cdc
    /// </summary>
    /// <exception cref="Exception">ConnectionString is null</exception>
    /// <exception cref="Exception">ConnectionStrings is null</exception>
    private static async Task EnableCDCAsync()
    {
        var config = Singleton<O24OpenAPIConfiguration>.Instance;
        if (string.IsNullOrEmpty(config.YourCDCSchema))
        {
            return;
        }
        var appSettings = Singleton<AppSettings>.Instance;
        var dataConfig =
            appSettings.Get<DataConfig>() ?? throw new Exception("ConnectionStrings is null");
        string connectionString = dataConfig.DefaultConnection;
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new Exception("ConnectionString is null");
        }
        if (await SqlExecutor.CDCEnableAsync(connectionString, config.YourDatabase))
        {
            return;
        }
        string enableDbCdc =
            $@"Use {Singleton<O24OpenAPIConfiguration>.Instance.YourDatabase};
                EXEC sys.sp_cdc_enable_db;";

        await SqlExecutor.ExecuteSqlScriptAsync(enableDbCdc, connectionString);
    }

    /// <summary>
    /// Configures the request pipeline using the specified application
    /// </summary>
    /// <param name="application">The application</param>
    public static void ConfigureRequestPipeline(this IApplicationBuilder application)
    {
        CreateDatabaseAsync().Wait();
        //EnableCDCAsync().Wait();
        application.UseO24Logging();
        EngineContext.Current.ConfigureRequestPipeline(application);
        application.ConfigureCodeEngine();
    }

    /// <summary>
    /// Starts the engine using the specified application
    /// </summary>
    /// <param name="application">The application</param>
    public static async Task StartEngine(this WebApplication application)
    {
        IEngine current = EngineContext.Current;
        if (!DataSettingsManager.IsDatabaseInstalled())
        {
            return;
        }

        using var scope = application.Services.CreateScope();
        AsyncScope.Scope = scope;

        var o24Config = Singleton<O24OpenAPIConfiguration>.Instance;
        if (o24Config.RunMigration)
        {
            IMigrationManager migrationManager = current.Resolve<IMigrationManager>();
            var environment =
                Environment.GetEnvironmentVariable("O24OPENAPI_ENVIRONMENT")
                ?? Singleton<O24OpenAPIConfiguration>.Instance.Environment;
            var environmentType = environment.ToEnum<EnvironmentType>();

            Console.WriteLine(
                "=====================Applying framework data migration=================="
            );
            ApplyMigration("O24OpenAPI.Framework", migrationManager, environmentType);

            Console.WriteLine(
                "(=====================Applying data engine migration=================="
            );
            var assemblies1 = AppDomain
                .CurrentDomain.GetAssemblies()
                .Where(ass => ass.GetName().Name == "O24OpenAPI.Data")
                .ToList();

            foreach (Assembly assembly in assemblies1)
            {
                migrationManager.ApplyUpMigrations(
                    assembly,
                    MigrationProcessType.NoMatter,
                    environmentType
                );
            }

            Console.WriteLine(
                "=====================Applying service data migration====================="
            );

            if (!string.IsNullOrEmpty(o24Config.AssemblyMigration))
            {
                var assemblies2 = AppDomain
                    .CurrentDomain.GetAssemblies()
                    .Where(ass =>
                        ass.GetName().Name.StartsWithOrdinalIgnoreCase(o24Config.AssemblyMigration)
                    )
                    .ToList();
                foreach (Assembly assembly in assemblies2)
                {
                    migrationManager.ApplyUpMigrations(
                        assembly,
                        MigrationProcessType.NoMatter,
                        environmentType
                    );
                }
            }
        }
        application.InitializeMessageQueue();
        await current.Resolve<ILogger>().Information("Application started");
        // application.MapGeneratedEndpoints();
    }

    /// <summary>
    /// Applies the migration using the specified assembly name
    /// </summary>
    /// <param name="assemblyName">The assembly name</param>
    /// <param name="migrationManager">The migration manager</param>
    /// <param name="environmentType">The environment type</param>
    private static void ApplyMigration(
        string assemblyName,
        IMigrationManager migrationManager,
        EnvironmentType environmentType
    )
    {
        if (string.IsNullOrEmpty(assemblyName))
        {
            return;
        }
        IEnumerable<Assembly> assemblies = AppDomain
            .CurrentDomain.GetAssemblies()
            .Where(ass =>
                ass.FullName.StartsWith(assemblyName, StringComparison.InvariantCultureIgnoreCase)
            );

        foreach (Assembly assembly in assemblies)
        {
            migrationManager.ApplyUpMigrations(
                assembly,
                MigrationProcessType.NoMatter,
                environmentType
            );
        }
    }
}
