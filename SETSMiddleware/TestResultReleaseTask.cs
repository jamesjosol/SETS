using System;
using System.Linq;
using System.Threading.Tasks;
using HCLAB;
using Model.SETSDB;
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
            var oracleConn = HclabConnection.ConnectionString(_branch);
            int releasedCount = 0;

            // ── Standard specimens ─────────────────────────────────────────
            var runningTests = master.SpecimenSection.GetAllRunningTests();

            if (runningTests.Count > 0)
            {
                Log($"Checking {runningTests.Count} standard running test(s)...", LogLevel.Info);

                foreach (var test in runningTests)
                {
                    try
                    {
                        var header = master.SpecimenSection.GetHeaderById(test.HeaderId);
                        var specimenNo = header?.SpecimenNo;

                        if (string.IsNullOrEmpty(specimenNo))
                        {
                            Log($"Header {test.HeaderId} not found — skipping.", LogLevel.Warning);
                            continue;
                        }

                        var labNo = specimenNo.Length >= 10
                            ? specimenNo.Substring(0, 10)
                            : specimenNo;

                        bool isReleased = await HclabMaster.HCLABTransactions
                            .CheckTestReleased(oracleConn, labNo, test.TestCode);

                        if (!isReleased) continue;

                        master.SpecimenSection.MarkTestReleased(test.Id);
                        master.SpecimenSection.TryCompleteHeader(test.HeaderId);

                        // ── Audit ──────────────────────────────────────────
                        try
                        {
                            master.Audit.Log(Audit_Log.ResultReleased(
                                specimenNo,
                                header!.SectionCode,
                                test.TestCode,
                                test.TestName));
                        }
                        catch (Exception auditEx)
                        {
                            Log($"[AUDIT] Failed for {specimenNo} — {test.TestCode}: {auditEx.Message}", LogLevel.Warning);
                        }

                        Log($"Standard released: {specimenNo} — {test.TestCode}", LogLevel.Success);
                        releasedCount++;
                    }
                    catch (Exception ex)
                    {
                        Log($"Error on standard test {test.Id} ({test.TestCode}): {ex.Message}", LogLevel.Error);
                    }
                }
            }
            else
            {
                Log("Standard: No running tests to check.", LogLevel.Info);
            }

            // ── On-Site specimens ──────────────────────────────────────────
            var onSiteRunningTests = master.OnSite.GetAllRunningTests();

            if (onSiteRunningTests.Count > 0)
            {
                Log($"Checking {onSiteRunningTests.Count} On-Site running test(s)...", LogLevel.Info);

                foreach (var test in onSiteRunningTests)
                {
                    try
                    {
                        var header = master.OnSite.GetHeaderById(test.HeaderId);
                        var specimenNo = header?.SpecimenNo;

                        if (string.IsNullOrEmpty(specimenNo))
                        {
                            Log($"On-Site header {test.HeaderId} not found — skipping.", LogLevel.Warning);
                            continue;
                        }

                        var labNo = specimenNo.Length >= 10
                            ? specimenNo.Substring(0, 10)
                            : specimenNo;

                        bool isReleased = await HclabMaster.HCLABTransactions
                            .CheckTestReleased(oracleConn, labNo, test.TestCode);

                        if (!isReleased) continue;

                        master.OnSite.MarkTestReleased(test.Id);
                        master.OnSite.TryCompleteHeader(test.HeaderId);

                        // ── Audit ──────────────────────────────────────────
                        try
                        {
                            master.Audit.Log(Audit_Log.ResultReleased(
                               specimenNo,
                               header!.SectionCode,
                               test.TestCode,
                               test.TestName));
                        }
                        catch (Exception auditEx)
                        {
                            Log($"[AUDIT] Failed for {specimenNo} — {test.TestCode}: {auditEx.Message}", LogLevel.Warning);
                        }

                        Log($"On-Site released: {specimenNo} — {test.TestCode}", LogLevel.Success);
                        releasedCount++;
                    }
                    catch (Exception ex)
                    {
                        Log($"Error on On-Site test {test.Id} ({test.TestCode}): {ex.Message}", LogLevel.Error);
                    }
                }
            }
            else
            {
                Log("On-Site: No running tests to check.", LogLevel.Info);
            }

            int totalChecked = runningTests.Count + onSiteRunningTests.Count;
            string summary = totalChecked == 0
                ? "Nothing running"
                : releasedCount > 0
                    ? $"Checked {totalChecked} — Released {releasedCount}"
                    : $"Checked {totalChecked} — None released yet";

            LastStatus = $"Last run: {DateTime.Now:HH:mm:ss} — {summary}";
            Log(summary, releasedCount > 0 ? LogLevel.Success : LogLevel.Info);
        }
    }
}