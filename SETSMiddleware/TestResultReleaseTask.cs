using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using HCLAB;
using Model.SETSDB;
using Service;

namespace SETSMiddleware.Tasks
{
    public class TestResultReleaseTask : TaskBase
    {
        private readonly string _branch;
        private static readonly HttpClient _http = new HttpClient();

        // Base URL of SETS.Server — adjust if different
        private const string SETS_BASE_URL = "http://localhost:5066";
        private const string MIDDLEWARE_KEY = "sets-middleware-secret-2026";

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

                        var resolvedTestCode = master.TestCodeMap.ResolveCode(test.TestCode);

                        bool isReleased = await HclabMaster.HCLABTransactions
                            .CheckTestReleased(oracleConn, labNo, resolvedTestCode);

                        if (!isReleased) continue;

                        master.SpecimenSection.MarkTestReleased(test.Id);

                        // ── TryCompleteHeader now returns bool ─────────────
                        bool justCompleted = master.SpecimenSection.TryCompleteHeader(test.HeaderId);

                        if (justCompleted && header != null)
                        {
                            await FireSpecimenCompletedAsync(header.SpecimenNo, header.SectionCode);
                        }

                        // ── Audit ──────────────────────────────────────────
                        // Resolve patient info from Batch_Specimen for standard specimens
                        try
                        {
                            var bs = master.Batch.CheckSpecimen(specimenNo);

                            master.Audit.Log(Audit_Log.ResultReleased(
                                specimenNo,
                                bs?.PatientName ?? string.Empty,
                                bs?.PID ?? string.Empty,
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

                        var resolvedTestCode = master.TestCodeMap.ResolveCode(test.TestCode);

                        bool isReleased = await HclabMaster.HCLABTransactions
                            .CheckTestReleased(oracleConn, labNo, resolvedTestCode);

                        if (!isReleased) continue;

                        master.OnSite.MarkTestReleased(test.Id);
                        bool justCompleted = master.OnSite.TryCompleteHeader(test.HeaderId);

                        if (justCompleted && header != null)
                        {
                            await FireSpecimenCompletedAsync(header.SpecimenNo, header.SectionCode);
                        }

                        // ── Audit ──────────────────────────────────────────
                        // OnSite_Section_Header carries PatientName/PID directly — no Batch_Specimen lookup needed.
                        try
                        {
                            master.Audit.Log(Audit_Log.ResultReleased(
                               specimenNo,
                               header!.PatientName ?? string.Empty,
                               header!.PID ?? string.Empty,
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

        // ── Fire SPECIMEN_COMPLETED to SETS.Server ─────────────────────────────
        private async Task FireSpecimenCompletedAsync(string specimenNo, string sectionCode)
        {
            try
            {
                var payload = new
                {
                    branch = _branch,
                    notifType = "SPECIMEN_COMPLETED",
                    title = "Specimen Completed",
                    message = $"Specimen {specimenNo} has been completed in your section.",
                    targetCategory = "3",
                    targetSection = sectionCode,
                    referenceID = specimenNo,
                    isAdmin = false
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                using var req = new HttpRequestMessage(HttpMethod.Post,
                    $"{SETS_BASE_URL}/api/notification/middleware");
                req.Headers.Add("X-Middleware-Key", MIDDLEWARE_KEY);
                req.Content = content;

                var response = await _http.SendAsync(req);

                if (!response.IsSuccessStatusCode)
                    Log($"[NOTIF] SPECIMEN_COMPLETED HTTP {response.StatusCode} for {specimenNo}", LogLevel.Warning);
                else
                    Log($"[NOTIF] SPECIMEN_COMPLETED fired for {specimenNo}", LogLevel.Success);
            }
            catch (Exception ex)
            {
                Log($"[NOTIF] FireSpecimenCompleted failed for {specimenNo}: {ex.Message}", LogLevel.Warning);
            }
        }
    }
}