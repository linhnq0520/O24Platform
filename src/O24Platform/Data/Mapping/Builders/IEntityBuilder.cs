using FluentMigrator.Builders.Create.Table;

namespace O24OpenAPI.Data.Mapping.Builders;

/// <summary>
/// The entity builder interface
/// </summary>
public interface IEntityBuilder
{
    /// <summary>
    /// Maps the entity using the specified table
    /// </summary>
    /// <param name="table">The table</param>
    void MapEntity(CreateTableExpressionBuilder table);
}
