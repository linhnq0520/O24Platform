using Microsoft.Extensions.DependencyInjection;
using O24OpenAPI.Core.Infrastructure;
using O24OpenAPI.Data;
using O24OpenAPI.Framework.Services.Logging;
using O24OpenAPI.Logging.Helpers;
using System.Net.Http;

namespace O24OpenAPI.Framework.Services.ScheduleTasks;

public class TaskScheduler(IServiceProvider serviceProvider) : ITaskScheduler
{
    protected static readonly List<TaskThread> _taskThreads = [];
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public async Task Initialize()
    {
        using IServiceScope scope = _serviceProvider.CreateScope();
        if (!DataSettingsManager.IsDatabaseInstalled() || _taskThreads.Count > 0)
        {
            return;
        }
        IScheduleTaskService scheduleTaskService =
            scope.ServiceProvider.GetService<IScheduleTaskService>();
        IList<ScheduleTask> source = await scheduleTaskService.GetAll();
        List<ScheduleTask> scheduleTasks = [.. source.OrderBy(x => x.Seconds)];
        foreach (ScheduleTask scheduleTask in scheduleTasks)
        {
            TaskThread taskThread = new(scheduleTask, new int?(60), _serviceProvider)
            {
                Seconds = scheduleTask.Seconds,
            };
            DateTime? lastStart = scheduleTask.LastStartUtc;
            TimeSpan timeSpan;
            if (lastStart.HasValue)
            {
                DateTime utcNow = DateTime.UtcNow;
                timeSpan = new TimeSpan?(utcNow - lastStart.GetValueOrDefault()).Value;
                double secondsLeft = timeSpan.TotalSeconds;
                taskThread.InitSeconds =
                    secondsLeft < scheduleTask.Seconds
                        ? (int)(scheduleTask.Seconds - secondsLeft) + 1
                        : 0;
            }
            else
            {
                lastStart = scheduleTask.LastEnabledUtc;
                if (lastStart.HasValue)
                {
                    DateTime utcNow = DateTime.UtcNow;
                    timeSpan = new TimeSpan?(utcNow - lastStart.GetValueOrDefault()).Value;
                    double secondsLeft = timeSpan.TotalSeconds;
                    taskThread.InitSeconds =
                        secondsLeft < scheduleTask.Seconds
                            ? (int)(scheduleTask.Seconds - secondsLeft) + 1
                            : 0;
                }
                else
                {
                    taskThread.InitSeconds = 0;
                }
            }

            _taskThreads.Add(taskThread);
            taskThread = null;
        }
    }

    public void ReInitialize() => _taskThreads.Clear();

    public void StartScheduler()
    {
        foreach (TaskThread taskThread in _taskThreads)
        {
            taskThread.InitTimer();
        }
    }

    public void StopScheduler()
    {
        foreach (TaskThread taskThread in _taskThreads)
        {
            taskThread.Dispose();
        }
    }

    #region TaskThread

    protected class TaskThread(ScheduleTask task, int? timeout, IServiceProvider serviceProvider)
        : IDisposable
    {
        protected readonly string _scheduleTaskUrl;
        protected readonly ScheduleTask _scheduleTask = task;
        protected readonly int? _timeout = timeout;
        protected Timer _timer;
        protected bool _disposed;
        internal static IHttpClientFactory HttpClientFactory { get; set; }
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        public int Seconds { get; set; } = 600;
        public int InitSeconds { get; set; }
        public DateTime StartedUtc { get; private set; }
        public bool IsRunning { get; private set; }
        public int Interval
        {
            get
            {
                int interval = Seconds * 1000;
                if (interval <= 0)
                {
                    interval = int.MaxValue;
                }

                return interval;
            }
        }
        public int InitInterval
        {
            get
            {
                int initInterval = InitSeconds * 1000;
                if (initInterval <= 0)
                {
                    initInterval = 0;
                }

                return initInterval;
            }
        }
        public bool RunOnlyOnce { get; set; }
        public bool IsStarted => _timer != null;
        public bool IsDisposed => _disposed;

        private static async Task StartScheduleTask(string taskType, IServiceScope serviceScope)
        {
            IScheduleTaskService scheduleTaskService =
                serviceScope.ServiceProvider.GetService<IScheduleTaskService>();
            ScheduleTask task = await scheduleTaskService.GetByType(taskType);
            if (task != null)
            {
                IScheduleTaskRunner taskRunner =
                    serviceScope.ServiceProvider.GetService<IScheduleTaskRunner>();
                await taskRunner.Execute(task, serviceScope);
            }
        }

        private void RunTask()
        {
            if (Seconds <= 0)
            {
                return;
            }

            StartedUtc = DateTime.UtcNow;
            IsRunning = true;

            _ = Task.Run(async () =>
            {
                using IServiceScope scope = _serviceProvider.CreateScope();
                AsyncScope.Scope = scope;
                try
                {
                    await ScheduleTaskLogHelper.LogProcess(
                        _scheduleTask.Name,
                        _scheduleTask.CorrelationId ?? Guid.NewGuid().ToString(),
                        async () =>
                        {
                            await StartScheduleTask(_scheduleTask.Type, scope);
                        }
                    );
                }
                catch (Exception ex)
                {
                    ILogger logger = scope.ServiceProvider.GetService<ILogger>();
                    await logger?.Error("Run task error - " + _scheduleTask.Name, ex);
                }
                finally
                {
                    IsRunning = false;
                }
            });
        }

        private void TimerHandler(object state)
        {
            try
            {
                _timer.Change(-1, -1);
                RunTask();
            }
            catch (Exception ex)
            {
                using IServiceScope scope = _serviceProvider.CreateScope();
                ILogger logger = EngineContext.Current.Resolve<ILogger>(scope);
                logger.Error("TimerHandler error - " + _scheduleTask.Name, ex);
            }
            finally
            {
                if (!_disposed && _timer != null)
                {
                    if (RunOnlyOnce)
                    {
                        Dispose();
                    }
                    else
                    {
                        _timer.Change(Interval, Interval);
                    }
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                lock (this)
                {
                    _timer?.Dispose();
                }
            }

            _disposed = true;
        }

        public void InitTimer()
        {
            if (_timer != null)
            {
                return;
            }

            _timer = new Timer(new TimerCallback(TimerHandler), null, InitInterval, Interval);
        }
    }

    #endregion TaskThread
}
