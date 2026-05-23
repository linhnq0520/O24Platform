using FluentMigrator;
using O24OpenAPI.Core.Attributes;
using O24OpenAPI.Core.Configuration;
using O24OpenAPI.Core.Domain;
using O24OpenAPI.Core.Domain.Localization;
using O24OpenAPI.Core.Domain.Logging;
using O24OpenAPI.Data.Extensions;
using O24OpenAPI.Data.Migrations;
using O24OpenAPI.Framework.Domain;

namespace O24OpenAPI.Framework.Migrations;

/// <summary>
/// The schema migration class
/// </summary>
/// <seealso cref="AutoReversingMigration"/>
[O24OpenAPIMigration(
    "2020/01/01 01:02:00:0000000",
    "1.Create system tables framework",
    MigrationProcessType.Installation
)]
[Environment(EnvironmentType.All)]
public class SchemaMigration : AutoReversingMigration
{
    /// <summary>
    /// Ups this instance
    /// </summary>
    public override void Up()
    {
        if (!Schema.Table(nameof(Log)).Exists())
        {
            Create.TableFor<Log>();
        }

        if (!Schema.Table(nameof(Language)).Exists())
        {
            Create.TableFor<Language>();
        }

        if (!Schema.Table(nameof(LocaleStringResource)).Exists())
        {
            Create.TableFor<LocaleStringResource>();
        }

        if (!Schema.Table(nameof(Setting)).Exists())
        {
            Create.TableFor<Setting>();
        }

        if (!Schema.Table(nameof(ScheduleTask)).Exists())
        {
            Create.TableFor<ScheduleTask>();
        }

        //if (!Schema.Table(nameof(Transaction)).Exists())
        //{
        //Create.TableFor<Transaction>();
        //}

        if (!Schema.Table(nameof(TransactionDetails)).Exists())
        {
            Create.TableFor<TransactionDetails>();
        }

        if (!Schema.Table(nameof(EntityField)).Exists())
        {
            Create.TableFor<EntityField>();
        }

        if (!Schema.Table(nameof(StoredCommand)).Exists())
        {
            Create.TableFor<StoredCommand>();

            if (!Schema.Table(nameof(StoredCommand)).Constraint("UC_StoredCommand").Exists())
            {
                Create
                    .UniqueConstraint("UC_StoredCommand")
                    .OnTable(nameof(StoredCommand))
                    .Columns(nameof(StoredCommand.Name));
            }
        }

        if (!Schema.Table(nameof(SessionManager)).Exists())
        {
            Create.TableFor<SessionManager>();
        }

        if (!Schema.Table(nameof(O24OpenAPIService)).Exists())
        {
            Create.TableFor<O24OpenAPIService>();
            Create
                .UniqueConstraint("UC_O24OpenAPIService")
                .OnTable(nameof(O24OpenAPIService))
                .Columns(nameof(O24OpenAPIService.StepCode));
        }

        if (!Schema.Table(nameof(SQLAuditLog)).Exists())
        {
            Create.TableFor<SQLAuditLog>();
        }

        if (!Schema.Table(nameof(EntityAudit)).Exists())
        {
            Create.TableFor<EntityAudit>();
            Create.Index().OnTable(nameof(EntityAudit)).OnColumn(nameof(EntityAudit.ExecutionId));
        }
    }
}

[O24OpenAPIMigration(
    "2020/01/01 01:05:05:0000000",
    "Update table O24OpenAPIService",
    MigrationProcessType.Installation
)]
[Environment(EnvironmentType.All)]
public class SchemaMigration1 : AutoReversingMigration
{
    /// <summary>
    /// Ups this instance
    /// </summary>
    public override void Up()
    {
        if (
            Schema.Table(nameof(O24OpenAPIService)).Exists()
            && !Schema
                .Table(nameof(O24OpenAPIService))
                .Column(nameof(O24OpenAPIService.IsModuleExecute))
                .Exists()
        )
        {
            Alter
                .Table(nameof(O24OpenAPIService))
                .AddColumn(nameof(O24OpenAPIService.IsModuleExecute))
                .AsBoolean()
                .WithDefaultValue(false);
        }
    }
}
