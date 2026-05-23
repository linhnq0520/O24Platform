using FluentMigrator;
using O24OpenAPI.Core.Attributes;
using O24OpenAPI.Data.Extensions;
using O24OpenAPI.Data.Migrations;
using O24OpenAPI.Framework.Domain;

namespace O24OpenAPI.Framework.Migrations;

/// <summary>
/// The schema migration class
/// </summary>
/// <seealso cref="AutoReversingMigration"/>
[O24OpenAPIMigration(
    "2025/12/05 01:00:00:0000000",
    "1.Create TableKeyConfig tables framework",
    MigrationProcessType.Installation
)]
[Environment(EnvironmentType.All)]
public class TableKeyConfigMigration : AutoReversingMigration
{
    /// <summary>
    /// Ups this instance
    /// </summary>
    public override void Up()
    {
        if (!Schema.Table(nameof(TableKeyConfig)).Exists())
        {
            Create.TableFor<TableKeyConfig>();
            Create
                .Index()
                .OnTable(nameof(TableKeyConfig))
                .OnColumn(nameof(TableKeyConfig.KeyColumn));
            Create
                .Index()
                .OnTable(nameof(TableKeyConfig))
                .OnColumn(nameof(TableKeyConfig.TableName));
            Create
                .Index()
                .OnTable(nameof(TableKeyConfig))
                .OnColumn(nameof(TableKeyConfig.SchemaName));
        }
    }
}
