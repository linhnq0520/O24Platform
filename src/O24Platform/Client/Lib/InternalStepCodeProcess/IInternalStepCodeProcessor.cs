using O24OpenAPI.Client.Scheme.Workflow;

namespace O24OpenAPI.Client.Lib.InternalStepCodeProcess;

/// <summary>
/// The internal step code processor interface
/// </summary>
public interface IInternalStepCodeProcessor
{
    /// <summary>
    /// Processes the p queue client
    /// </summary>
    /// <param name="pQueueClient">The queue client</param>
    /// <param name="pWorkflow">The workflow</param>
    /// <returns>A task containing the bool</returns>
    Task<bool> Process(QueueClient pQueueClient, WFScheme pWorkflow);
}
