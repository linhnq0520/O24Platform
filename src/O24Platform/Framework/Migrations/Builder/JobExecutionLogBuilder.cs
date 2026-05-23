using FluentMigrator.Builders.Create.Table;
using O24OpenAPI.Data.Mapping.Builders;
using O24OpenAPI.Framework.Domain;

namespace O24OpenAPI.Framework.Migrations.Builder;

/// <summary>
/// The 24 open api service builder class
/// </summary>
/// <seealso cref="O24OpenAPIEntityBuilder{JobExecutionHistory}"/>
public class JobExecutionLogBuilder : O24OpenAPIEntityBuilder<JobExecutionLog>
{
    /// <summary>
    /// Maps the entity using the specified table
    /// </summary>
    /// <param name="table">The table</param>
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(JobExecutionLog.JobName))
            .AsString(255)
            .NotNullable()
            .WithColumn(nameof(JobExecutionLog.StartTime))
            .AsDateTime2()
            .NotNullable()
            .WithColumn(nameof(JobExecutionLog.EndTime))
            .AsDateTime2()
            .Nullable()
            .WithColumn(nameof(JobExecutionLog.IsSuccess))
            .AsBoolean()
            .NotNullable()
            .WithColumn(nameof(JobExecutionLog.ErrorMessage))
            .AsString(int.MaxValue)
            .Nullable()
            .WithColumn(nameof(JobExecutionLog.EmbeddedData))
            .AsString(int.MaxValue)
            .Nullable();
    }
}
