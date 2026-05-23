using System.Text.Json;
using O24OpenAPI.Core.Domain;
using O24OpenAPI.Core.Infrastructure.Mapper;
using O24OpenAPI.Framework.Helpers;

namespace O24OpenAPI.Framework.Infrastructure.Mapper.Extensions;

/// <summary>
/// The mapping extensions class
/// </summary>
public static class MappingExtensions
{
    /// <summary>
    /// Maps the source
    /// </summary>
    /// <typeparam name="TDestination">The destination</typeparam>
    /// <param name="source">The source</param>
    /// <returns>The destination</returns>
    private static TDestination Map<TDestination>(this object source) =>
        AutoMapperConfiguration.Mapper.Map<TDestination>(source);

    /// <summary>
    /// Maps the to using the specified source
    /// </summary>
    /// <typeparam name="TSource">The source</typeparam>
    /// <typeparam name="TDestination">The destination</typeparam>
    /// <param name="source">The source</param>
    /// <param name="destination">The destination</param>
    /// <returns>The destination</returns>
    private static TDestination MapTo<TSource, TDestination>(
        this TSource source,
        TDestination destination
    )
    {
        return AutoMapperConfiguration.Mapper.Map(source, destination);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="entity"></param>
    /// <typeparam name="TBaseJsonModel"></typeparam>
    /// <returns></returns>
    public static TBaseJsonModel ToModel<TBaseJsonModel>(this object entity)
        where TBaseJsonModel : BaseO24OpenAPIModel =>
        entity != null
            ? entity.Map<TBaseJsonModel>()
            : throw new ArgumentNullException(nameof(entity));

    /// <summary>
    ///
    /// </summary>
    /// <param name="model"></param>
    /// <typeparam name="TEntity"></typeparam>
    /// <returns></returns>
    public static TEntity FromModel<TEntity>(this BaseO24OpenAPIModel model) =>
        model != null ? model.Map<TEntity>() : throw new ArgumentNullException(nameof(model));

    /// <summary>Execute a mapping from the model to a new entity</summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    /// <param name="entity">Entity to map from</param>
    /// <returns>Mapped entity</returns>
    public static TEntity ToEntity<TEntity>(this BaseEntity entity)
        where TEntity : BaseEntity =>
        entity != null ? entity.Map<TEntity>() : throw new ArgumentNullException(nameof(entity));

    /// <summary>
    /// Returns the entity using the specified entity
    /// </summary>
    /// <typeparam name="TEntity">The entity</typeparam>
    /// <param name="entity">The entity</param>
    /// <param name="destination">The destination</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <returns>The entity</returns>
    public static TEntity ToEntity<TEntity>(this BaseEntity entity, TEntity destination)
        where TEntity : BaseEntity
    {
        ArgumentNullException.ThrowIfNull(entity);
        return destination != null
            ? entity.MapTo(destination)
            : throw new ArgumentNullException(nameof(destination));
    }

    /// <summary>
    /// Returns the entity using the specified model
    /// </summary>
    /// <typeparam name="TEntity">The entity</typeparam>
    /// <typeparam name="TModel">The model</typeparam>
    /// <param name="model">The model</param>
    /// <param name="entity">The entity</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <returns>The entity</returns>
    public static TEntity ToEntity<TEntity, TModel>(this TModel model, TEntity entity)
        where TEntity : BaseEntity
        where TModel : BaseO24OpenAPIModel
    {
        ArgumentNullException.ThrowIfNull(model);
        return entity != null
            ? model.MapTo(entity)
            : throw new ArgumentNullException(nameof(entity));
    }

    /// <summary>
    /// To Entity Nullable
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TModel"></typeparam>
    /// <param name="model"></param>
    /// <param name="entity"></param>
    /// <returns></returns>
    public static TEntity ToEntityNullable<TEntity, TModel>(this TModel model, TEntity entity)
        where TEntity : BaseEntity
        where TModel : BaseO24OpenAPIModel
    {
        if (model == null)
        {
            return entity;
        }

        var minDate = new DateTime(1753, 1, 1);
        var modelProps = typeof(TModel).GetProperties();
        var entityProps = typeof(TEntity).GetProperties();

        foreach (var modelProp in modelProps)
        {
            var entityProp = entityProps.FirstOrDefault(p =>
                p.Name == modelProp.Name && p.CanWrite
            );
            if (entityProp == null)
            {
                continue;
            }

            var modelValue = modelProp.GetValue(model);

            if (
                modelProp.PropertyType == typeof(DateTime?)
                && entityProp.PropertyType == typeof(DateTime)
            )
            {
                var modelDate = (DateTime?)modelValue;
                var currentEntityDate = (DateTime)entityProp.GetValue(entity);

                if (modelDate.HasValue && modelDate.Value >= minDate)
                {
                    entityProp.SetValue(entity, modelDate.Value);
                }
                else
                {
                    modelProp.SetValue(model, currentEntityDate);
                }

                continue;
            }

            if (modelValue == null)
            {
                continue;
            }

            var isMediaField =
                entityProp.GetCustomAttributes(typeof(MediaFieldAttribute), false).Length != 0;
            if (isMediaField)
            {
                if (modelValue is string str)
                {
                    entityProp.SetValue(entity, str);
                }
                else
                {
                    try
                    {
                        var json = JsonSerializer.Serialize(modelValue);
                        entityProp.SetValue(entity, json);
                    }
                    catch { }
                }

                continue;
            }

            entityProp.SetValue(entity, modelValue);
        }

        return entity;
    }
}
