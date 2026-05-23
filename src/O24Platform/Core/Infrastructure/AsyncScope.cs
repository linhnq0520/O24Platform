using Microsoft.Extensions.DependencyInjection;

namespace O24OpenAPI.Core.Infrastructure;

public class AsyncScope
{
    private static readonly AsyncLocal<IServiceScope> _scope = new();
    private static readonly AsyncLocal<WorkContext> _workContext = new();

    public static IServiceScope Scope
    {
        get => _scope.Value;
        set => _scope.Value = value;
    }

    public static WorkContext WorkContext
    {
        get => _workContext.Value;
        set => _workContext.Value = value;
    }

    public static IServiceProvider ServiceProvider => Scope.ServiceProvider;

    public static void Clear()
    {
        _scope.Value?.Dispose();
        _scope.Value = null;
        _workContext.Value = null;
    }
}
