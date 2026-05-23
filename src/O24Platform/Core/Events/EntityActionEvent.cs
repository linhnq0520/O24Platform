using O24OpenAPI.Core.Enums;

namespace O24OpenAPI.Core.Events;

/// <summary>
/// Initializes a new instance of the <see cref="EntityInsertedEvent"/> class
/// </summary>
/// <param name="entity">The entity</param>
public class EntityActionEvent(string entityName, object entity, EntityActionEnum action)
{
    /// <summary>
    /// Gets or sets the value of the entity name
    /// </summary>
    public string EntityName { get; set; } = entityName;

    /// <summary>
    /// Gets the value of the entity
    /// </summary>
    public object Entity { get; } = entity;

    /// <summary>
    ///
    /// </summary>
    /// <value></value>
    public EntityActionEnum Action { get; set; } = action;
}
