using FluentMigrator.Builders.Create.Table;
using O24OpenAPI.Core.Domain;
using O24OpenAPI.Data.Attributes;
using O24OpenAPI.Data.Extensions;
using O24OpenAPI.Data.Mapping.Builders;

namespace O24OpenAPI.Data.Migrations.Builders;

/// <summary>
/// The stored command builder class
/// </summary>
/// <seealso cref="O24OpenAPIEntityBuilder{StoredCommand}"/>
[DatabaseType(DataProviderType.Oracle)]
public class ORACLE_StoredCommandBuilder : O24OpenAPIEntityBuilder<StoredCommand>
{
    /// <summary>
    /// Maps the entity using the specified table
    /// </summary>
    /// <param name="table">The table</param>
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(StoredCommand.Name))
            .AsString(100)
            .NotNullable()
            .WithColumn(nameof(StoredCommand.Query))
            .AsNCLOB()
            .NotNullable()
            .WithColumn(nameof(StoredCommand.Type))
            .AsString(50)
            .NotNullable()
            .WithColumn(nameof(StoredCommand.Description))
            .AsString(500)
            .Nullable()
            .WithColumn(nameof(StoredCommand.CreatedOnUtc))
            .AsDateTime2()
            .Nullable()
            .WithColumn(nameof(StoredCommand.UpdatedOnUtc))
            .AsDateTime2()
            .Nullable();
    }
}
