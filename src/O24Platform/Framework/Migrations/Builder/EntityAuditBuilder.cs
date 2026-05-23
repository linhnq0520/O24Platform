using FluentMigrator.Builders.Create.Table;
using O24OpenAPI.Core.Domain;
using O24OpenAPI.Data.Mapping.Builders;

namespace O24OpenAPI.Framework.Migrations.Builder;

/// <summary>
/// The entity audit builder class
/// </summary>
/// <seealso cref="O24OpenAPIEntityBuilder{EntityAudit}"/>
public class EntityAuditBuilder : O24OpenAPIEntityBuilder<EntityAudit>
{
    /// <summary>
    /// Maps the entity using the specified table
    /// </summary>
    /// <param name="table">The table</param>
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(EntityAudit.EntityName))
            .AsString(255)
            .NotNullable()
            .WithColumn(nameof(EntityAudit.EntityId))
            .AsInt32()
            .NotNullable()
            .WithColumn(nameof(EntityAudit.UserId))
            .AsString(255)
            .NotNullable()
            .WithColumn(nameof(EntityAudit.ExecutionId))
            .AsString(255)
            .NotNullable()
            .WithColumn(nameof(EntityAudit.ActionType))
            .AsString(255)
            .NotNullable()
            .WithColumn(nameof(EntityAudit.Changes))
            .AsString(int.MaxValue)
            .NotNullable()
            .WithColumn(nameof(EntityAudit.CreatedOnUtc))
            .AsDateTime2()
            .NotNullable()
            .WithColumn(nameof(EntityAudit.UpdatedOnUtc))
            .AsDateTime2()
            .NotNullable();
    }
}
