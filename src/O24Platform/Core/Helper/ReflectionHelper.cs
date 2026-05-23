using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using O24OpenAPI.Core.Infrastructure;

namespace O24OpenAPI.Core.Helper;

/// <summary>
/// The reflection helper class
/// </summary>
public class ReflectionHelper
{
    /// <summary>
    /// The type cache
    /// </summary>
    private static readonly ConcurrentDictionary<string, Type> _typeCache = new();

    /// <summary>
    /// The delegate cache
    /// </summary>
    private static readonly ConcurrentDictionary<
        (string ClassName, string MethodName),
        Delegate
    > _delegateCache = new();

    /// <summary>
    /// Dynamics the invoke async 1 using the specified full class name
    /// </summary>
    /// <typeparam name="T">The </typeparam>
    /// <param name="fullClassName">The full class name</param>
    /// <param name="methodName">The method name</param>
    /// <param name="parameters">The parameters</param>
    /// <exception cref="ArgumentNullException">Cannot create an instance of [{fullClassName}]</exception>
    /// <exception cref="ArgumentNullException">Cannot find Method with name [{key.MethodName}] in class [{key.ClassName}]</exception>
    /// <exception cref="ArgumentNullException">Cannot find class with Name=[{className}] in assembly [{assemblyName}]</exception>
    /// <returns>A task containing the</returns>
    public static async Task<T> DynamicInvokeAsync1<T>(
        string fullClassName,
        string methodName,
        object[] parameters
    )
    {
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
                            ?? throw new ArgumentNullException(
                                $"Cannot find class with Name=[{className}] in assembly [{assemblyName}]"
                            );
                    }
                );

                var methodInfo =
                    type.GetMethod(key.MethodName)
                    ?? throw new ArgumentNullException(
                        $"Cannot find Method with name [{key.MethodName}] in class [{key.ClassName}]"
                    );

                var instanceParam = Expression.Parameter(typeof(object), "instance");
                var argumentsParam = Expression.Parameter(typeof(object[]), "arguments");

                var methodParams = methodInfo.GetParameters();
                var argumentExpressions = new Expression[methodParams.Length];

                for (int i = 0; i < methodParams.Length; i++)
                {
                    var paramType = methodParams[i].ParameterType;
                    var arrayAccess = Expression.ArrayIndex(argumentsParam, Expression.Constant(i));
                    argumentExpressions[i] = Expression.Convert(arrayAccess, paramType);
                }

                var instance = Expression.Convert(instanceParam, type);
                var methodCall = Expression.Call(instance, methodInfo, argumentExpressions);

                var lambda = Expression.Lambda<Func<object, object[], Task<T>>>(
                    Expression.Convert(methodCall, typeof(Task<T>)),
                    instanceParam,
                    argumentsParam
                );

                return lambda.Compile();
            }
        );

        var classInstance =
            EngineContext.Current.ResolveUnregistered(_typeCache[fullClassName])
            ?? throw new ArgumentNullException($"Cannot create an instance of [{fullClassName}]");

        return await ((Func<object, object[], Task<T>>)compiledDelegate)(classInstance, parameters)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Dynamics the invoke using the specified full class name
    /// </summary>
    /// <typeparam name="T">The </typeparam>
    /// <param name="fullClassName">The full class name</param>
    /// <param name="methodName">The method name</param>
    /// <param name="parameters">The parameters</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentNullException">Cannot find Method with name [{methodName}] in class [{fullClassName}]]</exception>
    /// <exception cref="ArgumentNullException">Cannot find class with Name=[{fullClassName} in assembly [{neptuneAssemblyName}]]</exception>
    /// <returns>The</returns>
    public static T? DynamicInvoke<T>(string fullClassName, string methodName, object[] parameters)
    {
        try
        {
            string neptuneAssemblyName = O24Helper.GetO24AssemblyName(fullClassName);
            Type type =
                Type.GetType(fullClassName + ", " + neptuneAssemblyName)
                ?? throw new ArgumentNullException(
                    $"Cannot find class with Name=[{fullClassName} in assembly [{neptuneAssemblyName}]]"
                );
            MethodInfo method =
                type.GetMethod(methodName)
                ?? throw new ArgumentNullException(
                    $"Cannot find Method with name [{methodName}] in class [{fullClassName}]]"
                );
            object obj =
                EngineContext.Current.ResolveUnregistered(type)
                ?? throw new ArgumentNullException(
                    "Cannot create an instance of [" + fullClassName + "]"
                );
            return (T?)method.Invoke(obj, parameters);
        }
        catch
        {
            throw;
        }
    }

    /// <summary>
    /// Dynamics the invoke using the specified full class name
    /// </summary>
    /// <typeparam name="T">The </typeparam>
    /// <param name="fullClassName">The full class name</param>
    /// <param name="methodName">The method name</param>
    /// <param name="parameters">The parameters</param>
    /// /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentNullException">Cannot find Method with name [{methodName}] in class [{fullClassName}]]</exception>
    /// <exception cref="ArgumentNullException">Cannot find class with Name=[{fullClassName} in assembly [{assemblyName}]]</exception>
    /// <returns>A task containing the</returns>
    public static async Task<T> DynamicInvokeAsync<T>(
        string fullClassName,
        string methodName,
        object[] parameters
    )
    {
        string assemblyName = O24Helper.GetO24AssemblyName(fullClassName);
        Type type =
            Type.GetType(fullClassName + ", " + assemblyName)
            ?? throw new ArgumentNullException(
                $"Cannot find class with Name=[{fullClassName} in assembly [{assemblyName}]]"
            );

        MethodInfo method =
            type.GetMethod(methodName)
            ?? throw new ArgumentNullException(
                $"Cannot find Method with name [{methodName}] in class [{fullClassName}]]"
            );

        object classInstance =
            EngineContext.Current.ResolveUnregistered(type)
            ?? throw new ArgumentNullException(
                "Cannot create an instance of [" + fullClassName + "]"
            );

        Task<T>? task = (Task<T>?)method.Invoke(classInstance, parameters);
        if (task is not null)
        {
            return await task;
        }
        return default!;
    }

    /// <summary>
    /// Dynamics the invoke using the specified full class name
    /// </summary>
    /// <param name="fullClassName">The full class name</param>
    /// <param name="methodName">The method name</param>
    /// <param name="parameters">The parameters</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentNullException">Cannot find Method with name [{methodName}] in class [{fullClassName}]]</exception>
    /// <exception cref="ArgumentNullException">Cannot find class with Name=[{fullClassName} in assembly [{assemblyName}]]</exception>
    public static async Task DynamicInvokeAsync(
        string fullClassName,
        string methodName,
        object[] parameters
    )
    {
        string assemblyName = O24Helper.GetO24AssemblyName(fullClassName);

        Type type =
            Type.GetType(fullClassName + ", " + assemblyName)
            ?? throw new ArgumentNullException(
                $"Cannot find class with Name=[{fullClassName} in assembly [{assemblyName}]]"
            );
        MethodInfo method =
            type.GetMethod(methodName)
            ?? throw new ArgumentNullException(
                $"Cannot find Method with name [{methodName}] in class [{fullClassName}]]"
            );
        object classInstance =
            EngineContext.Current.ResolveUnregistered(type)
            ?? throw new ArgumentNullException(
                "Cannot create an instance of [" + fullClassName + "]"
            );
        var task = (Task?)method.Invoke(classInstance, parameters);
        if (task is not null)
        {
            await task;
        }
    }

    /// <summary>
    /// Finds the entity using the specified entity name
    /// </summary>
    /// <param name="entityName">The entity name</param>
    /// <exception cref="Exception"></exception>
    /// <returns>The type</returns>
    public static Type FindEntity(string entityName)
    {
        var type =
            Singleton<ITypeFinder>
                .Instance?.FindClassesOfType<BaseEntity>()
                .FirstOrDefault(t => t.Name.Equals(entityName))
            ?? throw new Exception("Cannot find the type " + entityName);
        return type;
    }

    /// <summary>
    /// Finds the property using the specified entity name
    /// </summary>
    /// <param name="entityName">The entity name</param>
    /// <param name="propertyName">The property name</param>
    /// <exception cref="Exception"></exception>
    /// <returns>The property</returns>
    public static PropertyInfo? FindProperty(string entityName, string propertyName)
    {
        Type type = FindEntity(entityName);
        if (type is null)
        {
            return null;
        }
        PropertyInfo property =
            type.GetProperty(propertyName)
            ?? throw new Exception(
                "Cannot find property " + propertyName + " of type " + entityName
            );
        return property;
    }
}
