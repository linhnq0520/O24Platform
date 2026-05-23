using O24OpenAPI.Client.Scheme.Workflow;

namespace O24OpenAPI.Framework.Services.Queue;

/// <summary>
/// The command invoker class
/// </summary>
/// <seealso cref="BaseQueue"/>
public class CommandInvoker : BaseQueue
{
    /// <summary>
    /// Invokes the command using the specified workflow
    /// </summary>
    /// <param name="workflow">The workflow</param>
    /// <returns>A task containing the wf scheme</returns>
    public async Task<WFScheme> InvokeCommand(WFScheme workflow)
    {
        return await InvokeCommandQuery(workflow);
    }
}
