using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HCLAB;
using Service;

namespace SETSMiddleware.Tasks
{
    /// <summary>
    /// Task 4 — HCLAB Reference Table Sync
    ///
    /// Runs once daily at midnight (00:00). Pulls Sample_Type and Test_Group
    /// reference data from Oracle (HCLAB) and upserts them into the SETS
    /// MSSQL database. Inserts new records and updates names of existing ones.
    /// Records no longer in Oracle are left untouched.
    ///
    /// Can also be triggered manually via the Run Now button in the UI.
    /// </summary>
    public class HclabReferenceSyncTask : TaskBase
    {
        private readonly string _branch;

        public HclabReferenceSyncTask(string branch, int intervalSeconds = 43200)
        {
            _branch = branch;
            TaskName = "HCLAB Reference Sync";
            Description = "Syncs Sample Type and Test Group reference tables from Oracle to MSSQL daily at midnight. " +
                          "Inserts new records and updates existing names.";
            IntervalSeconds = intervalSeconds;
        }

        // ── Loop Override — midnight-aligned ─────────────────────────────────────

        protected override async Task RunLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await ExecuteAsync();
                    SetLastRun(DateTime.Now);
                    RaiseStateChanged();
                }
                catch (Exception ex)
                {
                    LastStatus = $"Error: {ex.Message}";
                    Log($"Unhandled error in {TaskName}: {ex.Message}", LogLevel.Error);
                    RaiseStateChanged();
                }

                var delay = GetDelayUntilMidnight();
                Log($"Next sync scheduled at midnight — {delay.TotalHours:F1}h from now.", LogLevel.Info);

                try
                {
                    await Task.Delay(delay, token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }

        // ── Core Execution ────────────────────────────────────────────────────────

        protected override async Task ExecuteAsync()
        {
            Log("Starting HCLAB reference sync...", LogLevel.Info);

            var oracleConn = HclabConnection.ConnectionString(_branch);

            await SyncSampleTypesAsync(oracleConn);
            await SyncTestGroupsAsync(oracleConn);

            LastStatus = $"Last sync: {DateTime.Now:HH:mm:ss}";
            Log("Reference sync completed.", LogLevel.Success);
        }

        // ── Sample Type Sync ──────────────────────────────────────────────────────

        private async Task SyncSampleTypesAsync(string oracleConn)
        {
            Log("Fetching sample types from Oracle...", LogLevel.Info);

            var records = await HclabMaster.HCLABTests.GetSampleTypes(oracleConn);

            if (records == null || records.Count == 0)
            {
                Log("No sample types returned from Oracle — skipping.", LogLevel.Warning);
                return;
            }

            Log($"Syncing {records.Count} sample type(s)...", LogLevel.Info);

            using var master = new MasterService(_branch);
            master.SampleType.UpsertFromHclab(records);

            Log($"Sample types synced successfully — {records.Count} record(s) processed.", LogLevel.Success);
        }

        // ── Test Group Sync ───────────────────────────────────────────────────────

        private async Task SyncTestGroupsAsync(string oracleConn)
        {
            Log("Fetching test groups from Oracle...", LogLevel.Info);

            var records = await HclabMaster.HCLABTests.GetTestGroups(oracleConn);

            if (records == null || records.Count == 0)
            {
                Log("No test groups returned from Oracle — skipping.", LogLevel.Warning);
                return;
            }

            Log($"Syncing {records.Count} test group(s)...", LogLevel.Info);

            using var master = new MasterService(_branch);
            master.TestGroup.UpsertFromHclab(records);

            Log($"Test groups synced successfully — {records.Count} record(s) processed.", LogLevel.Success);
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        private static TimeSpan GetDelayUntilMidnight()
        {
            var now = DateTime.Now;
            var midnight = now.Date.AddDays(1);
            return midnight - now;
        }
    }
}