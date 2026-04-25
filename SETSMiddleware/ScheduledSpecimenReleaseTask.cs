using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Service;

namespace SETSMiddleware.Tasks
{
    /// <summary>
    /// Task 2 — Scheduled Specimen Release
    ///
    /// Fires once daily at midnight (00:00). Finds all Specimen_Section_Test rows
    /// where Status = "S", ScheduleTag IN ("END", "CRD", "SRD"), and RunningDate
    /// is today or in the past. For each matching test, flips the parent
    /// Specimen_Section_Header.Status from "S" back to "P" so the specimen
    /// re-surfaces in the lab section's Pending Specimens list.
    ///
    /// Can also be triggered manually via the Run Now button in the UI.
    /// </summary>
    public class ScheduledSpecimenReleaseTask : TaskBase
    {
        private readonly string _branch;
        public ScheduledSpecimenReleaseTask(string branch, int intervalSeconds = 120)
        {
            _branch = branch;
            TaskName = "Scheduled Specimen Release";
            Description = "Checks every N seconds. Releases scheduled specimens (END/CRD/SRD) " +
                          "whose RunningDate is today or past by flipping Header status back to Pending.";
            IntervalSeconds = intervalSeconds;
        }

        /// <summary>
        /// Overrides the default fixed-interval loop with a midnight-aligned loop.
        /// Calculates the delay to the next 00:00:00, waits, runs, then repeats every 24 h.
        /// Manual "Run Now" still works independently via RunNowAsync().
        /// </summary>

        protected override async Task ExecuteAsync()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            Log($"Running for date: {today:yyyy-MM-dd}", LogLevel.Info);

            using var master = new MasterService(_branch);

            var dueTests = master.SpecimenSection.GetDueScheduledTests(today);

            if (dueTests.Count == 0)
            {
                Log("No scheduled specimens due today or past.", LogLevel.Info);
                LastStatus = $"Last run: {DateTime.Now:HH:mm:ss} — nothing to release";
                return;
            }

            Log($"Found {dueTests.Count} due test(s) across " +
                $"{dueTests.Select(t => t.HeaderId).Distinct().Count()} header(s).", LogLevel.Info);

            var headerIds = dueTests.Select(t => t.HeaderId).Distinct().ToList();
            master.SpecimenSection.ReleaseScheduledHeaders(headerIds);

            string summary = $"Released {headerIds.Count} header(s) — {dueTests.Count} test(s) due";
            LastStatus = $"Last run: {DateTime.Now:HH:mm:ss} — {summary}";
            Log(summary, LogLevel.Success);

            await Task.CompletedTask;
        }
    }
}