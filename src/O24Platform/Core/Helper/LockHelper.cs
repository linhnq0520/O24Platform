namespace O24OpenAPI.Core.Helper;

/// <summary>
/// The lock helper class
/// </summary>
public class LockHelper
{
    /// <summary>
    /// Runs the with lock using the specified semaphore lock
    /// </summary>
    /// <param name="semaphoreLock">The semaphore lock</param>
    /// <param name="action">The action</param>
    public static async Task RunWithLockAsync(SemaphoreSlim semaphoreLock, Func<Task> action)
    {
        await semaphoreLock.WaitAsync();
        try
        {
            await action();
        }
        finally
        {
            semaphoreLock.Release();
        }
    }

    /// <summary>
    /// Runs the with lock async result using the specified semaphore lock
    /// </summary>
    /// <typeparam name="T">The </typeparam>
    /// <param name="semaphoreLock">The semaphore lock</param>
    /// <param name="action">The action</param>
    /// <returns>A task containing the</returns>
    public static async Task<T> RunWithLockAsyncResult<T>(
        SemaphoreSlim semaphoreLock,
        Func<Task<T>> action
    )
    {
        await semaphoreLock.WaitAsync();
        try
        {
            return await action();
        }
        finally
        {
            semaphoreLock.Release();
        }
    }
}
