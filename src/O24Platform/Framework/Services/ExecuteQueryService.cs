using O24OpenAPI.Core;
using O24OpenAPI.Core.Configuration;
using O24OpenAPI.Core.Domain;
using O24OpenAPI.Core.Extensions;
using O24OpenAPI.Core.Infrastructure;
using O24OpenAPI.Core.Utils;
using O24OpenAPI.Framework.Constants;
using O24OpenAPI.Framework.DBContext;
using O24OpenAPI.Framework.Domain;
using O24OpenAPI.Framework.Models;

namespace O24OpenAPI.Framework.Services;

/// <summary>
/// The execute query service class
/// </summary>
/// <seealso cref="IExecuteQueryService"/>
public class ExecuteQueryService(
    ServiceDBContext dbContext,
    IStoredCommandService storedCommandService
) : IExecuteQueryService
{
    /// <summary>
    /// The db context
    /// </summary>
    private readonly ServiceDBContext _dbContext = dbContext;

    /// <summary>
    /// The stored command service
    /// </summary>
    private readonly IStoredCommandService _storedCommandService = storedCommandService;

    /// <summary>
    /// Inits the audit log using the specified model
    /// </summary>
    /// <param name="model">The model</param>
    /// <param name="command">The command</param>
    /// <returns>The sql audit log</returns>
    private static SQLAuditLog InitAuditLog(ModelWithQuery model, StoredCommand command)
    {
        var sqlAuditLog = new SQLAuditLog
        {
            CommandName = command.Name,
            CommandType = command.Type,
            SourceService = model.AppCode ?? "localexport",
            ExecutedBy = !string.IsNullOrWhiteSpace(model.CurrentUsername)
                ? model.CurrentUsername
                : Singleton<O24OpenAPIConfiguration>.Instance.YourServiceID,
            Params = model.Parameters.ToSerializeSystemText(),
        };
        return sqlAuditLog;
    }

    /// <summary>
    /// Sqls the query using the specified model
    /// </summary>
    /// <param name="model">The model</param>
    /// <exception cref="O24OpenAPIException">Command [{model.CommandName}] not found</exception>
    /// <returns>A task containing the object</returns>
    public async Task<object> SqlQuery(ModelWithQuery model)
    {
        model.PageIndex ??= 0;
        model.PageSize ??= int.MaxValue;

        var command =
            await _storedCommandService.GetByName(model.CommandName)
            ?? throw new O24OpenAPIException($"Command [{model.CommandName}] not found");
        var sqlAuditLog = InitAuditLog(model, command);
        if (model.IsSearch)
        {
            return await _dbContext.ExecutePagedQueryAsync(
                pagedQuery: command.Query,
                pageIndex: model.PageIndex.Value,
                pageSize: model.PageSize.Value,
                parameters: model.Parameters,
                sQLAuditLog: sqlAuditLog
            );
        }
        return await _dbContext.ExecuteQueryAsync(
            command.Query,
            sQLAuditLog: sqlAuditLog,
            model.Parameters
        );
    }

    /// <summary>
    /// Executes the dml using the specified model
    /// </summary>
    /// <param name="model">The model</param>
    /// <exception cref="O24OpenAPIException">Id is required for delete action</exception>
    /// <returns>A task containing the int</returns>
    public async Task<int> ExecuteDML(ActionRequestModel model)
    {
        if (model.Data.TryGetValue("Id", out object id))
        {
            model.Data.Remove("Id");
        }
        var yourServiceId = Singleton<O24OpenAPIConfiguration>.Instance.YourServiceID;
        var sqlAuditLog = new SQLAuditLog
        {
            CommandName = $"{model.Action}_{model.TableName}",
            CommandType = model.Action,
            SourceService = StringUtils.Coalesce(model.AppCode, yourServiceId),
            ExecutedBy = StringUtils.Coalesce(model.CurrentUsername, yourServiceId),
            Params = System.Text.Json.JsonSerializer.Serialize(model.Data),
        };

        var o24Config = Singleton<O24OpenAPIConfiguration>.Instance;
        var schema = o24Config.GetDbSchema();
        string sql = "";
        Dictionary<string, object> parameters = [];

        switch (model.Action)
        {
            case DMLAction.Update:
                var updateClauses = model.Data.Select(k =>
                {
                    parameters[$"@{k.Key}"] = k.Value;
                    return $"[{k.Key}] = @{k.Key}";
                });
                sql =
                    $"UPDATE {schema}.{model.TableName} SET {string.Join(", ", updateClauses)} WHERE Id = @Id";
                parameters["@Id"] = id;
                break;

            case DMLAction.Insert:
                var keys = model.Data.Keys.Select(k => $"[{k}]");
                var values = model.Data.Select(k =>
                {
                    parameters[$"@{k.Key}"] = k.Value;
                    return $"@{k.Key}";
                });
                sql =
                    $"INSERT INTO {schema}.{model.TableName} ({string.Join(", ", keys)}) VALUES ({string.Join(", ", values)})";
                break;

            case DMLAction.Delete:
                if (model.Data.TryGetValue("Id", out id))
                {
                    sql = $"DELETE FROM {schema}.{model.TableName} WHERE Id = @Id";
                    parameters["@Id"] = id;
                }
                else
                {
                    throw new O24OpenAPIException("Id is required for delete action");
                }
                break;
        }
        return await _dbContext.ExecuteDML(sql, sqlAuditLog, parameters);
    }
}
