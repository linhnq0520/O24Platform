using FluentMigrator.Builders.Create.Table;
using O24OpenAPI.Core.Configuration;
using O24OpenAPI.Data.Attributes;
using O24OpenAPI.Data.Extensions;
using O24OpenAPI.Data.Mapping.Builders;

namespace O24OpenAPI.Data.Migrations.Builders;

[DatabaseType(DataProviderType.Oracle)]
public class SettingBuilder : O24OpenAPIEntityBuilder<Setting>
{
    /// <summary>
    /// Maps the entity using the specified table
    /// </summary>
    /// <param name="table">The table</param>
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(Setting.Name))
            .AsString(500)
            .NotNullable()
            .Unique()
            .WithColumn(nameof(Setting.OrganizationId))
            .AsInt32()
            .NotNullable()
            .WithColumn(nameof(Setting.Value))
            .AsNCLOB()
            .NotNullable();

    }
}
