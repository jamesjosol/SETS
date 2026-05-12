using Microsoft.Extensions.Configuration;
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
    public class OutboundStatusSyncTask : TaskBase
    {
        private readonly AppDbContextFactory _factory;
        private readonly string _localBranch;
        private readonly string _localConn;

        public OutboundStatusSyncTask(string branch, int intervalSeconds, AppDbContextFactory factory)
        {
            TaskName = "Outbound Status Sync";
            Description = "Polls destination branch DBs for outbound batch, specimen, and non-barcoded status changes and syncs them back to the local DB. Also retries failed mirror writes.";
            IntervalSeconds = intervalSeconds;
            _factory = factory;
            _localBranch = branch;
            _localConn = SetsConnection.ConnectionString(branch);
        }

        protected override async Task ExecuteAsync()
        {
            // ── Step 1: Load all pending outbound batches ─────────────────
            List<Batch_Header> pendingOutbound;

            try
            {
                using var localCtx = _factory.CreateContext(_localConn);
                pendingOutbound = localCtx.Batch_Header
                    .Where(b => b.IsOutbound && b.Status != "C")
                    .ToList();
            }
            catch (Exception ex)
            {
                LastStatus = $"Error loading outbound batches: {ex.Message}";
                Log($"Failed to load pending outbound batches: {ex.Message}", LogLevel.Error);
                return;
            }

            if (!pendingOutbound.Any())
            {
                LastStatus = $"No pending outbound batches. Last check: {DateTime.Now:HH:mm:ss}";
                Log("No pending outbound batches found.", LogLevel.Info);
                return;
            }

            Log($"Found {pendingOutbound.Count} pending outbound batch(es). Processing...", LogLevel.Info);

            // ── Step 2: Group by destination branch ───────────────────────
            var groups = pendingOutbound
                .Where(b => !string.IsNullOrEmpty(b.DestBranchCode))
                .GroupBy(b => b.DestBranchCode!)
                .ToList();

            int synced = 0, retried = 0, failed = 0;
            int specimenSynced = 0, receivingSynced = 0, nonBarcodedSynced = 0;

            foreach (var group in groups)
            {
                var (s, r, f, ss, rs, ns) = await SyncGroupAsync(group.Key, group.ToList());
                synced += s;
                retried += r;
                failed += f;
                specimenSynced += ss;
                receivingSynced += rs;
                nonBarcodedSynced += ns;
            }

            LastStatus = $"Synced: {synced} | Retried: {retried} | Failed: {failed} | " +
                         $"Specimens: {specimenSynced} | Receiving: {receivingSynced} | " +
                         $"NonBarcoded: {nonBarcodedSynced} | {DateTime.Now:HH:mm:ss}";

            Log($"Sync complete — Batches: {synced}, Retried: {retried}, Failed: {failed}, " +
                $"Specimens: {specimenSynced}, Receiving rows: {receivingSynced}, " +
                $"NonBarcoded: {nonBarcodedSynced}", LogLevel.Success);
        }

        private async Task<(int synced, int retried, int failed,
                             int specimenSynced, int receivingSynced, int nonBarcodedSynced)>
            SyncGroupAsync(string destBranch, List<Batch_Header> batches)
        {
            int synced = 0, retried = 0, failed = 0;
            int specimenSynced = 0, receivingSynced = 0, nonBarcodedSynced = 0;

            // ── Resolve connection string ──────────────────────────────────
            string destConn;
            try
            {
                destConn = SetsConnection.ConnectionString(destBranch);
            }
            catch
            {
                Log($"[{destBranch}] No connection string configured. Skipping {batches.Count} batch(es).", LogLevel.Warning);
                return (0, 0, batches.Count, 0, 0, 0);
            }

            // ── Open destination DB — load all tables in one connection ────
            Dictionary<string, Batch_Header?> destHeaders;
            Dictionary<string, List<Batch_Specimen>> destSpecimens;
            Dictionary<string, List<Batch_Specimen_Receiving>> destReceiving;
            Dictionary<string, List<Batch_NonBarcoded>> destNonBarcoded;
            bool destReachable = true;

            try
            {
                using var destCtx = _factory.CreateContext(destConn);

                var batchNos = batches.Select(b => b.BatchNo).ToHashSet();

                // Headers
                var allInbound = destCtx.Batch_Header
                    .Where(b => b.IsInbound)
                    .ToList();

                destHeaders = allInbound
                    .Where(b => batchNos.Contains(b.BatchNo))
                    .ToDictionary(b => b.BatchNo, b => (Batch_Header?)b);

                foreach (var bn in batchNos.Where(bn => !destHeaders.ContainsKey(bn)))
                    destHeaders[bn] = null;

                // Specimens — materialize then filter in memory (avoid CTE)
                destSpecimens = destCtx.Batch_Specimen
                    .ToList()
                    .Where(s => batchNos.Contains(s.BatchNo))
                    .GroupBy(s => s.BatchNo)
                    .ToDictionary(g => g.Key, g => g.ToList());

                // Receiving records
                destReceiving = destCtx.Batch_Specimen_Receiving
                    .ToList()
                    .Where(r => batchNos.Contains(r.BatchNo))
                    .GroupBy(r => r.BatchNo)
                    .ToDictionary(g => g.Key, g => g.ToList());

                // NonBarcoded
                destNonBarcoded = destCtx.Batch_NonBarcoded
                    .ToList()
                    .Where(n => batchNos.Contains(n.BatchNo))
                    .GroupBy(n => n.BatchNo)
                    .ToDictionary(g => g.Key, g => g.ToList());
            }
            catch (Exception ex)
            {
                Log($"[{destBranch}] Cannot connect: {ex.Message}. Will retry next tick.", LogLevel.Warning);
                destReachable = false;
                destHeaders = new();
                destSpecimens = new();
                destReceiving = new();
                destNonBarcoded = new();
            }

            // ── Process each batch ─────────────────────────────────────────
            foreach (var local in batches)
            {
                try
                {
                    if (!destReachable)
                    {
                        Log($"[{destBranch}] {local.BatchNo} — destination unreachable. Will retry next tick.", LogLevel.Warning);
                        failed++;
                        continue;
                    }

                    destHeaders.TryGetValue(local.BatchNo, out var destHeader);

                    // ── Case A: Missing from dest — retry mirror write ─────
                    if (destHeader == null)
                    {
                        Log($"[{destBranch}] {local.BatchNo} not found on destination. Re-attempting mirror write...", LogLevel.Warning);
                        var success = await RetryMirrorWriteAsync(local, destBranch, destConn);
                        if (success) retried++;
                        else failed++;
                        continue;
                    }

                    // ── Open local context for all updates this batch ──────
                    using var localCtx = _factory.CreateContext(_localConn);
                    bool anyChange = false;

                    // ── 1. Sync Batch_Header ───────────────────────────────
                    var localHeader = localCtx.Batch_Header
                        .FirstOrDefault(b => b.BatchNo == local.BatchNo);

                    if (localHeader == null)
                    {
                        Log($"[{destBranch}] {local.BatchNo} — not found in local DB. Skipping.", LogLevel.Warning);
                        failed++;
                        continue;
                    }

                    if (ShouldSyncStatus(localHeader.Status, destHeader.Status))
                    {
                        var old = localHeader.Status;
                        localHeader.Status = destHeader.Status;

                        if (destHeader.ProcReceived.HasValue && !localHeader.ProcReceived.HasValue)
                            localHeader.ProcReceived = destHeader.ProcReceived;

                        if (destHeader.Completed.HasValue && !localHeader.Completed.HasValue)
                            localHeader.Completed = destHeader.Completed;

                        Log($"[{destBranch}] {local.BatchNo} — header: {old} → {destHeader.Status}.", LogLevel.Success);
                        synced++;
                        anyChange = true;
                    }

                    // ── 2. Sync Batch_Specimen ─────────────────────────────
                    var localSpecimens = localCtx.Batch_Specimen
                        .Where(s => s.BatchNo == local.BatchNo)
                        .ToList();

                    destSpecimens.TryGetValue(local.BatchNo, out var destSpecs);
                    var destSpecDict = (destSpecs ?? new())
                        .ToDictionary(s => s.SpecimenNo, s => s);

                    int batchSpecSynced = 0;
                    foreach (var localSpec in localSpecimens)
                    {
                        if (!destSpecDict.TryGetValue(localSpec.SpecimenNo, out var destSpec))
                            continue;

                        bool changed = false;

                        if (ShouldSyncStatus(localSpec.Status, destSpec.Status))
                        {
                            localSpec.Status = destSpec.Status;
                            changed = true;
                        }

                        if (!string.IsNullOrEmpty(destSpec.CancelReason) &&
                            string.IsNullOrEmpty(localSpec.CancelReason))
                        {
                            localSpec.CancelReason = destSpec.CancelReason;
                            localSpec.CancelledBy = destSpec.CancelledBy;
                            localSpec.CancelledAt = destSpec.CancelledAt;
                            changed = true;
                        }

                        if (changed) batchSpecSynced++;
                    }

                    if (batchSpecSynced > 0)
                    {
                        specimenSynced += batchSpecSynced;
                        anyChange = true;
                        Log($"[{destBranch}] {local.BatchNo} — {batchSpecSynced} specimen(s) synced.", LogLevel.Success);
                    }

                    // ── 3. Sync Batch_Specimen_Receiving ───────────────────
                    // Mirror new receiving rows from dest that don't exist locally
                    destReceiving.TryGetValue(local.BatchNo, out var destRecRows);
                    destRecRows ??= new();

                    if (destRecRows.Any())
                    {
                        // Load existing local receiving rows for this batch
                        var localRecRows = localCtx.Batch_Specimen_Receiving
                            .Where(r => r.BatchNo == local.BatchNo)
                            .ToList();

                        var localRecNos = localRecRows
                            .Select(r => r.SpecimenNo)
                            .ToHashSet(StringComparer.OrdinalIgnoreCase);

                        int batchRecSynced = 0;
                        foreach (var destRec in destRecRows)
                        {
                            if (localRecNos.Contains(destRec.SpecimenNo))
                                continue; // already exists locally

                            localCtx.Batch_Specimen_Receiving.Add(new Batch_Specimen_Receiving
                            {
                                SpecimenNo = destRec.SpecimenNo,
                                BatchNo = destRec.BatchNo,
                                ProcReceived = destRec.ProcReceived,
                                ProcReceivedBy = destRec.ProcReceivedBy,
                                ReceivingRemarks = destRec.ReceivingRemarks
                            });
                            batchRecSynced++;
                        }

                        if (batchRecSynced > 0)
                        {
                            receivingSynced += batchRecSynced;
                            anyChange = true;
                            Log($"[{destBranch}] {local.BatchNo} — {batchRecSynced} receiving record(s) mirrored.", LogLevel.Success);
                        }
                    }

                    // ── 4. Sync Batch_NonBarcoded ──────────────────────────
                    destNonBarcoded.TryGetValue(local.BatchNo, out var destNbRows);
                    destNbRows ??= new();

                    if (destNbRows.Any())
                    {
                        var localNbRows = localCtx.Batch_NonBarcoded
                            .Where(n => n.BatchNo == local.BatchNo)
                            .ToList();

                        // Match by Description + Type since there's no unique key
                        int batchNbSynced = 0;
                        foreach (var localNb in localNbRows)
                        {
                            var destNb = destNbRows.FirstOrDefault(d =>
                                d.Type == localNb.Type &&
                                d.Description == localNb.Description);

                            if (destNb == null) continue;

                            bool changed = false;

                            if (ShouldSyncStatus(localNb.Status, destNb.Status))
                            {
                                localNb.Status = destNb.Status;
                                changed = true;
                            }

                            if (destNb.ProcReceived.HasValue && !localNb.ProcReceived.HasValue)
                            {
                                localNb.ProcReceived = destNb.ProcReceived;
                                localNb.ProcReceivedBy = destNb.ProcReceivedBy;
                                changed = true;
                            }

                            if (!string.IsNullOrEmpty(destNb.ReceivingRemarks) &&
                                string.IsNullOrEmpty(localNb.ReceivingRemarks))
                            {
                                localNb.ReceivingRemarks = destNb.ReceivingRemarks;
                                changed = true;
                            }

                            if (changed) batchNbSynced++;
                        }

                        if (batchNbSynced > 0)
                        {
                            nonBarcodedSynced += batchNbSynced;
                            anyChange = true;
                            Log($"[{destBranch}] {local.BatchNo} — {batchNbSynced} non-barcoded item(s) synced.", LogLevel.Success);
                        }
                    }

                    // ── Commit all changes for this batch atomically ───────
                    if (anyChange)
                        localCtx.SaveChanges();
                }
                catch (Exception ex)
                {
                    Log($"[{destBranch}] {local.BatchNo} — error: {ex.Message}", LogLevel.Error);
                    failed++;
                }
            }

            return (synced, retried, failed, specimenSynced, receivingSynced, nonBarcodedSynced);
        }

        // Only sync forward: P < PA < C — never downgrade
        private static bool ShouldSyncStatus(string localStatus, string destStatus)
        {
            var order = new Dictionary<string, int>
            {
                { "P",  0 },
                { "PA", 1 },
                { "R",  2 },
                { "C",  3 }
            };

            var localRank = order.TryGetValue(localStatus, out var lr) ? lr : -1;
            var destRank = order.TryGetValue(destStatus, out var dr) ? dr : -1;

            return destRank > localRank;
        }

        private async Task<bool> RetryMirrorWriteAsync(
            Batch_Header local, string destBranch, string destConn)
        {
            try
            {
                List<Batch_Specimen> specimens;
                List<Batch_NonBarcoded> nonBarcoded;

                using (var localCtx = _factory.CreateContext(_localConn))
                {
                    specimens = localCtx.Batch_Specimen
                        .Where(s => s.BatchNo == local.BatchNo)
                        .ToList();

                    nonBarcoded = localCtx.Batch_NonBarcoded
                        .Where(n => n.BatchNo == local.BatchNo)
                        .ToList();
                }

                using var destCtx = _factory.CreateContext(destConn);
                using var destTx = destCtx.Database.BeginTransaction();

                try
                {
                    destCtx.Batch_Header.Add(new Batch_Header
                    {
                        BatchNo = local.BatchNo,
                        EndorsedBy = local.EndorsedBy,
                        Endorsed = local.Endorsed,
                        Location = local.Location,
                        ProcDestination = local.ProcDestination,
                        Status = "P",
                        IsOutbound = false,
                        IsInbound = true,
                        DestBranchCode = _localBranch
                    });
                    destCtx.SaveChanges();

                    foreach (var s in specimens)
                    {
                        destCtx.Batch_Specimen.Add(new Batch_Specimen
                        {
                            SpecimenNo = s.SpecimenNo,
                            BatchNo = s.BatchNo,
                            LabNo = s.LabNo,
                            TrxDate = s.TrxDate,
                            PID = s.PID,
                            PatientName = s.PatientName,
                            SampleTypeCode = s.SampleTypeCode,
                            SampleTypeName = s.SampleTypeName,
                            Endorsed = s.Endorsed,
                            EndorsedBy = s.EndorsedBy,
                            Status = "P",
                            Remarks = s.Remarks
                        });
                    }

                    foreach (var nb in nonBarcoded)
                    {
                        destCtx.Batch_NonBarcoded.Add(new Batch_NonBarcoded
                        {
                            BatchNo = nb.BatchNo,
                            Type = nb.Type,
                            LabNo = nb.LabNo,
                            Description = nb.Description,
                            Quantity = nb.Quantity,
                            Endorsed = nb.Endorsed,
                            EndorsedBy = nb.EndorsedBy,
                            Status = "P",
                            Remarks = nb.Remarks
                        });
                    }

                    destCtx.SaveChanges();
                    destTx.Commit();

                    Log($"[{destBranch}] {local.BatchNo} — mirror write successful.", LogLevel.Success);
                    return true;
                }
                catch
                {
                    destTx.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                Log($"[{destBranch}] {local.BatchNo} — mirror write failed: {ex.Message}", LogLevel.Error);
                return false;
            }

            await Task.CompletedTask;
            return false;
        }
    }
}