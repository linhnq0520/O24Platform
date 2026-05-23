namespace O24OpenAPI.Client.Lib;

/// <summary>
/// The scheduler service class
/// </summary>
internal class SchedulerService
{
    /// <summary>
    /// The instance
    /// </summary>
    private static SchedulerService? _instance;

    /// <summary>
    /// The timer
    /// </summary>
    private Dictionary<string, Timer> timers = new Dictionary<string, Timer>();

    /// <summary>
    /// Gets the value of the instance
    /// </summary>
    private static SchedulerService Instance => _instance ?? (_instance = new SchedulerService());

    /// <summary>
    /// Initializes a new instance of the <see cref="SchedulerService"/> class
    /// </summary>
    private SchedulerService() { }

    /// <summary>
    /// Schedules the task using the specified task id
    /// </summary>
    /// <param name="taskID">The task id</param>
    /// <param name="firstTime">The first time</param>
    /// <param name="intervalInSeconds">The interval in seconds</param>
    /// <param name="task">The task</param>
    public void ScheduleTask(
        string taskID,
        DateTime firstTime,
        double intervalInSeconds,
        Action task
    )
    {
        DateTime now = DateTime.Now;
        DateTime dateTime = firstTime;
        if (firstTime < now)
        {
            dateTime = new DateTime(
                now.Year,
                now.Month,
                now.Day,
                firstTime.Hour,
                firstTime.Minute,
                firstTime.Second,
                firstTime.Millisecond
            );
            dateTime.AddDays(1.0);
        }
        TimeSpan timeSpan = dateTime - now;
        if (timeSpan < TimeSpan.Zero)
        {
            timeSpan = TimeSpan.Zero;
        }
        StopSchedule(taskID);
        Timer value = new Timer(
            delegate
            {
                task();
            },
            null,
            timeSpan,
            TimeSpan.FromSeconds(intervalInSeconds)
        );
        timers.Add(taskID, value);
    }

    /// <summary>
    /// Stops the schedule using the specified p task id
    /// </summary>
    /// <param name="pTaskID">The task id</param>
    public void StopSchedule(string pTaskID)
    {
        if (timers.ContainsKey(pTaskID))
        {
            timers[pTaskID].Dispose();
            timers.Remove(pTaskID);
        }
    }

    /// <summary>
    /// Clears the all schedules
    /// </summary>
    /// <returns>The count</returns>
    public static int ClearAllSchedules()
    {
        int count = Instance.timers.Count;
        foreach (string key in Instance.timers.Keys)
        {
            Instance.StopSchedule(key);
        }
        return count;
    }

    /// <summary>
    /// Intervals the in seconds using the specified task id
    /// </summary>
    /// <param name="taskID">The task id</param>
    /// <param name="intervalInSeconds">The interval in seconds</param>
    /// <param name="task">The task</param>
    /// <param name="FirstDateTimeToRun">The first date time to run</param>
    public static void IntervalInSeconds(
        string taskID,
        double intervalInSeconds,
        Action task,
        DateTime FirstDateTimeToRun
    )
    {
        Instance.ScheduleTask(taskID, FirstDateTimeToRun, intervalInSeconds, task);
    }

    /// <summary>
    /// Intervals the in seconds using the specified task id
    /// </summary>
    /// <param name="taskID">The task id</param>
    /// <param name="intervalInSeconds">The interval in seconds</param>
    /// <param name="task">The task</param>
    public static void IntervalInSeconds(string taskID, double intervalInSeconds, Action task)
    {
        Instance.ScheduleTask(taskID, DateTime.Now, intervalInSeconds, task);
    }
}
