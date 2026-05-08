using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Service;

namespace SETSMiddleware.Tasks
{
    /// <summary>
    /// Task 5 — Endorsement TAT Overnight Reset
    ///
    /// Runs every 60 seconds but only acts at 7pm (19:00).
    /// Closes any open TAT cycles left over from the previous day
    /// with Result = "EndOfDay" so no violation is flagged.
    /// The next day's first batch will start a fresh cycle.
    /// </summary>
    public class EndorsementTatResetTask : TaskBase
    {
        private readonly string _branch;

        public EndorsementTatResetTask(string branch, int intervalSeconds = 60)
        {
            _branch = branch;
            TaskName = "Endorsement TAT Reset";
            Description = "Runs at 7pm. Closes any open TAT cycles from the previous day " +
                          "without flagging violations, so the next day starts fresh.";
            IntervalSeconds = intervalSeconds;
        }

        protected override async Task ExecuteAsync()
        {
            var now = DateTime.Now;

            // Only act at 7pm (19:00)
            if (now.Hour != 19 || now.Minute != 00)
            {
                LastStatus = $"Waiting for 7:00 PM — current time: {now:HH:mm:ss}";
                await Task.CompletedTask;
                return;
            }

            Log("7:00 PM reached — starting end-of-day TAT reset...", LogLevel.Info);

            using var master = new MasterService(_branch);

            // Get all active endorsing sections (category = '1')
            var sections = master.Section.GetActive()
                .Where(s => s.Category == "1")
                .ToList();

            if (sections.Count == 0)
            {
                Log("No active endorsing sections found.", LogLevel.Info);
                LastStatus = $"Last run: {now:HH:mm:ss} — no sections";
                return;
            }

            int resetCount = 0;
            foreach (var section in sections)
            {
                try
                {
                    master.Tat.ResetOvernight(section.Code);
                    Log($"Reset: {section.Code} — {section.Name}", LogLevel.Success);
                    resetCount++;
                }
                catch (Exception ex)
                {
                    Log($"Failed to reset {section.Code}: {ex.Message}", LogLevel.Error);
                }
            }

            string summary = $"End-of-day reset: {resetCount}/{sections.Count} section(s)";
            LastStatus = $"Last run: {now:HH:mm:ss} — {summary}";
            Log(summary, LogLevel.Success);
        }
    }
}
