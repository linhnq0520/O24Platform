using O24OpenAPI.Core.Domain;

namespace O24OpenAPI.Framework.Services.ScheduleTasks;

/// <summary>
/// The schedule task service interface
/// </summary>
public interface IScheduleTaskService
{
    /// <summary>
    /// Deletes the task
    /// </summary>
    /// <param name="task">The task</param>
    Task Delete(ScheduleTask task);

    /// <summary>
    /// Gets the by id using the specified task id
    /// </summary>
    /// <param name="taskId">The task id</param>
    /// <returns>A task containing the schedule task</returns>
    Task<ScheduleTask> GetById(int taskId);

    /// <summary>
    /// Gets the by type using the specified type
    /// </summary>
    /// <param name="type">The type</param>
    /// <returns>A task containing the schedule task</returns>
    Task<ScheduleTask> GetByType(string type);

    /// <summary>
    /// Gets the all using the specified show hidden
    /// </summary>
    /// <param name="showHidden">The show hidden</param>
    /// <returns>A task containing a list of schedule task</returns>
    Task<IList<ScheduleTask>> GetAll(bool showHidden = false);

    /// <summary>
    /// Inserts the task
    /// </summary>
    /// <param name="task">The task</param>
    Task Insert(ScheduleTask task);

    /// <summary>
    /// Updates the task
    /// </summary>
    /// <param name="task">The task</param>
    Task Update(ScheduleTask task);
}
