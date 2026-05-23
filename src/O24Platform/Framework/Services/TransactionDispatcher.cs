using Microsoft.Extensions.DependencyInjection;
using O24OpenAPI.Client.Abstractions;
using O24OpenAPI.Client.Scheme.Workflow;
using O24OpenAPI.Core.Domain;
using O24OpenAPI.Core.Infrastructure;
using O24OpenAPI.Framework.Services.Queue;
using System.Threading.Channels;

namespace O24OpenAPI.Framework.Services;

public sealed class TransactionDispatcherModel(WFScheme workflowScheme, WorkContext workContext)
{
    public WFScheme WorkflowScheme { get; set; } = workflowScheme;
    public WorkContext WorkContext { get; set; } = workContext;
}

public class TransactionDispatcher : ITransactionDispatcher, IDisposable
{
    private readonly Channel<TransactionDispatcherModel>[] _internalQueues;
    private readonly int _poolSize;
    private readonly CancellationTokenSource _cts = new();

    public TransactionDispatcher(int poolSize = 32)
    {
        _poolSize = poolSize;
        _internalQueues = new Channel<TransactionDispatcherModel>[_poolSize];

        for (int i = 0; i < _poolSize; i++)
        {
            _internalQueues[i] = Channel.CreateUnbounded<TransactionDispatcherModel>();
            int index = i;
            Task.Run(() => StartWorker(index, _cts.Token));
        }
    }

    public void Dispatch(WFScheme workflowScheme, WorkContext workContext)
    {
        object workerId = workflowScheme.GetWorkerId();
        int queueIndex = (workerId.GetHashCode() & 0x7FFFFFFF) % _poolSize;

        _internalQueues[queueIndex]
            .Writer.TryWrite(new TransactionDispatcherModel(workflowScheme, workContext));
    }

    private async Task StartWorker(int index, CancellationToken ct)
    {
        await foreach (
            TransactionDispatcherModel transactionDispatcherModel in _internalQueues[index].Reader.ReadAllAsync(ct)
        )
        {
            using IServiceScope scope = EngineContext.Current.ServiceScopeFactory.CreateScope();
            AsyncScope.Scope = scope;
            EngineContext
                .Current.ResolveRequired<WorkContext>()
                .SetWorkContext(transactionDispatcherModel.WorkContext);
            await QueueContext.WorkflowDelivering(transactionDispatcherModel.WorkflowScheme);
            AsyncScope.Clear();
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}
