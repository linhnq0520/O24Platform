namespace O24OpenAPI.Core.Extensions;

/// <summary>
/// The task extensions class
/// </summary>
public static class TaskExtensions
{
    /// <summary>
    /// Gets the async result using the specified task
    /// </summary>
    /// <typeparam name="T">The </typeparam>
    /// <param name="task">The task</param>
    /// <returns>The</returns>
    public static T GetAsyncResult<T>(this Task<T> task)
    {
        return task.GetAwaiter().GetResult();
    }

    /// <summary>
    /// Gets the async result using the specified task
    /// </summary>
    /// <param name="task">The task</param>
    public static void GetAsyncResult(this Task task)
    {
        task.GetAwaiter().GetResult();
    }
}
