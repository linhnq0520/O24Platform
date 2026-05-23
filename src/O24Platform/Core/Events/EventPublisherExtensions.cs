using O24OpenAPI.Core.Configuration;
using O24OpenAPI.Core.Enums;
using O24OpenAPI.Core.Infrastructure;

namespace O24OpenAPI.Core.Events;

/// <summary>
/// The event publisher extensions class
/// </summary>
public static class EventPublisherExtensions
{
    /// <summary>
    /// Entities the inserted using the specified event publisher
    /// </summary>
    /// <typeparam name="T">The </typeparam>
    /// <param name="eventPublisher">The event publisher</param>
    /// <param name="entity">The entity</param>
    public static async Task EntityInserted<T>(this IEventPublisher eventPublisher, T entity)
        where T : BaseEntity =>
        await eventPublisher.Publish<EntityInsertedEvent<T>>(new EntityInsertedEvent<T>(entity));

    /// <summary>
    /// Entities the inserted using the specified event publisher
    /// </summary>
    /// <typeparam name="T">The </typeparam>
    /// <param name="eventPublisher">The event publisher</param>
    /// <param name="entity">The entity</param>
    /// <param name="details">The details</param>
    public static async Task EntityInserted<T>(
        this IEventPublisher eventPublisher,
        T entity,
        List<TransactionDetails> details
    )
        where T : BaseEntity
    {
        await eventPublisher.Publish<EntityInsertedEvent<T>>(
            new EntityInsertedEvent<T>(entity, details)
        );
    }

    /// <summary>
    /// Entities the updated using the specified event publisher
    /// </summary>
    /// <typeparam name="T">The </typeparam>
    /// <param name="eventPublisher">The event publisher</param>
    /// <param name="entity">The entity</param>
    public static async Task EntityUpdated<T>(this IEventPublisher eventPublisher, T entity)
        where T : BaseEntity =>
        await eventPublisher.Publish<EntityUpdatedEvent<T>>(new EntityUpdatedEvent<T>(entity));

    /// <summary>
    /// Entities the updated using the specified event publisher
    /// </summary>
    /// <typeparam name="T">The </typeparam>
    /// <param name="eventPublisher">The event publisher</param>
    /// <param name="entity">The entity</param>
    /// <param name="details">The details</param>
    public static async Task EntityUpdated<T>(
        this IEventPublisher eventPublisher,
        T entity,
        List<TransactionDetails> details
    )
        where T : BaseEntity
    {
        await eventPublisher.Publish<EntityUpdatedEvent<T>>(
            new EntityUpdatedEvent<T>(entity, details)
        );
    }

    /// <summary>
    /// Entities the deleted using the specified event publisher
    /// </summary>
    /// <typeparam name="T">The </typeparam>
    /// <param name="eventPublisher">The event publisher</param>
    /// <param name="entity">The entity</param>
    public static async Task EntityDeleted<T>(this IEventPublisher eventPublisher, T entity)
        where T : BaseEntity =>
        await eventPublisher.Publish<EntityDeletedEvent<T>>(new EntityDeletedEvent<T>(entity));

    /// <summary>
    /// Entities the deleted using the specified event publisher
    /// </summary>
    /// <typeparam name="T">The </typeparam>
    /// <param name="eventPublisher">The event publisher</param>
    /// <param name="entity">The entity</param>
    /// <param name="details">The details</param>
    public static async Task EntityDeleted<T>(
        this IEventPublisher eventPublisher,
        T entity,
        List<TransactionDetails> details
    )
        where T : BaseEntity
    {
        await eventPublisher.Publish<EntityDeletedEvent<T>>(
            new EntityDeletedEvent<T>(entity, details)
        );
    }

    /// <summary>
    /// Entities the changed using the specified event publisher
    /// </summary>
    /// <typeparam name="T">The </typeparam>
    /// <param name="eventPublisher">The event publisher</param>
    /// <param name="entity">The entity</param>
    public static async Task EntityAction<T>(
        this IEventPublisher eventPublisher,
        T entity,
        EntityActionEnum action
    )
        where T : BaseEntity
    {
        HashSet<string>? dataWarehouseEntities = Singleton<O24OpenAPIConfiguration>
            .Instance
            ?.DataWarehouseEntities;

        if (dataWarehouseEntities == null || !dataWarehouseEntities.Contains(typeof(T).Name))
        {
            return;
        }

        await eventPublisher.Publish(new EntityActionEvent(typeof(T).Name, entity, action));
    }
}
