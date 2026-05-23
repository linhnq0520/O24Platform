using Serilog.Context;

namespace O24OpenAPI.Logging.Helpers;

public static class LogContextHelper
{
    public const string CorrelationIdKey = "CorrelationId";
    public const string ServiceNameKey = "ServiceName";

    public static IDisposable Push(string correlationId, string serviceName)
    {
        return new CombinedDisposable(
            LogContext.PushProperty(ServiceNameKey, serviceName),
            LogContext.PushProperty(CorrelationIdKey, correlationId)
        );
    }

    private sealed class CombinedDisposable : IDisposable
    {
        private readonly IDisposable[] _disposables;

        public CombinedDisposable(params IDisposable[] disposables)
        {
            _disposables = disposables;
        }

        public void Dispose()
        {
            for (int i = _disposables.Length - 1; i >= 0; i--)
            {
                _disposables[i]?.Dispose();
            }
        }
    }
}
