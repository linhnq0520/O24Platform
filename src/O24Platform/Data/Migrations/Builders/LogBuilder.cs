using FluentMigrator.Builders.Create.Table;
using O24OpenAPI.Core.Domain.Logging;
using O24OpenAPI.Data.Attributes;
using O24OpenAPI.Data.Extensions;
using O24OpenAPI.Data.Mapping.Builders;

namespace O24OpenAPI.Data.Migrations.Builders;

[DatabaseType(DataProviderType.Oracle)]
public class LogBuilder : O24OpenAPIEntityBuilder<Log>
{
    /// <summary>
    /// Maps the entity using the specified table
    /// </summary>
    /// <param name="table">The table</param>
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(Log.LogLevelId))
            .AsInt32()
            .NotNullable()
            .WithColumn(nameof(Log.ShortMessage))
            .AsNCLOB()
            .NotNullable()
            .WithColumn(nameof(Log.FullMessage))
            .AsNCLOB()
            .Nullable()
            .WithColumn(nameof(Log.IpAddress))
            .AsString(100)
            .Nullable()
            .WithColumn(nameof(Log.UserId))
            .AsInt32()
            .Nullable()
            .WithColumn(nameof(Log.PageUrl))
            .AsNCLOB()
            .Nullable()
            .WithColumn(nameof(Log.ReferredUrl))
            .AsNCLOB()
            .Nullable()
            .WithColumn(nameof(Log.CreatedOnUtc))
            .AsDateTime2()
            .NotNullable();
    }
}
