using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Model.SETSDB;
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

            // ── Standard specimens ─────────────────────────────────────────
            var dueTests = master.SpecimenSection.GetDueScheduledTests(today);
            if (dueTests.Count > 0)
            {
                var headerIds = dueTests.Select(t => t.HeaderId).Distinct().ToList();

                // ── Capture headers that are actually still S before release ──
                var headersToRelease = headerIds
                    .Select(id => master.SpecimenSection.GetHeaderById(id))
                    .Where(h => h != null && h.Status == "S")
                    .ToList();

                var actualHeaderIds = headersToRelease.Select(h => h!.Id).ToList();

                if (actualHeaderIds.Any())
                    master.SpecimenSection.ReleaseScheduledHeaders(actualHeaderIds);

                // ── Audit — only tests under headers that were actually S ─────
                try
                {
                    var releasedHeaderIdSet = actualHeaderIds.ToHashSet();
                    foreach (var test in dueTests.Where(t => releasedHeaderIdSet.Contains(t.HeaderId)))
                    {
                        var header = headersToRelease.FirstOrDefault(h => h!.Id == test.HeaderId);
                        if (header == null) continue;

                        // Resolve patient info from Batch_Specimen
                        var bs = master.Batch.CheckSpecimen(header.SpecimenNo);

                        master.Audit.Log(Audit_Log.ScheduleDue(
                            header.SpecimenNo,
                            bs?.PatientName ?? string.Empty,
                            bs?.PID ?? string.Empty,
                            header.SectionCode,
                            test.TestCode,
                            test.TestName,
                            test.ScheduleTag ?? string.Empty,
                            test.RunningDate.HasValue
                                ? test.RunningDate.Value.ToDateTime(TimeOnly.MinValue)
                                : DateTime.Today));
                    }
                }
                catch (Exception auditEx)
                {
                    Log($"[AUDIT] Logging failed: {auditEx.Message}", LogLevel.Warning);
                }

                Log($"Standard: Released {actualHeaderIds.Count} header(s) — {dueTests.Count} test(s).", LogLevel.Success);
            }
            else
            {
                Log("Standard: No scheduled specimens due.", LogLevel.Info);
            }

            // ── On-Site specimens ──────────────────────────────────────────
            var onSiteDueTests = master.OnSite.GetDueScheduledTests(today);
            if (onSiteDueTests.Count > 0)
            {
                var onSiteHeaderIds = onSiteDueTests.Select(t => t.HeaderId).Distinct().ToList();

                // ── Capture headers that are actually still S before release ──
                var onSiteHeadersToRelease = onSiteHeaderIds
                    .Select(id => master.OnSite.GetHeaderById(id))
                    .Where(h => h != null && h.Status == "S")
                    .ToList();

                var actualOnSiteHeaderIds = onSiteHeadersToRelease.Select(h => h!.Id).ToList();

                if (actualOnSiteHeaderIds.Any())
                    master.OnSite.ReleaseScheduledHeaders(actualOnSiteHeaderIds);

                // ── Audit ──────────────────────────────────────────────────────
                // OnSite_Section_Header carries PatientName/PID directly — no Batch_Specimen lookup needed.
                try
                {
                    var releasedOnSiteHeaderIdSet = actualOnSiteHeaderIds.ToHashSet();
                    foreach (var test in onSiteDueTests.Where(t => releasedOnSiteHeaderIdSet.Contains(t.HeaderId)))
                    {
                        var onSiteHeader = onSiteHeadersToRelease.FirstOrDefault(h => h!.Id == test.HeaderId);
                        if (onSiteHeader == null) continue;

                        master.Audit.Log(Audit_Log.ScheduleDue(
                            onSiteHeader.SpecimenNo,
                            onSiteHeader.PatientName ?? string.Empty,
                            onSiteHeader.PID ?? string.Empty,
                            onSiteHeader.SectionCode,
                            test.TestCode,
                            test.TestName,
                            test.ScheduleTag ?? string.Empty,
                            test.RunningDate.HasValue
                                ? test.RunningDate.Value.ToDateTime(TimeOnly.MinValue)
                                : DateTime.Today));
                    }
                }
                catch (Exception auditEx)
                {
                    Log($"[AUDIT] On-Site logging failed: {auditEx.Message}", LogLevel.Warning);
                }

                Log($"On-Site: Released {actualOnSiteHeaderIds.Count} header(s) — {onSiteDueTests.Count} test(s).", LogLevel.Success);
            }
            else
            {
                Log("On-Site: No scheduled specimens due.", LogLevel.Info);
            }

            int totalHeaders = dueTests.Select(t => t.HeaderId).Distinct().Count()
                             + onSiteDueTests.Select(t => t.HeaderId).Distinct().Count();
            int totalTests = dueTests.Count + onSiteDueTests.Count;

            string summary = totalTests > 0
                ? $"Released {totalHeaders} header(s) — {totalTests} test(s) due"
                : "Nothing to release";

            LastStatus = $"Last run: {DateTime.Now:HH:mm:ss} — {summary}";
            Log(summary, totalTests > 0 ? LogLevel.Success : LogLevel.Info);

            await Task.CompletedTask;
        }
    }
}