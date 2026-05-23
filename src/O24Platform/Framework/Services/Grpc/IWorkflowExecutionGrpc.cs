using O24OpenAPI.Client.Workflow;

namespace O24OpenAPI.Framework.Services.Grpc;

public interface IWorkflowExecutionGrpc
{
    Task<WorkflowResponse> ExecuteWorkflowByGrpc(string input);
}
