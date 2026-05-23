using System.Collections.Concurrent;
using System.Linq.Expressions;
using O24OpenAPI.Client.Scheme.Workflow;
using O24OpenAPI.Core.Helper;
using O24OpenAPI.Core.Infrastructure;

namespace O24OpenAPI.Framework.Helpers;

public static class WorkflowHelper
{
    private static readonly ConcurrentDictionary<
        (string ClassName, string MethodName),
        Func<object, WFScheme, Task<WFScheme>>
    > _delegateCache = new();

    private static readonly ConcurrentDictionary<string, Type> _typeCache = new();

    public static async Task<WFScheme> InvokeAsync(
        this WFScheme input,
        string fullClassName,
        string methodName
    )
    {
        if (string.IsNullOrWhiteSpace(fullClassName))
        {
            throw new ArgumentNullException(nameof(fullClassName));
        }

        if (string.IsNullOrWhiteSpace(methodName))
        {
            throw new ArgumentNullException(nameof(methodName));
        }

        var compiledDelegate = _delegateCache.GetOrAdd(
            (fullClassName, methodName),
            key =>
            {
                var type = _typeCache.GetOrAdd(
                    key.ClassName,
                    className =>
                    {
                        string assemblyName = O24Helper.GetO24AssemblyName(className);
                        return Type.GetType($"{className}, {assemblyName}")
                            ?? throw new InvalidOperationException(
                                $"Cannot find class: {className} in assembly {assemblyName}"
                            );
                    }
                );

                var methodInfo =
                    type.GetMethod(key.MethodName)
                    ?? throw new MissingMethodException(
                        $"Method '{key.MethodName}' not found in '{key.ClassName}'"
                    );

                var parameters = methodInfo.GetParameters();
                if (parameters.Length != 1 || parameters[0].ParameterType != typeof(WFScheme))
                {
                    throw new InvalidOperationException(
                        $"Method '{key.MethodName}' must have exactly one parameter of type WFScheme."
                    );
                }

                if (!typeof(Task<WFScheme>).IsAssignableFrom(methodInfo.ReturnType))
                {
                    throw new InvalidOperationException(
                        $"Method '{key.MethodName}' must return Task<WFScheme>."
                    );
                }

                // Build delegate: (object instance, WFScheme input) => instance.Method(input)
                var instanceParam = Expression.Parameter(typeof(object), "instance");
                var inputParam = Expression.Parameter(typeof(WFScheme), "input");

                var castInstance = Expression.Convert(instanceParam, type);
                var callMethod = Expression.Call(castInstance, methodInfo, inputParam);

                var lambda = Expression.Lambda<Func<object, WFScheme, Task<WFScheme>>>(
                    callMethod,
                    instanceParam,
                    inputParam
                );

                return lambda.Compile();
            }
        );

        var targetType = _typeCache[fullClassName];
        var instance =
            EngineContext.Current.ResolveTypeInstance(targetType)
            ?? throw new InvalidOperationException(
                $"Cannot create instance of type '{fullClassName}'"
            );

        return await compiledDelegate(instance, input).ConfigureAwait(false);
    }
}
