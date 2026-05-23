using FluentMigrator;
using O24OpenAPI.Core.Attributes;
using O24OpenAPI.Core.Configuration;
using O24OpenAPI.Core.Infrastructure;
using O24OpenAPI.Data.Extensions;
using O24OpenAPI.Data.Migrations;
using O24OpenAPI.Framework.Domain;

namespace O24OpenAPI.Framework.Migrations;

/// <summary>
/// The add table last processed lsn class
/// </summary>
/// <seealso cref="AutoReversingMigration"/>
[O24OpenAPIMigration(
    "2024/01/01 02:00:00:0000000",
    "2. Add Table LastProcessedLSN",
    MigrationProcessType.Installation
)]
[Environment(EnvironmentType.All)]
public class AddTableLastProcessedLSN : AutoReversingMigration
{
    /// <summary>
    /// Ups this instance
    /// </summary>
    public override void Up()
    {
        string cdcSchema = Singleton<O24OpenAPIConfiguration>.Instance.YourCDCSchema;

        if (
            !string.IsNullOrEmpty(cdcSchema)
            && Schema.Schema(cdcSchema).Exists()
            && !Schema.Schema(cdcSchema).Table(nameof(LastProcessedLSN)).Exists()
        )
        {
            Create.TableFor<LastProcessedLSN>();

            Create
                .Index()
                .OnTable(nameof(LastProcessedLSN))
                .InSchema(cdcSchema)
                .OnColumn(nameof(LastProcessedLSN.TableName));
        }
    }
}
