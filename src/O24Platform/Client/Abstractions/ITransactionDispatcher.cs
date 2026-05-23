using O24OpenAPI.Client.Scheme.Workflow;
using O24OpenAPI.Core.Domain;

namespace O24OpenAPI.Client.Abstractions;

public interface ITransactionDispatcher
{
    void Dispatch(WFScheme workflowScheme, WorkContext workContext);
}
