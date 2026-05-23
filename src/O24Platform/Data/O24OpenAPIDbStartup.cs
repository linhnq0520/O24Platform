using System.Reflection;
using FluentMigrator;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Conventions;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using O24OpenAPI.Core.Infrastructure;
using O24OpenAPI.Data.Mapping;
using O24OpenAPI.Data.Migrations;

namespace O24OpenAPI.Data;

/// <summary>
/// The 24 open api db startup class
/// </summary>
/// <seealso cref="IO24OpenAPIStartup"/>
internal class O24OpenAPIDbStartup : IO24OpenAPIStartup
{
    /// <summary>
    /// Configures the services using the specified services
    /// </summary>
    /// <param name="services">The services</param>
    /// <param name="configuration">The configuration</param>
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        Assembly[] mAssemblies = Singleton<ITypeFinder>
            .Instance.FindClassesOfType<MigrationBase>(true)
            .Select(t => t.Assembly)
            .Where(assembly => !assembly?.FullName?.Contains("FluentMigrator.Runner") ?? false)
            .Distinct()
            .ToArray();

        services
            .AddFluentMigratorCore()
            .AddScoped<IProcessorAccessor, O24OpenAPIProcessorAccessor>()
            .AddScoped<IConnectionStringAccessor>(x => DataSettingsManager.LoadSettings())
            .AddScoped<IMigrationManager, MigrationManager>()
            .AddSingleton<IConventionSet, O24OpenAPIConventionSet>()
            .AddTransient<IMappingEntityAccessor>(x =>
                x.GetRequiredService<IDataProviderManager>().DataProvider
            )
            .ConfigureRunner(rb =>
                rb.WithVersionTable(new MigrationVersionInfo())
                    .AddSqlServer()
                    .AddMySql5()
                    .AddPostgres()
                    .AddOracle12CManaged()
                    .ScanIn(mAssemblies)
                    .For.Migrations()
            );
    }

    /// <summary>
    /// Configures the application
    /// </summary>
    /// <param name="application">The application</param>
    public void Configure(IApplicationBuilder application) { }

    /// <summary>
    /// Gets the value of the order
    /// </summary>
    public int Order => 10;
}
