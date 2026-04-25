using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HCLAB;
using Service;

namespace SETSMiddleware.Tasks
{
    /// <summary>
    /// Task 1 — HCLAB Routing Checker
    ///
    /// Polls Specimen_Section_Header for specimens where IsHclabRouted = false,
    /// then checks Oracle's ord_spl.OS_SPL_RCVD_FLAG for each.
    /// Once HCLAB confirms routing (flag = 'Y'), flips IsHclabRouted = true
    /// so the specimen becomes visible in the lab section's pending list.
    /// </summary>
    public class HclabRoutingTask : TaskBase
    {
        private readonly string _branch;

        public HclabRoutingTask(string branch, int intervalSeconds = 30)
        {
            _branch = branch;
            TaskName = "HCLAB Routing Checker";
            Description = "Checks Oracle ord_spl for unrouted specimens and flips IsHclabRouted once OS_SPL_RCVD_FLAG = 'Y'.";
            IntervalSeconds = intervalSeconds;
        }

        protected override async Task ExecuteAsync()
        {
            using var master = new MasterService(_branch);

            // Step 1 — Get all specimen numbers pending HCLAB routing confirmation
            var pending = master.SpecimenSection.GetUnroutedSpecimenNos();

            if (pending.Count == 0)
            {
                Log("No pending unrouted specimens.", LogLevel.Info);
                LastStatus = $"Last run: {DateTime.Now:HH:mm:ss} — nothing pending";
                return;
            }

            Log($"Checking {pending.Count} unrouted specimen(s)...", LogLevel.Info);

            var oracleConn = HclabConnection.ConnectionString(_branch);
            int routedCount = 0;

            foreach (var specimenNo in pending)
            {
                try
                {
                    // Step 2 — Ask Oracle if HCLAB has routed this specimen
                    bool isRouted = await HclabMaster.HCLABTransactions
                        .CheckSplRouted(oracleConn, specimenNo);

                    if (isRouted)
                    {
                        // Step 3 — Flip the flag in MSSQL via the service layer
                        master.SpecimenSection.FlipHclabRouted(specimenNo);
                        Log($"Routed: {specimenNo}", LogLevel.Success);
                        routedCount++;
                    }
                }
                catch (Exception ex)
                {
                    Log($"Error on {specimenNo}: {ex.Message}", LogLevel.Error);
                }
            }

            string summary = routedCount > 0
                ? $"Checked {pending.Count} — Routed {routedCount}"
                : $"Checked {pending.Count} — None confirmed yet";

            LastStatus = $"Last run: {DateTime.Now:HH:mm:ss} — {summary}";
            Log(summary, routedCount > 0 ? LogLevel.Success : LogLevel.Info);
        }
    }
}