using O24OpenAPI.Client.Scheme.Workflow;

namespace O24OpenAPI.Client.Lib.InternalStepCodeProcess;

/// <summary>
/// The client echo processor class
/// </summary>
/// <seealso cref="IInternalStepCodeProcessor"/>
public class ClientEchoProcessor : IInternalStepCodeProcessor
{
    /// <summary>
    /// Processes the p queue client
    /// </summary>
    /// <param name="pQueueClient">The queue client</param>
    /// <param name="pWorkflow">The workflow</param>
    /// <returns>A task containing the bool</returns>
    public async Task<bool> Process(QueueClient pQueueClient, WFScheme pWorkflow)
    {
        try
        {
            pWorkflow.response.data =
                $"This is an echo message, reply from the service [{pQueueClient.ServiceInfo.service_code}] at [UTC{DateTime.UtcNow}]";
            await pQueueClient.ReplyWorkflow(pWorkflow);
        }
        catch (Exception)
        {
            return false;
        }
        return await Task.FromResult(true);
    }
}
