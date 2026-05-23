using System.Linq.Dynamic.Core;
using FluentValidation;
using O24OpenAPI.Core.Domain;
using O24OpenAPI.Data.Mapping;

namespace O24OpenAPI.Framework.Validators;

/// <summary>
/// The base 24 open api validator class
/// </summary>
/// <seealso cref="AbstractValidator{TModel}"/>
public abstract class BaseO24OpenAPIValidator<TModel> : AbstractValidator<TModel>
    where TModel : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BaseO24OpenAPIValidator{TModel}"/> class
    /// </summary>
    protected BaseO24OpenAPIValidator()
    {
        PostInitialize();
    }

    /// <summary>
    /// Posts the initialize
    /// </summary>
    protected virtual void PostInitialize() { }

    /// <summary>
    /// Sets the database validation rules using the specified mapping entity accessor
    /// </summary>
    /// <typeparam name="TEntity">The entity</typeparam>
    /// <param name="mappingEntityAccessor">The mapping entity accessor</param>
    /// <param name="filterStringPropertyNames">The filter string property names</param>
    protected virtual void SetDatabaseValidationRules<TEntity>(
        IMappingEntityAccessor mappingEntityAccessor,
        params string[] filterStringPropertyNames
    )
        where TEntity : BaseEntity
    {
        ArgumentNullException.ThrowIfNull(mappingEntityAccessor);

        O24OpenAPIEntityDescriptor entityDescriptor = mappingEntityAccessor.GetEntityDescriptor(
            typeof(TEntity)
        );
        SetStringPropertiesMaxLength(entityDescriptor, filterStringPropertyNames);
        SetDecimalMaxValue(entityDescriptor);
    }

    /// <summary>
    /// Sets the string properties max length using the specified entity descriptor
    /// </summary>
    /// <param name="entityDescriptor">The entity descriptor</param>
    /// <param name="filterPropertyNames">The filter property names</param>
    protected virtual void SetStringPropertiesMaxLength(
        O24OpenAPIEntityDescriptor entityDescriptor,
        params string[] filterPropertyNames
    )
    {
        if (entityDescriptor == null)
        {
            return;
        }

        List<string> modelPropertyNames = (
            from property in typeof(TModel).GetProperties()
            where
                property.PropertyType == typeof(string)
                && !filterPropertyNames.Contains(property.Name)
            select property.Name
        ).ToList();
        IEnumerable<O24OpenAPIEntityFieldDescriptor> source = entityDescriptor.Fields.Where(
            (O24OpenAPIEntityFieldDescriptor field) =>
                modelPropertyNames.Contains(field.Name)
                && field.Type == typeof(string)
                && field.Size.HasValue
        );
        var list = source
            .Select(
                (O24OpenAPIEntityFieldDescriptor field) =>
                    new
                    {
                        MaxLength = field.Size.Value,
                        Expression = DynamicExpressionParser.ParseLambda<TModel, string>(
                            (ParsingConfig)null,
                            false,
                            "@" + field.Name,
                            Array.Empty<object>()
                        ),
                    }
            )
            .ToList();
        foreach (var item in list)
        {
            DefaultValidatorExtensions.Length(
                (IRuleBuilder<TModel, string>)(object)base.RuleFor<string>(item.Expression),
                0,
                item.MaxLength
            );
        }
    }

    /// <summary>
    /// Sets the decimal max value using the specified entity descriptor
    /// </summary>
    /// <param name="entityDescriptor">The entity descriptor</param>
    protected virtual void SetDecimalMaxValue(O24OpenAPIEntityDescriptor entityDescriptor)
    {
        if (entityDescriptor == null)
        {
            return;
        }

        List<string> modelPropertyNames = (
            from property in typeof(TModel).GetProperties()
            where property.PropertyType == typeof(decimal)
            select property.Name
        ).ToList();
        IEnumerable<O24OpenAPIEntityFieldDescriptor> source = entityDescriptor.Fields.Where(
            (O24OpenAPIEntityFieldDescriptor field) =>
                modelPropertyNames.Contains(field.Name)
                && field.Type == typeof(decimal)
                && field.Size.HasValue
                && field.Precision.HasValue
        );
        if (!source.Any())
        {
            return;
        }

        var list = source
            .Select(
                (O24OpenAPIEntityFieldDescriptor field) =>
                    new
                    {
                        MaxValue = (decimal)
                            Math.Pow(10.0, field.Size.Value - field.Precision.Value),
                        Expression = DynamicExpressionParser.ParseLambda<TModel, decimal>(
                            (ParsingConfig)null,
                            false,
                            field.Name,
                            Array.Empty<object>()
                        ),
                    }
            )
            .ToList();
        foreach (var item in list)
        {
            (
                (IRuleBuilder<TModel, decimal>)(object)base.RuleFor<decimal>(item.Expression)
            ).IsDecimal(item.MaxValue);
        }
    }
}
