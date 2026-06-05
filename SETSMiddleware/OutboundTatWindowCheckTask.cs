using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Service;

namespace SETSMiddleware.Tasks
{
    /// <summary>
    /// Task — Outbound TAT Window Check
    ///
    /// Runs every 60 seconds.
    /// For each active outbound TAT window configured for today's schedule,
    /// once the window's end time has passed, checks whether any outbound
    /// batches were endorsed during that window and logs the result:
    ///   - "Within"  → at least one outbound batch was endorsed in the window
    ///   - "Missed"  → no outbound batches endorsed in the window
    ///
    /// Idempotent — already-logged windows are skipped on subsequent ticks.
    /// Does not run if OutboundTatEnabled = false for this branch.
    /// </summary>
    public class OutboundTatWindowCheckTask : TaskBase
    {
        private readonly string _branch;

        public OutboundTatWindowCheckTask(string branch, int intervalSeconds = 60)
        {
            _branch = branch;
            TaskName = "Outbound TAT Window Check";
            Description = "Runs every minute. Logs each outbound TAT window as " +
                          "Within or Missed once the window's end time has passed.";
            IntervalSeconds = intervalSeconds;
        }

        protected override async Task ExecuteAsync()
        {
            var now = DateTime.Now;

            using var master = new MasterService(_branch);

            // Skip entirely if outbound TAT is disabled for this branch
            if (!master.TatOutbound.IsOutboundTatEnabled())
            {
                LastStatus = $"Outbound TAT disabled — idle at {now:HH:mm:ss}";
                await Task.CompletedTask;
                return;
            }

            // Get today's applicable windows (handles Sunday fallback logic)
            var todayWindows = master.TatOutbound.GetTodayWindows(now);

            if (todayWindows.Count == 0)
            {
                LastStatus = $"No windows configured for today — idle at {now:HH:mm:ss}";
                await Task.CompletedTask;
                return;
            }

            // Only process windows whose end time has already passed
            var closedWindows = todayWindows
                .Where(w => now.TimeOfDay >= w.WindowEnd)
                .ToList();

            if (closedWindows.Count == 0)
            {
                // Find the next window end time to show a useful status
                var nextClose = todayWindows
                    .Where(w => now.TimeOfDay < w.WindowEnd)
                    .OrderBy(w => w.WindowEnd)
                    .FirstOrDefault();

                LastStatus = nextClose != null
                    ? $"Waiting for window to close at {nextClose.WindowEnd:hh\\:mm} — {now:HH:mm:ss}"
                    : $"No closing windows pending — {now:HH:mm:ss}";

                await Task.CompletedTask;
                return;
            }

            int logged = 0;
            int skipped = 0;

            foreach (var window in closedWindows)
            {
                try
                {
                    // Idempotent — skip if already logged for today
                    var existingLog = master.TatOutbound.GetLog(window.Id, now);
                    if (existingLog != null)
                    {
                        skipped++;
                        continue;
                    }

                    // Resolve full datetimes for this window on today's date
                    var windowStart = now.Date.Add(window.WindowStart);
                    var windowEnd = now.Date.Add(window.WindowEnd);

                    // Find all outbound batches endorsed within this window today
                    // on Branch A (IsOutbound = true, not IsInbound)
                    var batchNosInWindow = GetOutboundBatchNosInWindow(windowStart, windowEnd);

                    var result = batchNosInWindow.Count > 0 ? "Within" : "Missed";

                    master.TatOutbound.LogWindowResult(
                        window.Id,
                        now,
                        windowStart,
                        windowEnd,
                        result,
                        batchNosInWindow
                    );

                    Log($"Window {window.WindowStart:hh\\:mm}–{window.WindowEnd:hh\\:mm} → {result}" +
                        (batchNosInWindow.Count > 0 ? $" ({batchNosInWindow.Count} batch(es))" : ""),
                        result == "Missed" ? LogLevel.Warning : LogLevel.Success);

                    logged++;
                }
                catch (Exception ex)
                {
                    Log($"Failed to log window {window.Id}: {ex.Message}", LogLevel.Error);
                }
            }

            string summary = $"Logged: {logged}, Skipped (already done): {skipped}";
            LastStatus = $"Last run: {now:HH:mm:ss} — {summary}";

            if (logged > 0)
                Log(summary, LogLevel.Info);

            await Task.CompletedTask;
        }

        /// <summary>
        /// Queries the branch DB for outbound batch headers endorsed within the given window.
        /// Uses a dedicated context to avoid sharing state with the master service context.
        /// </summary>
        private List<string> GetOutboundBatchNosInWindow(DateTime windowStart, DateTime windowEnd)
        {
            try
            {
                using var master = new MasterService(_branch);
                using var context = new Reposi.Context.AppDbContextFactory()
                    .CreateContext(Reposi.SetsConnection.ConnectionString(_branch));

                return context.Batch_Header
                    .Where(h => h.IsOutbound
                             && !h.IsInbound
                             && h.Endorsed >= windowStart
                             && h.Endorsed < windowEnd)
                    .ToList()
                    .Select(h => h.BatchNo)
                    .ToList();
            }
            catch
            {
                return new List<string>();
            }
        }
    }
}