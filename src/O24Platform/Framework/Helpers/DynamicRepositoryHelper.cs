using System.Reflection;
using System.Runtime.ExceptionServices;
using O24OpenAPI.Core.Domain;
using O24OpenAPI.Core.Helper;
using O24OpenAPI.Core.Infrastructure;
using O24OpenAPI.Core.SeedWork;
using O24OpenAPI.Framework.Extensions;

namespace O24OpenAPI.Framework.Helpers;

public static class DynamicRepositoryHelper
{
    /// <summary>
    /// Resolve IRepository&lt;<paramref name="entityType"/>&gt; and invoke an async method on it.
    /// Accepts both Task and Task&lt;T&gt; return types.
    /// </summary>
    /// <param name="entityType">Concrete entity type for IRepository&lt;T&gt;.</param>
    /// <param name="methodName">Repository method name (e.g., "GetByFields").</param>
    /// <param name="parameters">Method parameters (in order).</param>
    /// <returns>The result object for Task&lt;T&gt;, or null if Task (void).</returns>
    public static async Task<object> DynamicInvokeRepository(
        Type entityType,
        string methodName,
        params object[] parameters
    )
    {
        ArgumentNullException.ThrowIfNull(entityType);
        if (string.IsNullOrWhiteSpace(methodName))
        {
            throw new ArgumentException("Method name is required.", nameof(methodName));
        }

        var repoType = typeof(IRepository<>).MakeGenericType(entityType);
        var repo =
            EngineContext.Current.Resolve(repoType)
            ?? throw new InvalidOperationException(
                $"Could not resolve IRepository<{entityType.Name}> from DI."
            );

        var method = FindMethod(repoType, methodName, parameters);
        if (method == null)
        {
            throw new MissingMethodException(
                $"{repoType.FullName} does not contain method '{methodName}' matching provided parameters."
            );
        }

        object[] invokeParameters = BuildInvokeParameters(method, parameters);

        object invokeResult;
        try
        {
            invokeResult = method.Invoke(repo, invokeParameters);
        }
        catch (TargetInvocationException tie)
        {
            var inner = tie.InnerException ?? tie;
            await inner.LogErrorAsync($"Invoke error: {inner.Message}").ConfigureAwait(false);
            ExceptionDispatchInfo.Capture(inner).Throw();
            throw;
        }

        return await AwaitMaybeGenericTaskAsync(invokeResult).ConfigureAwait(false);
    }

    /// <summary>
    /// Resolve entity type by string name, then call <see cref="DynamicInvokeRepository(Type, string, object?[])"/>.
    /// </summary>
    public static async Task<object> DynamicInvokeRepository(
        string typeName,
        string methodName,
        params object[] parameters
    )
    {
        if (string.IsNullOrWhiteSpace(typeName))
        {
            throw new ArgumentException("Type name is required.", nameof(typeName));
        }

        var entityType =
            ReflectionHelper.FindEntity(typeName)
            ?? throw new TypeLoadException($"Entity type '{typeName}' was not found.");

        return await DynamicInvokeRepository(entityType, methodName, parameters)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Dynamically call a repository method "FilterAndUpdate" on the entity identified by <paramref name="fullClassName"/>.
    /// </summary>
    /// <remarks>
    /// <paramref name="fullClassName"/> should be the full type name; if your environment requires
    /// assembly suffixing (e.g. "TypeFullName, Assembly"), you can keep the original extension method
    /// <c>GetNeptuneAssemblyName()</c> to resolve it. Here we rely on <see cref="DynamicInvokeRepository(string, string, object?[])"/>.
    /// </remarks>
    public static async Task DynamicInvokeFilterAndUpdate(
        string fullClassName,
        Dictionary<string, string> searchInput,
        string propertyName,
        string value
    )
    {
        if (string.IsNullOrWhiteSpace(fullClassName))
        {
            throw new ArgumentException("Full class name is required.", nameof(fullClassName));
        }

        if (searchInput == null)
        {
            throw new ArgumentNullException(nameof(searchInput));
        }

        if (string.IsNullOrWhiteSpace(propertyName))
        {
            throw new ArgumentException("Property name is required.", nameof(propertyName));
        }

        await DynamicInvokeRepository(
                fullClassName,
                "FilterAndUpdate",
                searchInput,
                propertyName,
                value
            )
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Like <see cref="DynamicInvokeFilterAndUpdate"/> but calls <c>FilterAndUpdateWithAudit</c> so auditable entities persist <see cref="EntityAudit"/> rows for compensation/revert.
    /// </summary>
    public static async Task DynamicInvokeFilterAndUpdateWithAudit(
        string fullClassName,
        Dictionary<string, string> searchInput,
        string propertyName,
        string value
    )
    {
        if (string.IsNullOrWhiteSpace(fullClassName))
        {
            throw new ArgumentException("Full class name is required.", nameof(fullClassName));
        }

        if (searchInput == null)
        {
            throw new ArgumentNullException(nameof(searchInput));
        }

        if (string.IsNullOrWhiteSpace(propertyName))
        {
            throw new ArgumentException("Property name is required.", nameof(propertyName));
        }

        await DynamicInvokeRepository(
                fullClassName,
                "FilterAndUpdateWithAudit",
                searchInput,
                propertyName,
                value
            )
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Dynamically call repository method "GetByFields" and cast result to <typeparamref name="T"/>.
    /// Returns default(T) if null.
    /// </summary>
    public static async Task<T> DynamicGetByFields<T>(
        string fullClassName,
        Dictionary<string, string> searchInput
    )
        where T : BaseEntity
    {
        if (string.IsNullOrWhiteSpace(fullClassName))
        {
            throw new ArgumentException("Full class name is required.", nameof(fullClassName));
        }

        if (searchInput == null)
        {
            throw new ArgumentNullException(nameof(searchInput));
        }

        var result = await DynamicInvokeRepository(fullClassName, "GetByFields", searchInput)
            .ConfigureAwait(false);
        return result as T;
    }

    // ------------------------- helpers -------------------------

    private static MethodInfo FindMethod(Type repoType, string methodName, object[] args)
    {
        // Find first match by name and compatible parameters.
        var candidates = repoType
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Where(m => string.Equals(m.Name, methodName, StringComparison.Ordinal))
            .ToArray();

        if (candidates.Length == 0)
        {
            return null;
        }

        foreach (var m in candidates)
        {
            var ps = m.GetParameters();
            if (!CanInvokeWithArguments(ps, args))
            {
                continue;
            }

            return m;
        }

        return null;
    }

    private static bool CanInvokeWithArguments(ParameterInfo[] parameters, object[] args)
    {
        args ??= [];
        if (args.Length > parameters.Length)
        {
            return false;
        }

        for (int i = 0; i < args.Length; i++)
        {
            object arg = args[i];
            Type parameterType = parameters[i].ParameterType;
            if (arg is null)
            {
                if (parameterType.IsValueType && Nullable.GetUnderlyingType(parameterType) == null)
                {
                    return false;
                }

                continue;
            }

            if (!parameterType.IsInstanceOfType(arg))
            {
                return false;
            }
        }

        for (int i = args.Length; i < parameters.Length; i++)
        {
            ParameterInfo parameter = parameters[i];
            if (!parameter.IsOptional && !parameter.HasDefaultValue)
            {
                return false;
            }
        }

        return true;
    }

    private static object[] BuildInvokeParameters(MethodInfo method, object[] args)
    {
        args ??= [];
        ParameterInfo[] parameters = method.GetParameters();
        if (args.Length == parameters.Length)
        {
            return args;
        }

        object[] invokeParameters = new object[parameters.Length];
        Array.Copy(args, invokeParameters, args.Length);

        for (int i = args.Length; i < parameters.Length; i++)
        {
            ParameterInfo parameter = parameters[i];
            invokeParameters[i] = parameter.HasDefaultValue ? parameter.DefaultValue : Type.Missing;
        }

        return invokeParameters;
    }

    /// <summary>
    /// Await an object that may be Task or Task&lt;T&gt;; returns T (or null for Task).
    /// </summary>
    private static async Task<object> AwaitMaybeGenericTaskAsync(object taskObj)
    {
        if (taskObj is null)
        {
            return null;
        }

        if (taskObj is Task task)
        {
            await task.ConfigureAwait(false);

            var t = task.GetType();
            var resultProp = t.GetProperty("Result");
            if (resultProp != null && resultProp.CanRead)
            {
                return resultProp.GetValue(task);
            }

            return null;
        }

        return taskObj;
    }
}
