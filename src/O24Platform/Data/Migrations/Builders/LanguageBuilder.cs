using FluentMigrator.Builders.Create.Table;
using O24OpenAPI.Core.Domain.Localization;
using O24OpenAPI.Data.Attributes;
using O24OpenAPI.Data.Mapping.Builders;

namespace O24OpenAPI.Data.Migrations.Builders;

[DatabaseType(DataProviderType.Oracle)]
public class LanguageBuilder : O24OpenAPIEntityBuilder<Language>
{
    /// <summary>
    /// Maps the entity using the specified table
    /// </summary>
    /// <param name="table">The table</param>
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(Language.Name))
            .AsString(255)
            .NotNullable()
            .WithColumn(nameof(Language.LanguageCulture))
            .AsString(255)
            .NotNullable()
            .WithColumn(nameof(Language.UniqueSeoCode))
            .AsString(255)
            .NotNullable()
            .WithColumn(nameof(Language.DisplayOrder))
            .AsInt32()
            .NotNullable()
            ;
    }
}
