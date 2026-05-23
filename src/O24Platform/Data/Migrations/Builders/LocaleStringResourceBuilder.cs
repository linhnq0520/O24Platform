using FluentMigrator.Builders.Create.Table;
using O24OpenAPI.Core.Domain.Localization;
using O24OpenAPI.Data.Mapping.Builders;

namespace O24OpenAPI.Data.Migrations.Builders;

/// <summary>
/// The session manager builder class
/// </summary>
/// <seealso cref="O24OpenAPIEntityBuilder{SessionManager}"/>
public class LocaleStringResourceBuilder : O24OpenAPIEntityBuilder<LocaleStringResource>
{
    /// <summary>
    /// Maps the entity using the specified table
    /// </summary>
    /// <param name="table">The table</param>
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(LocaleStringResource.Language))
            .AsString(5)
            .NotNullable()
            .WithColumn(nameof(LocaleStringResource.ResourceName))
            .AsString(2000)
            .NotNullable()
            .WithColumn(nameof(LocaleStringResource.ResourceValue))
            .AsString(2000)
            .NotNullable()
            .WithColumn(nameof(LocaleStringResource.ResourceCode))
            .AsString(200)
            .Nullable();
    }
}
