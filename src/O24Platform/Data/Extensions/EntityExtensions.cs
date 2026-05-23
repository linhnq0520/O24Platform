using O24OpenAPI.Core.Domain;
using O24OpenAPI.Core.Extensions;
using O24OpenAPI.Core.Infrastructure;
using O24OpenAPI.Core.SeedWork;

namespace O24OpenAPI.Data.Extensions;

/// <summary>
/// The entity extensions class
/// </summary>
public static class EntityExtensions
{
    /// <summary>
    /// Sets the property date using the specified entity
    /// </summary>
    /// <param name="entity">The entity</param>
    /// <param name="propertyName">The property name</param>
    /// <param name="value">The value</param>
    public static void SetPropertyDate(this BaseEntity entity, string propertyName, DateTime value)
    {
        if (!entity.HasMethod(propertyName))
        {
            return;
        }

        entity.GetType().GetProperty(propertyName)?.SetValue(entity, value);
    }

    public static void SetProperty<T>(this BaseEntity entity, string propertyName, T value)
    {
        if (!entity.HasMethod(propertyName))
        {
            return;
        }

        entity.GetType().GetProperty(propertyName)?.SetValue(entity, value);
    }

    public static T GetProperty<T>(this BaseEntity entity, string propertyName)
    {
        if (!entity.HasMethod(propertyName))
        {
            return default;
        }
        var value = entity.GetType().GetProperty(propertyName)?.GetValue(entity);
        if (value == null)
        {
            return default;
        }
        if (value is T tValue)
        {
            return tValue;
        }
        return default;
    }

    /// <summary>
    /// Inserts the entity
    /// </summary>
    /// <param name="entity">The entity</param>
    /// <param name="referenceId">The reference id</param>
    /// <param name="publishEvent">The publish event</param>
    /// <param name="isReverse">The is reverse</param>
    /// <param name="isRemove">The is remove</param>
    public static async Task Insert(
        this BaseEntity entity,
        string referenceId = "",
        bool publishEvent = true,
        bool isReverse = false,
        bool isRemove = false
    )
    {
        await CallEntity(nameof(Insert), entity, referenceId, publishEvent, isReverse, isRemove);
    }

    /// <summary>
    /// Updates the entity
    /// </summary>
    /// <param name="entity">The entity</param>
    /// <param name="referenceId">The reference id</param>
    /// <param name="publishEvent">The publish event</param>
    /// <param name="isReverse">The is reverse</param>
    /// <param name="isRemove">The is remove</param>
    public static async Task Update(
        this BaseEntity entity,
        string referenceId = "",
        bool publishEvent = true,
        bool isReverse = false,
        bool isRemove = false
    )
    {
        await CallEntity(nameof(Update), entity, referenceId, publishEvent, isReverse, isRemove);
    }

    /// <summary>
    /// Deletes the entity
    /// </summary>
    /// <param name="entity">The entity</param>
    /// <param name="referenceId">The reference id</param>
    /// <param name="publishEvent">The publish event</param>
    /// <param name="isReverse">The is reverse</param>
    /// <param name="isRemove">The is remove</param>
    public static async Task Delete(
        this BaseEntity entity,
        string referenceId = "",
        bool publishEvent = true,
        bool isReverse = false,
        bool isRemove = false
    )
    {
        await CallEntity(nameof(Delete), entity, referenceId, publishEvent, isReverse, isRemove);
    }

    /// <summary>
    /// Updates the chains using the specified entity
    /// </summary>
    /// <param name="entity">The entity</param>
    /// <param name="actions">The actions</param>
    /// <param name="referenceId">The reference id</param>
    /// <param name="publishEvent">The publish event</param>
    /// <param name="isReverse">The is reverse</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static async Task UpdateChains(
        this BaseEntity entity,
        List<ActionChain> actions,
        string referenceId = "",
        bool publishEvent = true,
        bool isReverse = false
    )
    {
        string functionName = nameof(UpdateChains);
        if (entity == null)
        {
            throw new ArgumentNullException(entity.GetType().Name);
        }

        try
        {
            Type obj = typeof(IRepository<>).MakeGenericType(entity.GetType());
            object repo = EngineContext.Current.Resolve(obj);
            Task task = (Task)
                obj.GetMethods()
                    .FirstOrDefault(x => x.Name == functionName)
                    .Invoke(repo, [entity, actions, referenceId, publishEvent, isReverse]);
            await task;
            obj = null;
            repo = null;
            task = null;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            throw;
        }
    }

    /// <summary>
    /// Calls the entity using the specified function name
    /// </summary>
    /// <param name="functionName">The function name</param>
    /// <param name="entity">The entity</param>
    /// <param name="referenceId">The reference id</param>
    /// <param name="publishEvent">The publish event</param>
    /// <param name="isReverse">The is reverse</param>
    /// <param name="isRemove">The is remove</param>
    /// <exception cref="ArgumentNullException"></exception>
    private static async Task CallEntity(
        string functionName,
        BaseEntity entity,
        string referenceId,
        bool publishEvent,
        bool isReverse,
        bool isRemove
    )
    {
        if (entity == null)
        {
            throw new ArgumentNullException(entity.GetType().Name);
        }

        try
        {
            Type obj = typeof(IRepository<>).MakeGenericType(entity.GetType());
            object repo = EngineContext.Current.Resolve(obj);
            Task task = (Task)
                obj.GetMethods()
                    .FirstOrDefault(x => x.Name == functionName)
                    .Invoke(repo, [entity, referenceId, publishEvent, isReverse, isRemove]);
            await task;
            obj = null;
            repo = null;
            task = null;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            throw;
        }
    }

    /// <summary>
    /// Chains the update using the specified entity
    /// </summary>
    /// <param name="entity">The entity</param>
    /// <param name="updateField">The update field</param>
    /// <param name="value">The value</param>
    /// <returns>A list of action chain</returns>
    public static List<ActionChain> ChainUpdate(
        this BaseEntity entity,
        string updateField,
        object value
    )
    {
        return
        [
            new ActionChain()
            {
                Action = "U",
                UpdateField = updateField,
                UpdateValue = value,
            },
        ];
    }

    /// <summary>
    /// Chains the update using the specified chains
    /// </summary>
    /// <param name="chains">The chains</param>
    /// <param name="updateField">The update field</param>
    /// <param name="value">The value</param>
    /// <returns>The chains</returns>
    public static List<ActionChain> ChainUpdate(
        this List<ActionChain> chains,
        string updateField,
        object value
    )
    {
        chains ??= [];
        chains.Add(
            new ActionChain()
            {
                Action = "U",
                UpdateField = updateField,
                UpdateValue = value,
            }
        );
        return chains;
    }

    /// <summary>
    /// Chains the credit using the specified entity
    /// </summary>
    /// <param name="entity">The entity</param>
    /// <param name="updateField">The update field</param>
    /// <param name="value">The value</param>
    /// <returns>A list of action chain</returns>
    public static List<ActionChain> ChainCredit(
        this BaseEntity entity,
        string updateField,
        object value
    )
    {
        if (entity.GetType().GetProperty(updateField).GetValue(entity) == null)
        {
            return
            [
                new ActionChain()
                {
                    Action = "U",
                    UpdateField = updateField,
                    UpdateValue = value,
                },
            ];
        }

        return
        [
            new ActionChain()
            {
                Action = "C",
                UpdateField = updateField,
                UpdateValue = value,
            },
        ];
    }

    /// <summary>
    /// Chains the credit using the specified chains
    /// </summary>
    /// <param name="chains">The chains</param>
    /// <param name="updateField">The update field</param>
    /// <param name="value">The value</param>
    /// <returns>The chains</returns>
    public static List<ActionChain> ChainCredit(
        this List<ActionChain> chains,
        string updateField,
        object value
    )
    {
        chains ??= [];
        chains.Add(
            new ActionChain()
            {
                Action = "C",
                UpdateField = updateField,
                UpdateValue = value,
            }
        );
        return chains;
    }

    /// <summary>
    /// Chains the debit using the specified entity
    /// </summary>
    /// <param name="entity">The entity</param>
    /// <param name="updateField">The update field</param>
    /// <param name="value">The value</param>
    /// <returns>A list of action chain</returns>
    public static List<ActionChain> ChainDebit(
        this BaseEntity entity,
        string updateField,
        object value
    )
    {
        if (entity.GetType().GetProperty(updateField).GetValue(entity) == null)
        {
            return
            [
                new ActionChain()
                {
                    Action = "U",
                    UpdateField = updateField,
                    UpdateValue = value,
                },
            ];
        }
        return
        [
            new ActionChain()
            {
                Action = "D",
                UpdateField = updateField,
                UpdateValue = value,
            },
        ];
    }

    /// <summary>
    /// Chains the debit using the specified chains
    /// </summary>
    /// <param name="chains">The chains</param>
    /// <param name="updateField">The update field</param>
    /// <param name="value">The value</param>
    /// <returns>The chains</returns>
    public static List<ActionChain> ChainDebit(
        this List<ActionChain> chains,
        string updateField,
        object value
    )
    {
        chains ??= [];
        chains.Add(
            new ActionChain()
            {
                Action = "D",
                UpdateField = updateField,
                UpdateValue = value,
            }
        );
        return chains;
    }
}
