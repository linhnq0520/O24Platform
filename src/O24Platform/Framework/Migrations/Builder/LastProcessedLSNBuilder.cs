using FluentMigrator.Builders.Create.Table;
using O24OpenAPI.Core.Configuration;
using O24OpenAPI.Core.Infrastructure;
using O24OpenAPI.Data.Mapping.Builders;
using O24OpenAPI.Framework.Domain;

namespace O24OpenAPI.Framework.Migrations.Builder;

/// <summary>
/// The last processed lsn builder class
/// </summary>
/// <seealso cref="O24OpenAPIEntityBuilder{LastProcessedLSN}"/>
public class LastProcessedLSNBuilder : O24OpenAPIEntityBuilder<LastProcessedLSN>
{
    /// <summary>
    /// Maps the entity using the specified table
    /// </summary>
    /// <param name="table">The table</param>
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .InSchema(Singleton<O24OpenAPIConfiguration>.Instance.YourCDCSchema)
            .WithColumn(nameof(LastProcessedLSN.TableName))
            .AsString(100)
            .NotNullable()
            .WithColumn(nameof(LastProcessedLSN.LastLSN))
            .AsBinary(10)
            .NotNullable()
            .WithColumn(nameof(LastProcessedLSN.LastProcessedTime))
            .AsDateTime2()
            .NotNullable();
    }
}
