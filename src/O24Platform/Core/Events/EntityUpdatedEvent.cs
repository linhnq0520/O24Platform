namespace O24OpenAPI.Core.Events;

/// <summary>
/// The entity updated event class
/// </summary>
public class EntityUpdatedEvent<T>
    where T : BaseEntity
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityUpdatedEvent"/> class
    /// </summary>
    /// <param name="entity">The entity</param>
    public EntityUpdatedEvent(T entity) => this.Entity = entity;

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityUpdatedEvent"/> class
    /// </summary>
    /// <param name="entity">The entity</param>
    /// <param name="details">The details</param>
    public EntityUpdatedEvent(T entity, List<TransactionDetails> details)
    {
        this.Entity = entity;
        this.Details = details;
    }

    /// <summary>
    /// Gets the value of the entity
    /// </summary>
    public T Entity { get; }

    /// <summary>
    /// Gets the value of the details
    /// </summary>
    public List<TransactionDetails> Details { get; }
}
