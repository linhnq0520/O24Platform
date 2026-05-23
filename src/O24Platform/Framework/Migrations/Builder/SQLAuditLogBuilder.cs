using FluentMigrator.Builders.Create.Table;
using O24OpenAPI.Data.Mapping.Builders;
using O24OpenAPI.Framework.Domain;

namespace O24OpenAPI.Framework.Migrations.Builder;

/// <summary>
/// The sql audit log builder class
/// </summary>
/// <seealso cref="O24OpenAPIEntityBuilder{SQLAuditLog}"/>
public class SQLAuditLogBuilder : O24OpenAPIEntityBuilder<SQLAuditLog>
{
    /// <summary>
    /// Maps the entity using the specified table
    /// </summary>
    /// <param name="table">The table</param>
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(SQLAuditLog.CommandName))
            .AsString(255)
            .NotNullable()
            .WithColumn(nameof(SQLAuditLog.CommandType))
            .AsString(255)
            .NotNullable()
            .WithColumn(nameof(SQLAuditLog.Params))
            .AsString(int.MaxValue)
            .Nullable()
            .WithColumn(nameof(SQLAuditLog.Query))
            .AsString(int.MaxValue)
            .Nullable()
            .WithColumn(nameof(SQLAuditLog.ExecutionTime))
            .AsDateTime2()
            .NotNullable()
            .WithColumn(nameof(SQLAuditLog.Status))
            .AsString(50)
            .NotNullable()
            .WithColumn(nameof(SQLAuditLog.ErrorMessage))
            .AsString(1000)
            .Nullable()
            .WithColumn(nameof(SQLAuditLog.Result))
            .AsString(int.MaxValue)
            .Nullable()
            .WithColumn(nameof(SQLAuditLog.ExecutedBy))
            .AsString(255)
            .NotNullable()
            .WithColumn(nameof(SQLAuditLog.SourceService))
            .AsString(255)
            .NotNullable();
    }
}
