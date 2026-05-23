namespace O24OpenAPI.Core.Events;

/// <summary>
/// The entity inserted event class
/// </summary>
public class EntityInsertedEvent<T>
    where T : BaseEntity
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityInsertedEvent"/> class
    /// </summary>
    /// <param name="entity">The entity</param>
    public EntityInsertedEvent(T entity) => this.Entity = entity;

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityInsertedEvent"/> class
    /// </summary>
    /// <param name="entity">The entity</param>
    /// <param name="details">The details</param>
    public EntityInsertedEvent(T entity, List<TransactionDetails> details)
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
