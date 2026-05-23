using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace O24OpenAPI.Core.Infrastructure;

/// <summary>
/// The engine interface
/// </summary>
public interface IEngine
{
    IServiceProvider? ServiceProvider { get; }
    IServiceScopeFactory? ServiceScopeFactory { get; }
    void ConfigureServices(IServiceCollection services, IConfiguration configuration);
    void ConfigureRequestPipeline(IApplicationBuilder application);
    T? Resolve<T>(IServiceScope? scope = null);
    object? Resolve(Type type, IServiceScope? scope = null);
    IEnumerable<T>? ResolveAll<T>();
    object? ResolveUnregistered(Type type);
    object? ResolveInterfaceUnregistered(Type type);
    IServiceScope? CreateScope();
    T? Resolve<T>(object keyed, IServiceScope? scope = null);
    object? ResolveRequired(Type type, object keyed, IServiceScope? scope = null);
    T ResolveRequired<T>(object? keyed = null, IServiceScope? scope = null);
    object ResolveTypeInstance(Type type);
    IServiceScope CreateQueueScope(WorkContext workContext);
}
