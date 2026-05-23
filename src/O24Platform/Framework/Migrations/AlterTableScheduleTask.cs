using FluentMigrator;
using O24OpenAPI.Core.Attributes;
using O24OpenAPI.Core.Domain;
using O24OpenAPI.Data.Migrations;

namespace O24OpenAPI.Framework.Migrations;

[O24OpenAPIMigration(
    "2024/01/01 02:02:02:0000000",
    "2. Add Table LastProcessedLSN",
    MigrationProcessType.Installation
)]
[Environment(EnvironmentType.All)]
public class AlterTableScheduleTask : AutoReversingMigration
{
    public override void Up()
    {
        if (
            Schema.Table(nameof(ScheduleTask)).Exists()
            && !Schema
                .Table(nameof(ScheduleTask))
                .Column(nameof(ScheduleTask.CorrelationId))
                .Exists()
        )
        {
            Alter
                .Table(nameof(ScheduleTask))
                .AddColumn(nameof(ScheduleTask.CorrelationId))
                .AsString(255)
                .Nullable();
        }
    }
}
