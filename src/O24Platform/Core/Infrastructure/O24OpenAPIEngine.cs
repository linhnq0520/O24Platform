using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using O24OpenAPI.Core.Infrastructure.Mapper;
using System.Collections.Concurrent;
using System.Reflection;

namespace O24OpenAPI.Core.Infrastructure;

/// <summary>
/// The 24 open api engine class
/// </summary>
/// <seealso cref="IEngine"/>
public class O24OpenAPIEngine : IEngine
{
    public virtual IServiceProvider? ServiceProvider { get; protected set; }
    public IServiceScopeFactory? ServiceScopeFactory { get; protected set; }
    private static readonly ConcurrentDictionary<
        Type,
        Func<IServiceProvider, object>
    > _unregisteredFactoryCache = new();

    protected IServiceProvider? GetServiceProvider(IServiceScope? scope = null)
    {
        if (scope != null)
        {
            return scope.ServiceProvider;
        }

        IServiceProvider? serviceProvider = ServiceProvider;
        IHttpContextAccessor? httpContextAccessor =
            serviceProvider?.GetService<IHttpContextAccessor>();
        HttpContext? httpContext = httpContextAccessor?.HttpContext;

        if (httpContext?.RequestServices != null)
        {
            return httpContext.RequestServices;
        }

        try
        {
            if (AsyncScope.Scope?.ServiceProvider != null)
            {
                return AsyncScope.Scope.ServiceProvider;
            }
        }
        catch (ObjectDisposedException)
        {
            AsyncScope.Clear();
        }

        return serviceProvider;
    }

    public void ConfigureRequestPipeline(IApplicationBuilder application)
    {
        ServiceProvider = application.ApplicationServices;
        ServiceScopeFactory = ServiceProvider.GetRequiredService<IServiceScopeFactory>();

        ITypeFinder typeFinder =
            Resolve<ITypeFinder>(null)
            ?? throw new O24OpenAPIException("ITypeFinder not initialized");

        var startups = typeFinder
            .FindClassesOfType<IO24OpenAPIStartup>()
            .Select(type =>
                (IO24OpenAPIStartup)(
                    Activator.CreateInstance(type)
                    ?? throw new O24OpenAPIException($"Cannot create startup {type.FullName}")
                )
            )
            .OrderBy(startup => startup.Order)
            .ToList();

        foreach (IO24OpenAPIStartup? startup in startups)
        {
            startup.Configure(application);
        }
    }

    protected virtual void AddAutoMapper()
    {
        ITypeFinder typeFinder =
            Singleton<ITypeFinder>.Instance
            ?? throw new O24OpenAPIException(
                "Singleton<ITypeFinder> is not initialized when config auto mapper"
            );
        IOrderedEnumerable<IOrderedMapperProfile?> instances = typeFinder
            .FindClassesOfType<IOrderedMapperProfile>()
            .Select(type => Activator.CreateInstance(type) as IOrderedMapperProfile)
            .Where(profile => profile != null)!
            .OrderBy(profile => profile!.Order);

        AutoMapperConfiguration.Init(
            new MapperConfiguration(cfg =>
            {
                foreach (
                    IOrderedMapperProfile orderedMapperProfile in (IEnumerable<IOrderedMapperProfile>)
                        instances
                )
                {
                    cfg.AddProfile(orderedMapperProfile.GetType());
                }
            })
        );
    }

    private Assembly? CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
    {
        Assembly? assembly = AppDomain
            .CurrentDomain.GetAssemblies()
            .FirstOrDefault<Assembly>(a => a.FullName == args.Name);
        if (assembly != null)
        {
            return assembly;
        }

        ITypeFinder? instance = Singleton<ITypeFinder>.Instance;
        return instance?.GetAssemblies().FirstOrDefault<Assembly>(a => a.FullName == args.Name);
    }

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IEngine>(this);

        ITypeFinder typeFinder =
            Singleton<ITypeFinder>.Instance
            ?? throw new O24OpenAPIException("Singleton<ITypeFinder> is not initialized");

        var listStartUp = typeFinder
            .FindClassesOfType<IO24OpenAPIStartup>()
            .Select(type =>
                (IO24OpenAPIStartup)(
                    Activator.CreateInstance(type)
                    ?? throw new O24OpenAPIException($"Cannot create startup {type.FullName}")
                )
            )
            .OrderBy(startup => startup.Order)
            .ToList();

        foreach (IO24OpenAPIStartup? startup in listStartUp)
        {
            try
            {
                startup.ConfigureServices(services, configuration);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in {startup.GetType().Name}.ConfigureServices: {ex}");
                throw;
            }
        }

        AddAutoMapper();

        if (CurrentDomain_AssemblyResolve != null)
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve!;
        }
    }

    public IServiceScope CreateScope()
    {
        WorkContext? workContext = EngineContext.Current.Resolve<WorkContext>();
        IServiceScope scope =
            (ServiceScopeFactory?.CreateScope())
            ?? throw new O24OpenAPIException("Cannot create scope, ServiceScopeFactory is null");
        AsyncScope.Scope = scope;
        if (workContext != null)
        {
            WorkContext newWorkContext = scope.ServiceProvider.GetRequiredService<WorkContext>();
            newWorkContext.SetWorkContext(workContext);
            AsyncScope.WorkContext = newWorkContext;
        }

        return scope;
    }

    public T? Resolve<T>(IServiceScope? scope = null)
    {
        return (T?)Resolve(typeof(T), scope);
    }

    public object? Resolve(Type type, IServiceScope? scope = null)
    {
        try
        {
            return GetServiceProvider(scope)?.GetService(type);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to resolve type: {type.FullName}, Exception: {ex}");
            return null;
        }
    }

    public T? Resolve<T>(object keyed, IServiceScope? scope = null)
    {
        IServiceProvider serviceProvider =
            GetServiceProvider(scope)
            ?? throw new InvalidOperationException("Service provider is null");
        return keyed is null || string.IsNullOrWhiteSpace(keyed.ToString())
            ? serviceProvider.GetService<T>()
            : serviceProvider.GetKeyedService<T>(keyed);
    }

    public object ResolveRequired(Type type, object keyed, IServiceScope? scope = null)
    {
        IServiceProvider serviceProvider =
            GetServiceProvider(scope)
            ?? throw new InvalidOperationException("Service provider is null");
        return serviceProvider.GetRequiredKeyedService(type, keyed);
    }

    public IEnumerable<T>? ResolveAll<T>()
    {
        return (IEnumerable<T>?)GetServiceProvider()?.GetServices(typeof(T));
    }

    public virtual object? ResolveUnregistered(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        List<Exception> exceptions = [];
        IServiceProvider? serviceProvider = GetServiceProvider();

        foreach (
            ConstructorInfo constructor in type.GetConstructors()
                .OrderBy(c => c.GetParameters().Length)
        )
        {
            try
            {
                // Resolve tất cả parameter của constructor
                object[] parameters = constructor
                    .GetParameters()
                    .Select(param =>
                        serviceProvider?.GetService(param.ParameterType)
                        ?? Resolve(param.ParameterType, null)
                        ?? throw new InvalidOperationException(
                            $"Cannot resolve dependency: {param.ParameterType.FullName}"
                        )
                    )
                    .ToArray();

                return Activator.CreateInstance(type, parameters);
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        }

        throw new O24OpenAPIException(
            $"No constructor found for {type.FullName}. Errors: {string.Join("; ", exceptions.Select(e => e.Message))}",
            exceptions.FirstOrDefault()
        );
    }

    public virtual object ResolveTypeInstance(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        Func<IServiceProvider, object> factory = _unregisteredFactoryCache.GetOrAdd(
            type,
            CreateFactory
        );

        try
        {
            IServiceProvider? serviceProvider = GetServiceProvider();
            return serviceProvider is null
                ? throw new O24OpenAPIException("O24OpenAPIEngine ServiceProvider is null.")
                : factory(serviceProvider);
        }
        catch (Exception ex)
        {
            throw new O24OpenAPIException($"Failed to resolve type: {type.FullName}", ex);
        }
    }

    private Func<IServiceProvider, object> CreateFactory(Type type)
    {
        if (type.IsAbstract || type.IsInterface)
        {
            throw new InvalidOperationException(
                $"Cannot create instance of abstract/interface: {type.FullName}"
            );
        }

        ConstructorInfo? defaultCtor = type.GetConstructor(Type.EmptyTypes);
        if (defaultCtor != null)
        {
            return _ => Activator.CreateInstance(type)!;
        }

        ConstructorInfo[] constructors = type.GetConstructors();
        ConstructorInfo? constructor = constructors
            .OrderByDescending(c =>
                c.IsDefined(typeof(ActivatorUtilitiesConstructorAttribute), true) ? 1 : 0
            )
            .ThenByDescending(c => c.GetParameters().Length)
            .FirstOrDefault();

        if (constructor == null)
        {
            throw new InvalidOperationException($"No public constructor found for {type.FullName}");
        }

        ParameterInfo[] parameters = constructor.GetParameters();

        return serviceProvider =>
        {
            object[] args = parameters
                .Select(p =>
                {
                    object? service = serviceProvider?.GetService(p.ParameterType);
                    if (service != null)
                    {
                        return service;
                    }

                    return Resolve(p.ParameterType, null)
                        ?? throw new InvalidOperationException(
                            $"Cannot resolve dependency: {p.ParameterType.FullName}"
                        );
                })
                .ToArray();

            return constructor.Invoke(args);
        };
    }

    public virtual object? ResolveInterfaceUnregistered(Type type)
    {
        Exception? innerException = null;
        Type? implementingClasses =
            type.Assembly.GetExportedTypes()
                .FirstOrDefault(t => type.IsAssignableFrom(t) && t.IsClass)
            ?? throw new O24OpenAPIException(
                "No constructor was found for " + type.FullName + ".",
                innerException
            );
        foreach (ConstructorInfo constructor in implementingClasses.GetConstructors())
        {
            try
            {
                IEnumerable<object> source = constructor
                    .GetParameters()
                    .Select(parameter =>
                    {
                        return Resolve(parameter.ParameterType, null)
                            ?? throw new O24OpenAPIException("Unknown dependency");
                    });
                return Activator.CreateInstance(implementingClasses, source.ToArray<object>());
            }
            catch (Exception ex)
            {
                innerException = ex;
            }
        }

        throw new O24OpenAPIException(
            "No constructor was found for " + type.FullName + ".",
            innerException
        );
    }

    public IServiceScope CreateQueueScope(WorkContext workContext)
    {
        IServiceScope? scope =
            (ServiceScopeFactory?.CreateScope())
            ?? throw new O24OpenAPIException(
                "Cannot create queue scope, ServiceScopeFactory is null"
            );
        WorkContext newWorkContext = scope.ServiceProvider.GetRequiredService<WorkContext>();
        newWorkContext.SetWorkContext(workContext);
        AsyncScope.Scope = scope;
        AsyncScope.WorkContext = newWorkContext;
        return scope;
    }

    public T ResolveRequired<T>(object? keyed = null, IServiceScope? scope = null)
    {
        IServiceProvider serviceProvider =
            GetServiceProvider(scope)
            ?? throw new InvalidOperationException("Service provider is null");
        if (keyed is not null)
        {
            return (T)serviceProvider.GetRequiredKeyedService(typeof(T), keyed);
        }
        return (T)serviceProvider.GetRequiredService(typeof(T));
    }
}
