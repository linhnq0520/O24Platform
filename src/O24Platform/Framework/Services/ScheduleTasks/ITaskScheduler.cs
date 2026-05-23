namespace O24OpenAPI.Framework.Services.ScheduleTasks;

/// <summary>
/// The task scheduler interface
/// </summary>
public interface ITaskScheduler
{
    /// <summary>Initializes task scheduler</summary>
    Task Initialize();

    /// <summary>Reset the scheduler</summary>
    void ReInitialize();

    /// <summary>Starts the task scheduler</summary>
    void StartScheduler();

    /// <summary>Stops the task scheduler</summary>
    void StopScheduler();
}
