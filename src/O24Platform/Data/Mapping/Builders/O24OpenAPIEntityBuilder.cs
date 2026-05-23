using FluentMigrator.Builders.Create.Table;
using O24OpenAPI.Core.Domain;

namespace O24OpenAPI.Data.Mapping.Builders;

/// <summary>
/// The 24 open api entity builder class
/// </summary>
/// <seealso cref="IEntityBuilder"/>
public abstract class O24OpenAPIEntityBuilder<TEntity> : IEntityBuilder
    where TEntity : BaseEntity
{
    /// <summary>
    /// Maps the entity using the specified table
    /// </summary>
    /// <param name="table">The table</param>
    public abstract void MapEntity(CreateTableExpressionBuilder table);
}
