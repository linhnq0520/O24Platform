using FluentMigrator;
using O24OpenAPI.Core.Attributes;
using O24OpenAPI.Data.Extensions;
using O24OpenAPI.Data.Migrations;
using O24OpenAPI.Framework.Domain;

namespace O24OpenAPI.Framework.Migrations;

[O24OpenAPIMigration(
    "2025/04/05 15:08:00:0000000",
    "Add Table JobExecutionLog",
    MigrationProcessType.Installation
)]
[Environment(EnvironmentType.All)]
public class AddTableJobExecutionLog : AutoReversingMigration
{
    public override void Up()
    {
        if (!Schema.Table(nameof(JobExecutionLog)).Exists())
        {
            Create.TableFor<JobExecutionLog>();
            Create
                .Index()
                .OnTable(nameof(JobExecutionLog))
                .OnColumn(nameof(JobExecutionLog.JobName));
        }
    }
}
