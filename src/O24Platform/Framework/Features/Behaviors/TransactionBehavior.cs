using LinKit.Core.Cqrs;
using O24OpenAPI.Data;

namespace O24OpenAPI.Framework.Features.Behaviors;

[CqrsBehavior(typeof(ICommand), 0)]
internal class TransactionBehavior<TRequest, TResponse>(
    IDataConnectionFactory dataConnectionFactory
) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand<TResponse>
{
    public async Task<TResponse> HandleAsync(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        bool ownsTransaction = !dataConnectionFactory.HasActiveTransaction;
        if (ownsTransaction)
        {
            await dataConnectionFactory.BeginTransactionAsync(cancellationToken);
        }

        try
        {
            TResponse result = await next();
            if (ownsTransaction)
            {
                await dataConnectionFactory.CommitAsync();
            }

            return result;
        }
        catch
        {
            if (ownsTransaction)
            {
                await dataConnectionFactory.RollbackAsync();
            }

            throw;
        }
    }
}
