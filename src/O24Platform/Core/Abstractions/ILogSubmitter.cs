using Serilog.Events;

namespace O24OpenAPI.Core.Abstractions;

public interface ILogSubmitter : IDisposable
{
    Task SubmitAsync(IEnumerable<LogEvent> logEvents);
}
