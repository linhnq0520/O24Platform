using FluentMigrator.Builders.Create.Table;
using O24OpenAPI.Data.Mapping.Builders;
using O24OpenAPI.Framework.Domain;

namespace O24OpenAPI.Framework.Migrations.Builder;

/// <summary>
/// The session manager builder class
/// </summary>
/// <seealso cref="O24OpenAPIEntityBuilder{SessionManager}"/>
public class SessionManagerBuilder : O24OpenAPIEntityBuilder<SessionManager>
{
    /// <summary>
    /// Maps the entity using the specified table
    /// </summary>
    /// <param name="table">The table</param>
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(SessionManager.Token))
            .AsString(int.MaxValue)
            .NotNullable()
            .WithColumn(nameof(SessionManager.ExpireAt))
            .AsDateTimeOffset()
            .NotNullable()
            .WithColumn(nameof(SessionManager.Identifier))
            .AsString(255)
            .NotNullable()
            .WithColumn(nameof(SessionManager.IsRevoked))
            .AsBoolean()
            .Nullable()
            .WithColumn(nameof(SessionManager.RevokeReason))
            .AsString(1000)
            .Nullable()
            .WithColumn(nameof(SessionManager.CreatedOnUtc))
            .AsDateTime2()
            .Nullable();
    }
}
