using System.Reflection;
using O24OpenAPI.Core.Domain;

namespace O24OpenAPI.Data;

/// <summary>
/// Shallow property copy for building an "old row" baseline before atomic SQL updates, so codegen <c>GetChanges</c> can run on (after, snapshot).
/// </summary>
public static class AuditEntitySnapshot
{
    public static T ShallowClone<T>(T source)
        where T : BaseEntity, new()
    {
        ArgumentNullException.ThrowIfNull(source);
        var copy = new T();
        foreach (
            PropertyInfo p in typeof(T).GetProperties(
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy
            )
        )
        {
            if (!p.CanRead || !p.CanWrite || p.GetIndexParameters().Length > 0)
            {
                continue;
            }

            object? val = p.GetValue(source);
            p.SetValue(copy, val);
        }

        return copy;
    }
}
