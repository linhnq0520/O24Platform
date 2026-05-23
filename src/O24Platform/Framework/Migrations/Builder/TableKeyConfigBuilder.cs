using FluentMigrator.Builders.Create.Table;
using O24OpenAPI.Data.Mapping.Builders;
using O24OpenAPI.Framework.Domain;

namespace O24OpenAPI.Framework.Migrations.Builder;

/// <summary>
/// The entity audit builder class
/// </summary>
/// <seealso cref="O24OpenAPIEntityBuilder{EntityAudit}"/>
public class TableKeyConfigBuilder : O24OpenAPIEntityBuilder<TableKeyConfig>
{
    /// <summary>
    /// Maps the entity using the specified table
    /// </summary>
    /// <param name="table">The table</param>
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(TableKeyConfig.SchemaName))
            .AsString(255)
            .NotNullable()
            .WithColumn(nameof(TableKeyConfig.TableName))
            .AsString(255)
            .NotNullable()
            .WithColumn(nameof(TableKeyConfig.KeyColumn))
            .AsString(255)
            .NotNullable()
            .WithColumn(nameof(TableKeyConfig.SortOrder))
            .AsInt16()
            .NotNullable()
            .WithColumn(nameof(TableKeyConfig.IsActive))
            .AsBoolean()
            .NotNullable();
    }
}
