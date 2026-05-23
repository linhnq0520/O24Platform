using FluentMigrator.Builders.Create.Table;
using O24OpenAPI.Core.Domain;
using O24OpenAPI.Data.Mapping.Builders;

namespace O24OpenAPI.Data.Migrations.Builders;

public class ScheduleTaskBuilder : O24OpenAPIEntityBuilder<ScheduleTask>
{
    /// <summary>
    /// Maps the entity using the specified table
    /// </summary>
    /// <param name="table">The table</param>
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(ScheduleTask.Name))
            .AsString(500)
            .NotNullable()
            .WithColumn(nameof(ScheduleTask.CorrelationId))
            .AsString(255)
            .Nullable()
            .WithColumn(nameof(ScheduleTask.Seconds))
            .AsInt32()
            .NotNullable()
            .WithColumn(nameof(ScheduleTask.Type))
            .AsString(500)
            .NotNullable()
            .WithColumn(nameof(ScheduleTask.LastEnabledUtc))
            .AsDateTime()
            .Nullable()
            .WithColumn(nameof(ScheduleTask.Enabled))
            .AsBoolean()
            .NotNullable()
            .WithColumn(nameof(ScheduleTask.StopOnError))
            .AsBoolean()
            .NotNullable()
            .WithColumn(nameof(ScheduleTask.LastStartUtc))
            .AsDateTime()
            .Nullable()
            .WithColumn(nameof(ScheduleTask.LastEndUtc))
            .AsDateTime()
            .Nullable()
            .WithColumn(nameof(ScheduleTask.LastSuccessUtc))
            .AsDateTime()
            .Nullable();
    }
}
