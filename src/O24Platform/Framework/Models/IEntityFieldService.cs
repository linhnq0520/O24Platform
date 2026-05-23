using O24OpenAPI.Core.Domain;

namespace O24OpenAPI.Framework.Models;

/// <summary>
/// The entity field service interface
/// </summary>
public interface IEntityFieldService
{
    /// <summary>
    /// Gets the by id using the specified id
    /// </summary>
    /// <param name="id">The id</param>
    /// <returns>A task containing the entity field</returns>
    Task<EntityField> GetById(int id);

    /// <summary>
    /// Gets the by entity field using the specified entity name
    /// </summary>
    /// <param name="entityName">The entity name</param>
    /// <param name="entityField">The entity field</param>
    /// <param name="lang">The lang</param>
    /// <param name="moduleCode">The module code</param>
    /// <returns>A task containing the string</returns>
    Task<string> GetByEntityField(
        string entityName,
        string entityField,
        string lang = "",
        string moduleCode = ""
    );
}
