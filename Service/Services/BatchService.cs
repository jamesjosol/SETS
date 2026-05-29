using Model.Main;
using Model.SETSDB;
using Reposi;
using Reposi.Context;
using Service.Interfaces;

namespace Service.Services
{
    public class BatchService : IBatchService
    {
        private readonly AppDbContextFactory _factory;
        private readonly string _branch;
        private readonly string _branch_raw;

        public BatchService(AppDbContextFactory factory, string branch)
        {
            _factory = factory;
            _branch = SetsConnection.ConnectionString(branch);
            _branch_raw = branch;
        }

        public EndorseResult Endorse(EndorseRequest request)
        {
            try
            {
                var isOutbound = !string.IsNullOrEmpty(request.DestBranchCode) &&
                                 request.DestBranchCode != _branch_raw;

                using var context = _factory.CreateContext(_branch);
                using var tx = context.Database.BeginTransaction();

                string batchNo;
                DateTime now;

                try
                {
                    // 1. Fetch section and increment AutoNo
                    var section = context.Section_Master
                        .FirstOrDefault(s => s.Code == request.SectionCode)
                        ?? throw new Exception($"Section '{request.SectionCode}' not found.");

                    section.AutoNo += 1;

                    var yy = DateTime.Now.ToString("yy");
                    batchNo = $"{section.Code}{yy}-{section.AutoNo:D5}";
                    now = DateTime.Now;

                    // 2. Insert Batch_Header on Branch A (originating)
                    var header = new Batch_Header
                    {
                        BatchNo = batchNo,
                        EndorsedBy = request.UserID,
                        Endorsed = now,
                        Location = request.SectionCode,
                        ProcDestination = request.ProcDestination,
                        Status = "P",
                        IsOutbound = isOutbound,
                        IsInbound = false,
                        DestBranchCode = isOutbound ? request.DestBranchCode : null
                    };
                    context.Batch_Header.Add(header);
                    context.SaveChanges();

                    // 3. Insert Batch_Specimen on Branch A
                    foreach (var s in request.Specimens)
                    {
                        context.Batch_Specimen.Add(new Batch_Specimen
                        {
                            SpecimenNo = s.SpecimenNo,
                            BatchNo = batchNo,
                            LabNo = s.LabNo,
                            TrxDate = s.TrxDate.Kind == DateTimeKind.Unspecified
                                                ? DateTime.SpecifyKind(s.TrxDate, DateTimeKind.Local)
                                                : s.TrxDate,
                            PID = s.PID,
                            PatientName = s.PatientName,
                            SampleTypeCode = s.SampleTypeCode,
                            SampleTypeName = s.SampleTypeName,
                            Endorsed = now,
                            EndorsedBy = request.UserID,
                            Status = "P",
                            Remarks = s.Remarks
                        });
                    }

                    // 4. Insert Batch_NonBarcoded on Branch A
                    foreach (var nb in request.NonBarcoded)
                    {
                        context.Batch_NonBarcoded.Add(new Batch_NonBarcoded
                        {
                            BatchNo = batchNo,
                            Type = nb.Type ?? "others",
                            LabNo = nb.LabNo,
                            Description = nb.Description,
                            Quantity = nb.Quantity,
                            Endorsed = now,
                            EndorsedBy = request.UserID,
                            Status = "P",
                            Remarks = nb.Remarks
                        });
                    }

                    // 5. Commit Branch A
                    context.SaveChanges();
                    tx.Commit();
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }

                // ── Dual-write: Branch B (destination) ───────────────────────────
                // Runs AFTER Branch A commits — Branch A is source of truth.
                // If Branch B write fails, middleware will retry on next sync tick.
                if (isOutbound)
                {
                    try
                    {
                        var destConn = SetsConnection.ConnectionString(request.DestBranchCode!);
                        using var destContext = _factory.CreateContext(destConn);
                        using var destTx = destContext.Database.BeginTransaction();

                        try
                        {
                            // Mirror Batch_Header on Branch B
                            // IsInbound=true so Branch B's processing module can identify it
                            // ProcDestination = the processing section code on Branch B
                            var destHeader = new Batch_Header
                            {
                                BatchNo = batchNo,
                                EndorsedBy = request.UserID,
                                Endorsed = now,
                                Location = request.SectionCode,
                                ProcDestination = request.ProcDestination,
                                Status = "P",
                                IsOutbound = false,
                                IsInbound = true,
                                DestBranchCode = _branch_raw   // from Branch B's perspective, origin = Branch A
                            };
                            destContext.Batch_Header.Add(destHeader);
                            destContext.SaveChanges();

                            // Mirror Batch_Specimen on Branch B
                            foreach (var s in request.Specimens)
                            {
                                destContext.Batch_Specimen.Add(new Batch_Specimen
                                {
                                    SpecimenNo = s.SpecimenNo,
                                    BatchNo = batchNo,
                                    LabNo = s.LabNo,
                                    TrxDate = s.TrxDate.Kind == DateTimeKind.Unspecified
                                                         ? DateTime.SpecifyKind(s.TrxDate, DateTimeKind.Local)
                                                         : s.TrxDate,
                                    PID = s.PID,
                                    PatientName = s.PatientName,
                                    SampleTypeCode = s.SampleTypeCode,
                                    SampleTypeName = s.SampleTypeName,
                                    Endorsed = now,
                                    EndorsedBy = request.UserID,
                                    Status = "P",
                                    Remarks = s.Remarks
                                });
                            }

                            // Mirror Batch_NonBarcoded on Branch B
                            foreach (var nb in request.NonBarcoded)
                            {
                                destContext.Batch_NonBarcoded.Add(new Batch_NonBarcoded
                                {
                                    BatchNo = batchNo,
                                    Type = nb.Type ?? "others",
                                    LabNo = nb.LabNo,
                                    Description = nb.Description,
                                    Quantity = nb.Quantity,
                                    Endorsed = now,
                                    EndorsedBy = request.UserID,
                                    Status = "P",
                                    Remarks = nb.Remarks
                                });
                            }

                            destContext.SaveChanges();
                            destTx.Commit();

                            Console.WriteLine($"[OUTBOUND] Batch {batchNo} mirrored to {request.DestBranchCode} successfully.");
                        }
                        catch
                        {
                            destTx.Rollback();
                            throw;
                        }
                    }
                    catch (Exception destEx)
                    {
                        // Branch B write failed — log it, don't throw.
                        // Middleware OutboundStatusSyncTask will retry on next tick.
                        Console.WriteLine($"[OUTBOUND] Branch B write failed for {batchNo} → {request.DestBranchCode}: {destEx.Message}");
                    }
                }

                // ── TAT evaluation (Branch A only) ────────────────────────────────
                bool isOutsideTat = false;
                try
                {
                    using var master = new MasterService(_branch_raw);
                    isOutsideTat = master.Tat.EvaluateAndCycle(request.SectionCode, batchNo, now);

                    if (isOutsideTat)
                    {
                        using var ctx2 = _factory.CreateContext(_branch);
                        var committed = ctx2.Batch_Header.FirstOrDefault(h => h.BatchNo == batchNo);
                        if (committed != null)
                        {
                            committed.IsOutsideTat = true;
                            ctx2.SaveChanges();
                        }
                    }
                }
                catch (Exception tatEx)
                {
                    Console.WriteLine($"[TAT] EvaluateAndCycle failed for {batchNo}: {tatEx.Message}");
                }

                // ── Audit Logging ─────────────────────────────────────────────────
                try
                {
                    using var master = new MasterService(_branch_raw);
                    foreach (var s in request.Specimens)
                        master.Audit.Log(Audit_Log.Endorsed(
                            s, batchNo, request.SectionCode,
                            request.ProcDestination, request.UserID, isOutsideTat));
                }
                catch (Exception auditEx)
                {
                    Console.WriteLine($"[AUDIT] Logging failed for batch {batchNo}: {auditEx.Message}");
                }

                // ── Fetch section name for result ─────────────────────────────────
                using var finalCtx = _factory.CreateContext(_branch);
                var sectionForResult = finalCtx.Section_Master
                    .FirstOrDefault(s => s.Code == request.SectionCode);

                return new EndorseResult
                {
                    BatchNo = batchNo,
                    ProcDestination = request.ProcDestination,
                    LocationName = sectionForResult?.Name ?? request.SectionCode
                };
            }
            catch { throw; }
        }

        public Batch_Specimen? CheckSpecimen(string specimenNo)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                return context.Batch_Specimen
                    .FirstOrDefault(s => s.SpecimenNo == specimenNo);
            }
            catch { throw; }
        }

        public BatchSummary GetDashboardSummary(string sectionCode, DateTime date)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);

                var batches = context.Batch_Header
                    .Where(b =>
                        b.Location == sectionCode &&
                        b.Endorsed.Date == date.Date)
                    .ToList();

                return new BatchSummary
                {
                    TotalEndorsed = batches.Count,
                    // P = no specimen received, PA = partial — both count as Pending
                    Pending = batches.Count(b => b.Status == "P" || b.Status == "PA"),
                    // C = all specimens received
                    Received = batches.Count(b => b.Status == "C"),
                    // TAT setup deferred — always 0 for now
                    OutsideTAT = batches.Count(b => b.IsOutsideTat)
                };
            }
            catch { throw; }
        }

        public List<BatchSectionSummary> GetAllSectionsSummary(DateTime date)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);

                // Load all active sections for the branch
                var sections = context.Section_Master
                    .Where(s => s.Active && s.Category == "1")
                    .ToList();

                // Load all batch headers for today in one query
                var batches = context.Batch_Header
                    .Where(b => b.Endorsed.Date == date.Date)
                    .ToList();

                // Group batches by section (Location) and map to summary per section
                var result = sections.Select(s =>
                {
                    var sectionBatches = batches
                        .Where(b => b.Location == s.Code)
                        .ToList();

                    return new BatchSectionSummary
                    {
                        SectionCode = s.Code,
                        SectionName = s.Name,
                        SectionCategory = s.Category,
                        TotalEndorsed = sectionBatches.Count,
                        Pending = sectionBatches.Count(b => b.Status == "P" || b.Status == "PA"),
                        Received = sectionBatches.Count(b => b.Status == "C"),
                        OutsideTAT = sectionBatches.Count(b => b.IsOutsideTat)
                    };
                })
                .OrderBy(s => s.SectionCategory)
                .ThenBy(s => s.SectionCode)
                .ToList();

                return result;
            }
            catch { throw; }
        }

        public List<BatchRecentItem> GetRecentBatches(string sectionCode)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                var sections = context.Section_Master
                    .ToDictionary(s => s.Code, s => s.Name);

                var batches = context.Batch_Header
                    .Where(b => b.Location == sectionCode &&
                                b.Endorsed.Date == DateTime.Today)
                    .OrderByDescending(b => b.Endorsed)
                    .ToList();

                var batchNos = batches.Select(b => b.BatchNo).ToList();
                var unpostedBatchNos = GetBatchNosWithUnpostedSpecimens(context, batchNos);
                var flaggedBatchNos = GetBatchNosWithFlaggedSpecimens(context, batchNos);

                return batches.Select(b => new BatchRecentItem
                {
                    BatchNo = b.BatchNo,
                    Location = null,
                    Endorsed = b.Endorsed,
                    EndorsedBy = b.EndorsedBy,
                    Destination = sections.TryGetValue(b.ProcDestination, out var dest) ? dest : b.ProcDestination,
                    Status = b.Status,
                    HasUnpostedSpecimens = unpostedBatchNos.Contains(b.BatchNo),
                    HasFlaggedSpecimens = flaggedBatchNos.Contains(b.BatchNo),
                }).ToList();
            }
            catch { throw; }
        }

        public List<BatchRecentItem> GetAllSectionsRecentBatches()
        {
            try
            {
                using var context = _factory.CreateContext(_branch);

                // Load section name lookup in one query
                var sections = context.Section_Master
                    .ToDictionary(s => s.Code, s => s.Name);

                // Join Batch_Header with Section_Master to filter endorser sections only
                // Avoids Contains() which causes EF Core CTE syntax error on SQL Server
                var batches = (
                    from b in context.Batch_Header
                    join s in context.Section_Master
                        on b.Location equals s.Code
                    where s.Active && s.Category == "1" && b.Endorsed.Date == DateTime.Today
                    orderby b.Endorsed descending
                    select b
                )
                .ToList();

                var batchNos = batches.Select(b => b.BatchNo).ToList();
                var unpostedBatchNos = GetBatchNosWithUnpostedSpecimens(context, batchNos);
                var flaggedBatchNos = GetBatchNosWithFlaggedSpecimens(context, batchNos);

                return batches.Select(b => new BatchRecentItem
                {
                    BatchNo = b.BatchNo,
                    Location = sections.TryGetValue(b.Location, out var loc) ? loc : b.Location,
                    Endorsed = b.Endorsed,
                    EndorsedBy = b.EndorsedBy,
                    Destination = sections.TryGetValue(b.ProcDestination, out var dest) ? dest : b.ProcDestination,
                    Status = b.Status,
                    HasUnpostedSpecimens = unpostedBatchNos.Contains(b.BatchNo),
                    HasFlaggedSpecimens = flaggedBatchNos.Contains(b.BatchNo)
                }).ToList();
            }
            catch { throw; }
        }

        public BatchDetailResponse? GetBatchDetail(string batchNo)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);

                var header = context.Batch_Header
                    .FirstOrDefault(b => b.BatchNo == batchNo);

                if (header == null) return null;

                var sections = context.Section_Master
                    .ToDictionary(s => s.Code, s => s.Name);

                var specimens = context.Batch_Specimen
                    .Where(s => s.BatchNo == batchNo)
                    .OrderBy(s => s.SampleTypeName)
                    .ThenBy(s => s.SpecimenNo)
                    .ToList();

                // Load receiving records for this batch
                var receivingRecords = context.Batch_Specimen_Receiving
                    .Where(r => r.BatchNo == batchNo)
                    .ToList()
                    .ToDictionary(r => r.SpecimenNo, r => r);

                var nonBarcoded = context.Batch_NonBarcoded
                    .Where(n => n.BatchNo == batchNo)
                    .ToList();

                return new BatchDetailResponse
                {
                    BatchNo = header.BatchNo,
                    LocationCode = header.Location,
                    Location = sections.TryGetValue(header.Location, out var loc) ? loc : header.Location,
                    Endorsed = header.Endorsed,
                    EndorsedBy = header.EndorsedBy,
                    DestinationCode = header.ProcDestination,
                    Destination = sections.TryGetValue(header.ProcDestination, out var dest) ? dest : header.ProcDestination,
                    Status = header.Status,
                    ProcReceived = header.ProcReceived,
                    Completed = header.Completed,
                    Temp = header.Temp,        
                    TempRemarks = header.TempRemarks,  
                    BagNo = header.BagNo,
                    Specimens = specimens.Select(s => new BatchSpecimenDetail
                    {
                        Id = s.Id,
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
                        Status = s.Status,
                        Remarks = s.Remarks,
                        ReceivingRemarks = receivingRecords.TryGetValue(s.SpecimenNo, out var rr) ? rr.ReceivingRemarks : null,
                        SpecimenAlert = rr?.SpecimenAlert,
                        SpecimenAlertSetBy = rr?.SpecimenAlertSetBy,
                        SpecimenAlertSetAt = rr?.SpecimenAlertSetAt,
                        CancelReason = s.CancelReason,
                        CancelledBy = s.CancelledBy,
                        CancelledAt = s.CancelledAt,
                        IsPostedToDest = s.IsPostedToDest,
                        IsFlagged = s.IsFlagged,
                        FlagReason = s.FlagReason,
                        FlaggedBy = s.FlaggedBy,
                        FlaggedAt = s.FlaggedAt
                    }).ToList(),
                    NonBarcoded = nonBarcoded,
                    IsOutbound = header.IsOutbound,
                };
            }
            catch { throw; }
        }

        public List<BatchDailyFlow> GetWeeklyFlow(string sectionCode)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);

                var (monday, sunday) = GetCurrentWeekRange();

                var batches = context.Batch_Header
                    .Where(b =>
                        b.Location == sectionCode &&
                        b.Endorsed.Date >= monday &&
                        b.Endorsed.Date <= sunday)
                    .ToList();

                return BuildWeeklyFlow(batches, monday);
            }
            catch { throw; }
        }

        public List<BatchDailyFlow> GetAllSectionsWeeklyFlow()
        {
            try
            {
                using var context = _factory.CreateContext(_branch);

                var (monday, sunday) = GetCurrentWeekRange();

                // Join with Section_Master to filter endorser sections only
                var batches = (
                    from b in context.Batch_Header
                    join s in context.Section_Master
                        on b.Location equals s.Code
                    where s.Active &&
                          s.Category == "1" &&
                          b.Endorsed.Date >= monday &&
                          b.Endorsed.Date <= sunday
                    select b
                ).ToList();

                return BuildWeeklyFlow(batches, monday);
            }
            catch { throw; }
        }

        // ── Helpers ────────────────────────────────────────────────────────

        private static (DateTime Monday, DateTime Sunday) GetCurrentWeekRange()
        {
            var today = DateTime.Today;
            // DayOfWeek: Sunday=0, Monday=1 ... Saturday=6
            var diff = (int)today.DayOfWeek - (int)DayOfWeek.Monday;
            if (diff < 0) diff += 7;  // handle Sunday
            var monday = today.AddDays(-diff);
            var sunday = monday.AddDays(6);
            return (monday, sunday);
        }

        private static List<BatchDailyFlow> BuildWeeklyFlow(
            List<Batch_Header> batches, DateTime monday)
        {
            var today = DateTime.Today;
            var result = new List<BatchDailyFlow>();

            for (int i = 0; i < 7; i++)
            {
                var date = monday.AddDays(i);
                var count = batches.Count(b => b.Endorsed.Date == date);

                result.Add(new BatchDailyFlow
                {
                    Day = date.ToString("ddd"),   // "Mon", "Tue", etc.
                    Date = date.ToString("yyyy-MM-dd"),
                    Count = count,
                    IsToday = date == today
                });
            }

            return result;
        }

        private HashSet<string> GetBatchNosWithUnpostedSpecimens(
            AppDbContext context, List<string> batchNos)
        {
            if (!batchNos.Any()) return new HashSet<string>();

            // Step 1: From the given batch nos, find which ones are outbound
            var outboundBatchNos = context.Batch_Header
                .Where(h => h.IsOutbound)
                .ToList()
                .Where(h => batchNos.Contains(h.BatchNo))
                .Select(h => h.BatchNo)
                .ToHashSet();

            if (!outboundBatchNos.Any()) return new HashSet<string>();

            // Step 2: From those outbound batches, find which have at least one unposted specimen
            var unposted = context.Batch_Specimen
                .Where(s => !s.IsPostedToDest && s.Status != "X")
                .ToList()
                .Where(s => outboundBatchNos.Contains(s.BatchNo))
                .Select(s => s.BatchNo)
                .ToHashSet();

            return unposted;
        }

        private HashSet<string> GetBatchNosWithFlaggedSpecimens(
            AppDbContext context, List<string> batchNos)
        {
            if (!batchNos.Any()) return new HashSet<string>();

            return context.Batch_Specimen
                .Where(s => s.IsFlagged && s.Status == "P")
                .ToList()
                .Where(s => batchNos.Contains(s.BatchNo))
                .Select(s => s.BatchNo)
                .ToHashSet();
        }

        public List<BatchRecentItem> GetEndorsements(string sectionCode, DateTime dateFrom, DateTime dateTo)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);

                var sections = context.Section_Master
                    .ToDictionary(s => s.Code, s => s.Name);

                var batches = context.Batch_Header
                    .Where(b =>
                        b.Location == sectionCode &&
                        b.Endorsed.Date >= dateFrom.Date &&
                        b.Endorsed.Date <= dateTo.Date)
                    .OrderByDescending(b => b.Endorsed)
                    .ToList();

                var batchNos = batches.Select(b => b.BatchNo).ToList();
                var unpostedBatchNos = GetBatchNosWithUnpostedSpecimens(context, batchNos);
                var flaggedBatchNos = GetBatchNosWithFlaggedSpecimens(context, batchNos);

                return batches.Select(b => new BatchRecentItem
                {
                    BatchNo = b.BatchNo,
                    Location = null,
                    Endorsed = b.Endorsed,
                    EndorsedBy = b.EndorsedBy,
                    Destination = sections.TryGetValue(b.ProcDestination, out var dest) ? dest : b.ProcDestination,
                    Status = b.Status,
                    IsOutsideTat = b.IsOutsideTat,
                    HasUnpostedSpecimens = unpostedBatchNos.Contains(b.BatchNo),
                    HasFlaggedSpecimens = flaggedBatchNos.Contains(b.BatchNo)
                }).ToList();
            }
            catch { throw; }
        }

        public List<BatchRecentItem> GetAllEndorsements(DateTime dateFrom, DateTime dateTo)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);

                var sections = context.Section_Master
                    .ToDictionary(s => s.Code, s => s.Name);

                var batches = (
                    from b in context.Batch_Header
                    join s in context.Section_Master
                        on b.Location equals s.Code
                    where s.Active && s.Category == "1" &&
                          b.Endorsed.Date >= dateFrom.Date &&
                          b.Endorsed.Date <= dateTo.Date
                    orderby b.Endorsed descending
                    select b
                ).ToList();

                var batchNos = batches.Select(b => b.BatchNo).ToList();
                var unpostedBatchNos = GetBatchNosWithUnpostedSpecimens(context, batchNos);
                var flaggedBatchNos = GetBatchNosWithFlaggedSpecimens(context, batchNos);

                return batches.Select(b => new BatchRecentItem
                {
                    BatchNo = b.BatchNo,
                    Location = sections.TryGetValue(b.Location, out var loc) ? loc : b.Location,
                    Endorsed = b.Endorsed,
                    EndorsedBy = b.EndorsedBy,
                    Destination = sections.TryGetValue(b.ProcDestination, out var dest) ? dest : b.ProcDestination,
                    Status = b.Status,
                    IsOutsideTat = b.IsOutsideTat,
                    HasUnpostedSpecimens = unpostedBatchNos.Contains(b.BatchNo),
                    HasFlaggedSpecimens = flaggedBatchNos.Contains(b.BatchNo)
                }).ToList();
            }
            catch { throw; }
        }

        public List<GlobalSearchResult> GlobalSearch(string query, string userID, string sectionCategory, string sectionCode)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);

                var q = query.Trim();
                var cutoff = DateTime.Today.AddDays(-90);

                var sections = context.Section_Master
                    .ToDictionary(s => s.Code, s => s.Name);

                var results = new List<GlobalSearchResult>();

                // ══════════════════════════════════════════════════════════════
                // RUNNER (category 3) — searches Specimen_Section_Header
                // ══════════════════════════════════════════════════════════════
                if (sectionCategory == "3")
                {
                    // 1. Pull section headers for this section, within cutoff
                    var headers = context.Specimen_Section_Header
                        .Where(h => h.SectionCode == sectionCode && h.Routed >= cutoff)
                        .ToList();

                    // 2. Pull batch specimens in memory to join for patient info
                    //    Pre-filter by cutoff to avoid full table scan
                    var specimenNosInHeaders = headers
                      .Select(h => h.SpecimenNo)
                      .ToHashSet(StringComparer.OrdinalIgnoreCase);

                    var batchSpecimens = context.Batch_Specimen
                        .Where(s => s.Endorsed >= cutoff)
                        .ToList()
                        .Where(s => specimenNosInHeaders.Contains(s.SpecimenNo))
                        .GroupBy(s => s.SpecimenNo)
                        .ToDictionary(g => g.Key, g => g.OrderByDescending(s => s.Endorsed).First());

                    // 3. Filter by query across all relevant fields
                    var matches = headers
                        .Where(h =>
                        {
                            batchSpecimens.TryGetValue(h.SpecimenNo, out var bs);
                            return h.SpecimenNo.Contains(q, StringComparison.OrdinalIgnoreCase)
                                || (bs?.LabNo != null && bs.LabNo.Contains(q, StringComparison.OrdinalIgnoreCase))
                                || (bs?.PID != null && bs.PID.Contains(q, StringComparison.OrdinalIgnoreCase))
                                || (bs?.PatientName != null && bs.PatientName.Contains(q, StringComparison.OrdinalIgnoreCase));
                        })
                        .OrderByDescending(h => h.Routed)
                        .Take(10)
                        .ToList();

                    foreach (var h in matches)
                    {
                        batchSpecimens.TryGetValue(h.SpecimenNo, out var bs);
                        results.Add(new GlobalSearchResult
                        {
                            Type = "specimen",
                            SpecimenNo = h.SpecimenNo,
                            BatchNo = bs?.BatchNo ?? "",
                            LabNo = bs?.LabNo,
                            PID = bs?.PID,
                            PatientName = bs?.PatientName,
                            SampleTypeName = bs?.SampleTypeName ?? h.SampleTypeCode,
                            Status = h.Status,       // P, R, C from Specimen_Section_Header
                            LocationCode = sectionCode,
                            Location = sections.TryGetValue(sectionCode, out var sn) ? sn : sectionCode,
                            ProcDestination = sectionCode,
                            Endorsed = h.Routed
                        });
                    }

                    return results;
                }

                // ══════════════════════════════════════════════════════════════
                // ENDORSER (1) + RECEIVER (2) + ADMIN — searches Batch_Specimen
                // ══════════════════════════════════════════════════════════════

                // Materialize headers first (avoids CTE issue)
                var allHeaders = context.Batch_Header
                    .Where(h => h.Endorsed >= cutoff)
                    .ToList()
                    .ToDictionary(h => h.BatchNo);

                // Materialize specimens with date pre-filter
                var specimenRaw = context.Batch_Specimen
                    .Where(s => s.Endorsed >= cutoff)
                    .ToList();

                // Role scope in-memory
                specimenRaw = sectionCategory switch
                {
                    "1" => specimenRaw.Where(s => s.EndorsedBy == userID).ToList(),
                    "2" => specimenRaw
                                   .Where(s => allHeaders.TryGetValue(s.BatchNo, out var hdr)
                                            && hdr.ProcDestination == sectionCode)
                                   .ToList(),
                    _ => specimenRaw   // admin
                };

                var specimenMatches = specimenRaw
                    .Where(s =>
                        s.SpecimenNo.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                        s.LabNo.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                        (s.PID != null && s.PID.Contains(q, StringComparison.OrdinalIgnoreCase)) ||
                        (s.PatientName != null && s.PatientName.Contains(q, StringComparison.OrdinalIgnoreCase)))
                    .OrderByDescending(s => s.Endorsed)
                    .Take(8)
                    .ToList();

                // Batch search
                var batchRaw = allHeaders.Values.ToList();

                batchRaw = sectionCategory switch
                {
                    "1" => batchRaw.Where(h => h.EndorsedBy == userID).ToList(),
                    "2" => batchRaw.Where(h => h.ProcDestination == sectionCode).ToList(),
                    _ => batchRaw
                };

                var batchMatches = batchRaw
                    .Where(h => h.BatchNo.Contains(q, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(h => h.Endorsed)
                    .Take(5)
                    .ToList();

                // Build specimen results — reuse allHeaders (no second DB query)
                foreach (var s in specimenMatches)
                {
                    allHeaders.TryGetValue(s.BatchNo, out var hdr);
                    results.Add(new GlobalSearchResult
                    {
                        Type = "specimen",
                        SpecimenNo = s.SpecimenNo,
                        BatchNo = s.BatchNo,
                        LabNo = s.LabNo,
                        PID = s.PID,
                        PatientName = s.PatientName,
                        SampleTypeName = s.SampleTypeName,
                        Status = s.Status,
                        LocationCode = hdr?.Location ?? "",
                        Location = sections.TryGetValue(hdr?.Location ?? "", out var loc) ? loc : hdr?.Location ?? "",
                        ProcDestination = hdr?.ProcDestination ?? "",
                        Endorsed = s.Endorsed
                    });
                }

                // Build batch results
                foreach (var h in batchMatches)
                {
                    results.Add(new GlobalSearchResult
                    {
                        Type = "batch",
                        SpecimenNo = "",
                        BatchNo = h.BatchNo,
                        LabNo = null,
                        PID = null,
                        PatientName = null,
                        SampleTypeName = null,
                        Status = h.Status,
                        LocationCode = h.Location,
                        Location = sections.TryGetValue(h.Location, out var loc) ? loc : h.Location,
                        ProcDestination = h.ProcDestination,
                        Endorsed = h.Endorsed
                    });
                }

                return results
                    .OrderByDescending(r => r.Endorsed)
                    .ToList();
            }
            catch { throw; }
        }
    }
}