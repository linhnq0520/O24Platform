using Microsoft.Extensions.DependencyInjection;
using O24OpenAPI.Client.EventBus.Abstractions;
using O24OpenAPI.Contracts.Events;
using O24OpenAPI.Contracts.Extensions;
using O24OpenAPI.Core.Abstractions;
using Serilog.Events;

namespace O24OpenAPI.Client.EventBus.Submitters;

public class RabbitMqLogSubmitter(IServiceProvider serviceProvider) : ILogSubmitter
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public async Task SubmitAsync(IEnumerable<LogEvent> logEvents)
    {
        if (!logEvents.Any())
        {
            return;
        }

        try
        {
            using IServiceScope scope = _serviceProvider.CreateScope();
            IEventBus eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();
            foreach (LogEvent logEvent in logEvents)
            {
                var dto = logEvent.ToLogEntryModel();
                var @event = new LoggingEvent() { LogEntryModel = dto };
                var cancellationToken = new CancellationToken();
                await eventBus.PublishAsync(@event, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"[FATAL][RabbitMqLogSubmitter] Failed to submit logs via RabbitMQ. Error: {ex.Message}"
            );
            await Task.Delay(5000);
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
