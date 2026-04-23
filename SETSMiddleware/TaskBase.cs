using System;
using System.Threading;
using System.Threading.Tasks;

namespace SETSMiddleware.Tasks
{
    public abstract class TaskBase
    {
        public string TaskName { get; protected set; }
        public string Description { get; protected set; }
        public int IntervalSeconds { get; protected set; }
        public bool IsRunning { get; private set; }
        public DateTime? LastRun { get; private set; }
        public string LastStatus { get; protected set; } = "Never run";

        private CancellationTokenSource _cts;
        private Task _loopTask;

        public event Action<string, LogLevel> OnLog;
        public event Action OnStateChanged;

        public enum LogLevel { Info, Success, Warning, Error }

        protected void Log(string message, LogLevel level = LogLevel.Info)
        {
            OnLog?.Invoke(message, level);
        }

        public void Start()
        {
            if (IsRunning) return;
            IsRunning = true;
            _cts = new CancellationTokenSource();
            _loopTask = RunLoopAsync(_cts.Token);
            LastStatus = "Running";
            OnStateChanged?.Invoke();
            Log($"{TaskName} started. Interval: {IntervalSeconds}s", LogLevel.Success);
        }

        public void Stop()
        {
            if (!IsRunning) return;
            _cts?.Cancel();
            IsRunning = false;
            LastStatus = "Stopped";
            OnStateChanged?.Invoke();
            Log($"{TaskName} stopped.", LogLevel.Warning);
        }

        public async Task RunNowAsync()
        {
            Log($"Manual run triggered for {TaskName}...", LogLevel.Info);
            await ExecuteAsync();
            LastRun = DateTime.Now;
            OnStateChanged?.Invoke();
        }

        private async Task RunLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await ExecuteAsync();
                    LastRun = DateTime.Now;
                    OnStateChanged?.Invoke();
                }
                catch (Exception ex)
                {
                    LastStatus = $"Error: {ex.Message}";
                    Log($"Unhandled error in {TaskName}: {ex.Message}", LogLevel.Error);
                    OnStateChanged?.Invoke();
                }

                try
                {
                    await Task.Delay(IntervalSeconds * 1000, token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }

        protected abstract Task ExecuteAsync();
    }
}