using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace O24OpenAPI.Core.Infrastructure;

/// <summary>
/// The io 24 open api startup interface
/// </summary>
public interface IO24OpenAPIStartup
{
    /// <summary>
    /// Configures the services using the specified services
    /// </summary>
    /// <param name="services">The services</param>
    /// <param name="configuration">The configuration</param>
    void ConfigureServices(IServiceCollection services, IConfiguration configuration);

    /// <summary>
    /// Configures the application
    /// </summary>
    /// <param name="application">The application</param>
    void Configure(IApplicationBuilder application);

    /// <summary>
    /// Gets the value of the order
    /// </summary>
    int Order { get; }
}
