using System;
using System.Linq;
using System.Threading.Tasks;
using HCLAB;
using Service;

namespace SETSMiddleware.Tasks
{
    /// <summary>
    /// Task 3 — Test Result Release Checker
    ///
    /// Runs every 60 seconds. Finds all Specimen_Section_Test rows with Status = "R"
    /// (Running), queries Oracle ord_dtl to check if all child rows (od_item_type = 'U')
    /// under the matching od_order_ti have a non-null od_release_on.
    /// If fully released: flips test Status → "X", then checks if all sibling tests
    /// are also "X" — if so flips the parent Specimen_Section_Header Status → "C".
    /// </summary>
    public class TestResultReleaseTask : TaskBase
    {
        private readonly string _branch;
        public TestResultReleaseTask(string branch, int intervalSeconds = 60)
        {
            _branch = branch;
            TaskName = "Test Result Release";
            Description = "Checks Oracle ord_dtl every 60s for running tests. " +
                          "Marks tests released (X) when all child results are out, " +
                          "then completes the header (C) when all tests are released.";
            IntervalSeconds = intervalSeconds;
        }

        protected override async Task ExecuteAsync()
        {
            using var master = new MasterService(_branch);

            // Step 1 — Get all running tests across all sections
            var runningTests = master.SpecimenSection.GetAllRunningTests();

            if (runningTests.Count == 0)
            {
                Log("No running tests to check.", LogLevel.Info);
                LastStatus = $"Last run: {DateTime.Now:HH:mm:ss} — nothing running";
                return;
            }

            Log($"Checking {runningTests.Count} running test(s)...", LogLevel.Info);

            var oracleConn = HclabConnection.ConnectionString(_branch);
            int releasedCount = 0;

            foreach (var test in runningTests)
            {
                try
                {
                    // Step 2 — Derive lab number: first 10 chars of SpecimenNo
                    // SpecimenNo lives on the header — load it
                    var specimenNo = master.SpecimenSection
                        .GetHeaderById(test.HeaderId)?.SpecimenNo;

                    if (string.IsNullOrEmpty(specimenNo))
                    {
                        Log($"Header {test.HeaderId} not found — skipping.", LogLevel.Warning);
                        continue;
                    }

                    var labNo = specimenNo.Length >= 10
                        ? specimenNo.Substring(0, 10)
                        : specimenNo;

                    // Step 3 — Ask Oracle if this test is fully released
                    bool isReleased = await HclabMaster.HCLABTransactions
                        .CheckTestReleased(oracleConn, labNo, test.TestCode);

                    if (!isReleased) continue;

                    // Step 4 — Mark test as released
                    master.SpecimenSection.MarkTestReleased(test.Id);
                    Log($"Released: {specimenNo} — {test.TestCode}", LogLevel.Success);
                    releasedCount++;

                    // Step 5 — Check if header is now fully complete
                    master.SpecimenSection.TryCompleteHeader(test.HeaderId);
                }
                catch (Exception ex)
                {
                    Log($"Error on test {test.Id} ({test.TestCode}): {ex.Message}", LogLevel.Error);
                }
            }

            string summary = releasedCount > 0
                ? $"Checked {runningTests.Count} — Released {releasedCount}"
                : $"Checked {runningTests.Count} — None released yet";

            LastStatus = $"Last run: {DateTime.Now:HH:mm:ss} — {summary}";
            Log(summary, releasedCount > 0 ? LogLevel.Success : LogLevel.Info);
        }
    }
}