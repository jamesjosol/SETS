using HCLAB;
using Model.SETSDB;
using Reposi;
using Reposi.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SETSMiddleware.Tasks
{
    public class OutboundHclabPostCheckTask : TaskBase
    {
        private readonly AppDbContextFactory _factory;
        private readonly string _localBranch;
        private readonly string _localConn;

        public OutboundHclabPostCheckTask(
            string branch, int intervalSeconds, AppDbContextFactory factory)
        {
            TaskName = "Outbound HCLAB Post Check";
            Description = "Checks if outbound specimens have been posted to the destination branch HCLAB Oracle. Updates IsPostedToDest flag when confirmed.";
            IntervalSeconds = intervalSeconds;
            _factory = factory;
            _localBranch = branch;
            _localConn = SetsConnection.ConnectionString(branch);
        }

        protected override async Task ExecuteAsync()
        {
            // ── Step 1: Load unposted outbound specimens ───────────────────
            // Join Batch_Specimen with Batch_Header in memory to find
            // outbound specimens where IsPostedToDest = 0
            List<(Batch_Specimen Specimen, string DestBranchCode)> unposted;

            try
            {
                using var localCtx = _factory.CreateContext(_localConn);

                // Materialize headers first — avoid EF Core CTE issue
                var outboundHeaders = localCtx.Batch_Header
                    .Where(h => h.IsOutbound && h.Status != "C")
                    .ToList()
                    .Where(h => !string.IsNullOrEmpty(h.DestBranchCode))
                    .ToDictionary(h => h.BatchNo, h => h.DestBranchCode!);

                if (!outboundHeaders.Any())
                {
                    LastStatus = $"No active outbound batches. Last check: {DateTime.Now:HH:mm:ss}";
                    Log("No active outbound batches found.", LogLevel.Info);
                    return;
                }

                var batchNos = outboundHeaders.Keys.ToHashSet();

                // Get unposted specimens from those batches
                var unpostedSpecimens = localCtx.Batch_Specimen
                    .Where(s => !s.IsPostedToDest && s.Status != "X")
                    .ToList()
                    .Where(s => batchNos.Contains(s.BatchNo))
                    .ToList();

                unposted = unpostedSpecimens
                    .Select(s => (s, outboundHeaders[s.BatchNo]))
                    .ToList();
            }
            catch (Exception ex)
            {
                LastStatus = $"Error loading specimens: {ex.Message}";
                Log($"Failed to load unposted specimens: {ex.Message}", LogLevel.Error);
                return;
            }

            if (!unposted.Any())
            {
                LastStatus = $"All outbound specimens posted. Last check: {DateTime.Now:HH:mm:ss}";
                Log("All outbound specimens are posted to destination.", LogLevel.Info);
                return;
            }

            Log($"Found {unposted.Count} unposted specimen(s). Checking destination HCLAB...", LogLevel.Info);

            // ── Step 2: Group by destination branch ───────────────────────
            var groups = unposted
                .GroupBy(x => x.DestBranchCode)
                .ToList();

            int confirmed = 0, stillPending = 0, errors = 0;

            foreach (var group in groups)
            {
                var destBranch = group.Key;
                var specimens = group.Select(x => x.Specimen).ToList();

                try
                {
                    var (c, p, e) = await CheckGroupAsync(destBranch, specimens);
                    confirmed += c;
                    stillPending += p;
                    errors += e;
                }
                catch (Exception ex)
                {
                    Log($"[{destBranch}] Group error: {ex.Message}", LogLevel.Error);
                    errors += specimens.Count;
                }
            }

            LastStatus = $"Confirmed: {confirmed} | Pending: {stillPending} | Errors: {errors} | {DateTime.Now:HH:mm:ss}";
            Log($"HCLAB post check complete — Confirmed: {confirmed}, Still pending: {stillPending}, Errors: {errors}",
                confirmed > 0 ? LogLevel.Success : LogLevel.Info);

            await Task.CompletedTask;
        }

        private async Task<(int confirmed, int stillPending, int errors)> CheckGroupAsync(
            string destBranch, List<Batch_Specimen> specimens)
        {
            int confirmed = 0, stillPending = 0, errors = 0;

            // ── Resolve destination HCLAB connection ──────────────────────
            string destHclabConn;
            try
            {
                destHclabConn = HclabConnection.ConnectionString(destBranch);
            }
            catch
            {
                Log($"[{destBranch}] No HCLAB connection string configured. Skipping {specimens.Count} specimen(s).",
                    LogLevel.Warning);
                return (0, 0, specimens.Count);
            }

            // ── Check each specimen against dest HCLAB ────────────────────
            // Collect all that are confirmed so we can batch-update local DB
            var confirmedSpecimenIds = new List<int>();

            foreach (var specimen in specimens)
            {
                try
                {
                    var tests = await HclabMaster.HCLABTransactions
                        .GetOrd_Dtl(destHclabConn, specimen.LabNo, specimen.SampleTypeCode);

                    if (tests != null && tests.Any())
                    {
                        confirmedSpecimenIds.Add(specimen.Id);
                        Log($"[{destBranch}] {specimen.SpecimenNo} (Lab: {specimen.LabNo}) — posted ✓",
                            LogLevel.Success);
                        confirmed++;
                    }
                    else
                    {
                        Log($"[{destBranch}] {specimen.SpecimenNo} (Lab: {specimen.LabNo}) — not yet posted.",
                            LogLevel.Info);
                        stillPending++;
                    }
                }
                catch (Exception ex)
                {
                    Log($"[{destBranch}] {specimen.SpecimenNo} — HCLAB check error: {ex.Message}",
                        LogLevel.Warning);
                    errors++;
                }
            }

            // ── Batch update confirmed specimens in local DB ───────────────
            if (confirmedSpecimenIds.Any())
            {
                try
                {
                    using var localCtx = _factory.CreateContext(_localConn);

                    // Materialize then filter in memory — avoid EF Core CTE issue
                    var toUpdate = localCtx.Batch_Specimen
                        .Where(s => confirmedSpecimenIds.Contains(s.Id))
                        .ToList();

                    foreach (var s in toUpdate)
                        s.IsPostedToDest = true;

                    localCtx.SaveChanges();

                    Log($"[{destBranch}] Updated {toUpdate.Count} specimen(s) as posted in local DB.",
                        LogLevel.Success);
                }
                catch (Exception ex)
                {
                    Log($"[{destBranch}] Failed to update IsPostedToDest in local DB: {ex.Message}",
                        LogLevel.Error);
                }
            }

            return (confirmed, stillPending, errors);
        }
    }
}