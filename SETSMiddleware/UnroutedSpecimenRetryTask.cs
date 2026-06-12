using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Service;

namespace SETSMiddleware.Tasks
{
    /// <summary>
    /// Task — Unrouted Specimen Retry
    ///
    /// Polls for Batch_Specimen rows where Status = 'R' but no corresponding
    /// Specimen_Section_Header exists — meaning RouteToLabSections silently
    /// returned empty at receive time (Oracle timeout, Oracle returning no rows,
    /// unmapped test group, etc.).
    ///
    /// For each candidate, retries the full routing logic via
    /// ReceivingService.RetryRouteSpecimen(), which opens its own context +
    /// transaction and calls RouteToLabSections exactly as the original receive
    /// would have. RoutedBy is stamped as "SYSTEM".
    ///
    /// Scoped to a configurable lookback window (default 24 h) so it does not
    /// churn through indefinitely old data on every tick.
    /// </summary>
    public class UnroutedSpecimenRetryTask : TaskBase
    {
        private readonly string _branch;
        private readonly int _lookbackHours;

        public UnroutedSpecimenRetryTask(string branch, int intervalSeconds = 120, int lookbackHours = 24)
        {
            _branch = branch;
            _lookbackHours = lookbackHours;
            TaskName = "Unrouted Specimen Retry";
            Description = $"Finds received specimens (Status = R) with no Specimen_Section_Header " +
                          $"and retries routing. Lookback: {lookbackHours}h. Interval: {intervalSeconds}s.";
            IntervalSeconds = intervalSeconds;
        }

        protected override async Task ExecuteAsync()
        {
            using var master = new MasterService(_branch);

            var candidates = master.Receiving.GetReceivedWithoutSection(_lookbackHours);

            if (candidates.Count == 0)
            {
                Log("No unrouted received specimens found.", LogLevel.Info);
                LastStatus = $"Last run: {DateTime.Now:HH:mm:ss} — nothing pending";
                return;
            }

            var mappingCache = master.SectionTestGroup
                .GetActive()
                .ToDictionary(m => m.TestGroupCode, m => m.SectionCode);

            if (mappingCache.Count == 0)
            {
                Log("No active Section_TestGroup mappings found — nothing to route against.", LogLevel.Warning);
                LastStatus = $"Last run: {DateTime.Now:HH:mm:ss} — no mappings";
                return;
            }

            Log($"Found {candidates.Count} specimen(s) received but not routed. Retrying...", LogLevel.Warning);

            // Test groups confirmed empty or unmapped this tick — skip Oracle for any
            // subsequent specimen that shares the same LabNo + SampleTypeCode combination.
            var emptyOracleKeys = new HashSet<string>(); // key: "{LabNo}|{SampleTypeCode}"
            var unmappedGroups = new HashSet<string>(); // key: testGroupCode

            int routed = 0;
            int skipped = 0;
            int failed = 0;

            foreach (var specimen in candidates)
            {
                var oracleKey = $"{specimen.LabNo}|{specimen.SampleTypeCode}";

                // Skip — same LabNo+SampleType already returned empty from Oracle this tick
                if (emptyOracleKeys.Contains(oracleKey))
                {
                    Log($"Skipped: {specimen.SpecimenNo} — Oracle returned empty for {oracleKey} (cached this tick).", LogLevel.Warning);
                    skipped++;
                    continue;
                }

                try
                {
                    bool success = await master.Receiving.RetryRouteSpecimen(
                        specimen.SpecimenNo, mappingCache, unmappedGroups, emptyOracleKeys);

                    if (success)
                    {
                        Log($"Routed: {specimen.SpecimenNo} (LabNo: {specimen.LabNo})", LogLevel.Success);
                        routed++;
                    }
                    else
                    {
                        Log($"Skipped: {specimen.SpecimenNo} — Oracle empty or test group unmapped.", LogLevel.Warning);
                        skipped++;
                    }
                }
                catch (Exception ex)
                {
                    Log($"Failed: {specimen.SpecimenNo} — {ex.Message}", LogLevel.Error);
                    failed++;
                }
            }

            string summary = $"Checked {candidates.Count} — Routed {routed} — Skipped {skipped} — Failed {failed}";
            LastStatus = $"Last run: {DateTime.Now:HH:mm:ss} — {summary}";
            Log(summary, routed > 0 ? LogLevel.Success : LogLevel.Warning);
        }
    }
}