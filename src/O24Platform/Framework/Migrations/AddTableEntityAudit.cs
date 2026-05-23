using FluentMigrator;
using O24OpenAPI.Core.Attributes;
using O24OpenAPI.Core.Domain;
using O24OpenAPI.Data.Extensions;
using O24OpenAPI.Data.Migrations;

namespace O24OpenAPI.Framework.Migrations;

[O24OpenAPIMigration(
    "2025/12/26 02:00:00:0000000",
    "Add Table EntityAudit",
    MigrationProcessType.Installation
)]
[Environment(EnvironmentType.All)]
public class AddTableEntityAudit : AutoReversingMigration
{
    public override void Up()
    {
        if (!Schema.Table(nameof(EntityAudit)).Exists())
        {
            Create.TableFor<EntityAudit>();
            Create.Index().OnTable(nameof(EntityAudit)).OnColumn(nameof(EntityAudit.ExecutionId));
        }
    }
}
