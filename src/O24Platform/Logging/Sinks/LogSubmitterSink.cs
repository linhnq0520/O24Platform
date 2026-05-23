using O24OpenAPI.Core.Abstractions;
using Serilog.Core;
using Serilog.Events;

namespace O24OpenAPI.Logging.Sinks;

public class LogSubmitterSink(ILogSubmitter submitter, IFormatProvider? formatProvider = null)
    : ILogEventSink,
        IDisposable
{
    private readonly ILogSubmitter _submitter = submitter;
    private readonly IFormatProvider? _formatProvider = formatProvider;

    public void Emit(LogEvent logEvent) { }

    public void Dispose()
    {
        _submitter.Dispose();
    }
}
