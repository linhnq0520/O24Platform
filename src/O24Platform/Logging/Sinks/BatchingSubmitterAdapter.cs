using O24OpenAPI.Core.Abstractions;
using Serilog.Events;
using Serilog.Sinks.PeriodicBatching;

namespace O24OpenAPI.Logging.Sinks;

public class BatchingSubmitterAdapter(ILogSubmitter submitter) : IBatchedLogEventSink
{
    private readonly ILogSubmitter _submitter = submitter;

    public void Dispose()
    {
        _submitter.Dispose();
    }

    public async Task EmitBatchAsync(IEnumerable<LogEvent> batch)
    {
        await _submitter.SubmitAsync(batch);
    }

    public Task OnEmptyBatchAsync()
    {
        return Task.CompletedTask;
    }
}
