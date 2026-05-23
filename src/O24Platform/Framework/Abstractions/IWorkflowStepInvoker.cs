using LinKit.Core.Cqrs;
using O24OpenAPI.Client.Scheme.Workflow;

namespace O24OpenAPI.Framework.Abstractions;

public interface IWorkflowStepInvoker
{
    Task<object> InvokeAsync(
        string stepCode,
        WFScheme scheme,
        IMediator mediator,
        CancellationToken cancellationToken
    );
}
