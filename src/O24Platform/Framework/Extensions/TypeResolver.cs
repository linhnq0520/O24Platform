namespace O24OpenAPI.Framework.Extensions;

public static class TypeResolver
{
    public static Type? ResolveTypeByFullName(string fullClassName)
    {
        if (string.IsNullOrWhiteSpace(fullClassName))
        {
            return null;
        }

        var t = Type.GetType(fullClassName, throwOnError: false);
        if (t != null)
        {
            return t;
        }

        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                t = asm.GetType(fullClassName, throwOnError: false, ignoreCase: false);
                if (t != null)
                {
                    return t;
                }
            }
            catch { }
        }

        return null;
    }

    public static Type RequireType(string fullClassName) =>
        ResolveTypeByFullName(fullClassName)
        ?? throw new TypeLoadException(
            $"Cannot resolve type '{fullClassName}'. Make sure its assembly is loaded."
        );
}
