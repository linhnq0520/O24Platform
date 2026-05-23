using Microsoft.Extensions.DependencyInjection;

namespace O24OpenAPI.Client.EventBus.Abstractions;

public interface IEventBusBuilder
{
    public IServiceCollection Services { get; }
}
