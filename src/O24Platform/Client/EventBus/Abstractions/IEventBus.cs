using O24OpenAPI.Contracts.Events;

namespace O24OpenAPI.Client.EventBus.Abstractions;

public interface IEventBus
{
    Task PublishAsync(IntegrationEvent @event, CancellationToken cancellationToken = default);
}
