using LinqToDB.Mapping;

namespace O24OpenAPI.Data.Mapping;

/// <summary>
/// The mapping entity accessor interface
/// </summary>
public interface IMappingEntityAccessor
{
    /// <summary>
    /// Gets the entity descriptor using the specified entity type
    /// </summary>
    /// <param name="entityType">The entity type</param>
    /// <returns>The 24 open api entity descriptor</returns>
    O24OpenAPIEntityDescriptor GetEntityDescriptor(Type entityType);

    /// <summary>
    /// Gets the mapping schema
    /// </summary>
    /// <returns>The mapping schema</returns>
    MappingSchema GetMappingSchema();
}
