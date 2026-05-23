using O24OpenAPI.Framework.Models;

namespace O24OpenAPI.Framework.Services;

/// <summary>
/// The execute query service interface
/// </summary>
public interface IExecuteQueryService
{
    /// <summary>
    /// Sqls the query using the specified model
    /// </summary>
    /// /// <param name="model">The model</param>
    /// <returns>A task containing the object</returns>
    Task<object> SqlQuery(ModelWithQuery model);

    /// <summary>
    /// Executes the dml using the specified data
    /// </summary>
    /// <param name="data">The data</param>
    /// <returns>A task containing the int</returns>
    Task<int> ExecuteDML(ActionRequestModel data);
}
