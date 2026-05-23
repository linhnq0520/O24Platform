using O24OpenAPI.Client.Scheme.Workflow;
using O24OpenAPI.Framework.Models;

namespace O24OpenAPI.Framework.Services.Queue;

/// <summary>
/// The stored procedure invoke service class
/// </summary>
/// <seealso cref="BaseQueueService"/>
public class StoredProcedureInvokeService : BaseQueueService
{
    /// <summary>
    /// Invokes the stored procedure using the specified workflow
    /// </summary>
    /// <param name="workflow">The workflow</param>
    /// <param name="StoredProcedureName">The stored procedure name</param>
    /// <returns>A task containing the workflow scheme</returns>
    public async Task<WorkflowScheme> InvokeStoredProcedure(
        WorkflowScheme workflow,
        string StoredProcedureName
    )
    {
        return await Invoke2<BaseTransactionModel>(workflow, StoredProcedureName);
    }
}
