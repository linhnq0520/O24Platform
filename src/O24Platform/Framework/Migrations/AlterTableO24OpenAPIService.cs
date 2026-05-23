using FluentMigrator;
using O24OpenAPI.Core.Attributes;
using O24OpenAPI.Data.Migrations;
using O24OpenAPI.Framework.Domain;

namespace O24OpenAPI.Framework.Migrations;

[O24OpenAPIMigration(
    "2024/01/01 02:02:03:0000000",
    "2. Add Table LastProcessedLSN",
    MigrationProcessType.Installation
)]
[Environment(EnvironmentType.All)]
public class AlterTableO24OpenAPIService : AutoReversingMigration
{
    public override void Up()
    {
        if (
            Schema.Table(nameof(O24OpenAPIService)).Exists()
            && !Schema
                .Table(nameof(O24OpenAPIService))
                .Column(nameof(O24OpenAPIService.IsAutoReverse))
                .Exists()
        )
        {
            Alter
                .Table(nameof(O24OpenAPIService))
                .AddColumn(nameof(O24OpenAPIService.IsAutoReverse))
                .AsBoolean()
                .Nullable()
                .WithDefaultValue(true);
        }

        if (
            Schema.Table(nameof(O24OpenAPIService)).Exists()
            && !Schema
                .Table(nameof(O24OpenAPIService))
                .Column(nameof(O24OpenAPIService.MediatorKey))
                .Exists()
        )
        {
            Alter
                .Table(nameof(O24OpenAPIService))
                .AddColumn(nameof(O24OpenAPIService.MediatorKey))
                .AsString(255)
                .NotNullable()
                .WithDefaultValue(string.Empty);
        }
    }
}
