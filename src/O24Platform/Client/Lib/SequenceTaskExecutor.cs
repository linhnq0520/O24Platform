using System.Collections.Concurrent;

namespace O24OpenAPI.Client.Lib;

/// <summary>
/// The sequence task executor class
/// </summary>
/// <seealso cref="IDisposable"/>
public class SequenceTaskExecutor<T> : IDisposable
{
    /// <summary>
    /// The task queue
    /// </summary>
    private readonly ConcurrentQueue<(Func<T, Task> taskFunc, T parameter)> _taskQueue;

    /// <summary>
    /// The cancellation token source
    /// </summary>
    private readonly CancellationTokenSource _cancellationTokenSource;

    /// <summary>
    /// The processing task
    /// </summary>
    private readonly Task _processingTask;

    /// <summary>
    /// The time span
    /// </summary>
    private readonly TimeSpan _pollingInterval = new TimeSpan(0, 0, 0, 0, 100);

    /// <summary>
    /// Initializes a new instance of the <see cref="SequenceTaskExecutor{T}"/> class
    /// </summary>
    public SequenceTaskExecutor()
        : this(new TimeSpan(0, 0, 0, 0, 100)) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="SequenceTaskExecutor{T}"/> class
    /// </summary>
    /// <param name="pollingInterval">The polling interval</param>
    public SequenceTaskExecutor(TimeSpan pollingInterval)
    {
        _taskQueue = new ConcurrentQueue<(Func<T, Task>, T)>();
        _cancellationTokenSource = new CancellationTokenSource();
        _pollingInterval = pollingInterval;
        _processingTask = Task.Run((Func<Task>)ProcessQueueAsync);
    }

    /// <summary>
    /// Registers the task using the specified task func
    /// </summary>
    /// <param name="taskFunc">The task func</param>
    /// <param name="parameter">The parameter</param>
    public void RegisterTask(Func<T, Task> taskFunc, T parameter)
    {
        _taskQueue.Enqueue((taskFunc, parameter));
    }

    /// <summary>
    /// Processes the queue
    /// </summary>
    private async Task ProcessQueueAsync()
    {
        while (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            if (_taskQueue.TryDequeue(out (Func<T, Task>, T) result))
            {
                try
                {
                    await result.Item1(result.Item2);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Task execution failed: " + ex.Message);
                }
            }
            else
            {
                await Task.Delay(_pollingInterval, _cancellationTokenSource.Token);
            }
        }
    }

    /// <summary>
    /// Disposes this instance
    /// </summary>
    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        try
        {
            _processingTask.Wait();
        }
        catch (AggregateException ex)
        {
            Console.WriteLine("Processing task threw an exception: " + ex.Message);
        }
        finally
        {
            _cancellationTokenSource.Dispose();
        }
    }
}
