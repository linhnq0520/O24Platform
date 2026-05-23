using FluentMigrator.Builders.Create.Table;
using O24OpenAPI.Data.Mapping.Builders;
using O24OpenAPI.Framework.Domain;

namespace O24OpenAPI.Framework.Migrations.Builder;

/// <summary>
/// The 24 open api service builder class
/// </summary>
/// <seealso cref="O24OpenAPIEntityBuilder{O24OpenAPIService}"/>
public class O24OpenAPIServiceBuilder : O24OpenAPIEntityBuilder<O24OpenAPIService>
{
    /// <summary>
    /// Maps the entity using the specified table
    /// </summary>
    /// <param name="table">The table</param>
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(O24OpenAPIService.StepCode))
            .AsString(255)
            .NotNullable()
            .WithColumn(nameof(O24OpenAPIService.FullClassName))
            .AsString(2000)
            .Nullable()
            .WithColumn(nameof(O24OpenAPIService.MethodName))
            .AsString(255)
            .Nullable()
            .WithColumn(nameof(O24OpenAPIService.ShouldAwait))
            .AsBoolean()
            .NotNullable()
            .WithColumn(nameof(O24OpenAPIService.IsInquiry))
            .AsBoolean()
            .NotNullable()
            .WithColumn(nameof(O24OpenAPIService.IsModuleExecute))
            .AsBoolean()
            .NotNullable()
            .WithColumn(nameof(O24OpenAPIService.IsAutoReverse))
            .AsBoolean()
            .Nullable();
    }
}
