using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Main;
using Model.SETSDB;
using Reposi;
using Reposi.Context;
using Service.Interfaces;

namespace Service.Services
{
    public class ReceivingService : IReceivingService
    {
        private readonly AppDbContextFactory _factory;
        private readonly string _branch;

        public ReceivingService(AppDbContextFactory factory, string branch)
        {
            _factory = factory;
            _branch = SetsConnection.ConnectionString(branch);
        }

        public ReceiveSpecimenResponse ReceiveSpecimen(ReceiveSpecimenRequest request)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var tx = context.Database.BeginTransaction();

                try
                {
                    var now = DateTime.Now;

                    // 1. Find the specimen — must exist and be pending
                    var specimen = context.Batch_Specimen
                        .FirstOrDefault(s => s.SpecimenNo == request.SpecimenNo)
                        ?? throw new Exception($"Specimen '{request.SpecimenNo}' was not endorsed.");

                    // 2. Batch restriction — if a current batch is active, block different batch
                    if (!string.IsNullOrEmpty(request.CurrentBatchNo) &&
                        specimen.BatchNo != request.CurrentBatchNo)
                    {
                        throw new Exception(
                            $"Specimen '{request.SpecimenNo}' belongs to batch {specimen.BatchNo}. " +
                            $"Currently receiving batch {request.CurrentBatchNo}. " +
                            $"Clear the list to receive a different batch."
                        );
                    }

                    // 3. Check if already received
                    bool alreadyReceived = context.Batch_Specimen_Receiving
                        .Any(r => r.SpecimenNo == request.SpecimenNo && r.BatchNo == specimen.BatchNo);

                    if (alreadyReceived)
                        throw new Exception($"Specimen '{request.SpecimenNo}' has already been received.");


                    // 3. Insert receiving record
                    context.Batch_Specimen_Receiving.Add(new Batch_Specimen_Receiving
                    {
                        SpecimenNo = request.SpecimenNo,
                        BatchNo = specimen.BatchNo,
                        ProcReceived = now,
                        ProcReceivedBy = request.UserID,
                        ReceivingRemarks = request.ReceivingRemarks
                    });

                    // 4. Update specimen status
                    specimen.Status = "R";
                    context.Batch_Specimen.Update(specimen);

                    // 5. Stamp ProcReceived on Batch_Header if this is the first specimen received
                    var header = context.Batch_Header
                        .FirstOrDefault(b => b.BatchNo == specimen.BatchNo)
                        ?? throw new Exception($"Batch '{specimen.BatchNo}' not found.");

                    if (header.ProcReceived == null)
                    {
                        header.ProcReceived = now;
                    }

                    // 6. Check if batch is now fully complete
                    // Save first so counts reflect this receive
                    context.SaveChanges();

                    string newBatchStatus = ResolveBatchStatus(context, specimen.BatchNo);
                    header.Status = newBatchStatus;

                    if (newBatchStatus == "C")
                        header.Completed = now;

                    var sections = context.Section_Master
                        .ToDictionary(s => s.Code, s => s.Name);

                    context.Batch_Header.Update(header);
                    context.SaveChanges();

                    tx.Commit();

                    return new ReceiveSpecimenResponse
                    {
                        SpecimenNo = specimen.SpecimenNo,
                        BatchNo = specimen.BatchNo,
                        PID = specimen.PID,          // ← add
                        PatientName = specimen.PatientName,
                        SampleTypeName = specimen.SampleTypeName,
                        Location = header.Location,       // ← add
                        LocationName = sections.TryGetValue(header.Location, out var locName) ? locName : header.Location,
                        ProcReceived = now,
                        ProcReceivedBy = request.UserID,
                        BatchStatus = newBatchStatus,
                        Temp = header.Temp,        // ← carry existing temp
                        TempRemarks = header.TempRemarks,
                        BagNo = header.BagNo
                    };
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
            catch { throw; }
        }
        public void UpdateBatchTemp(UpdateBatchTempRequest request)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);

                var header = context.Batch_Header
                    .FirstOrDefault(b => b.BatchNo == request.BatchNo)
                    ?? throw new Exception($"Batch '{request.BatchNo}' not found.");

                header.Temp = request.Temp;
                header.TempRemarks = request.TempRemarks;
                header.BagNo = request.BagNo;

                context.Batch_Header.Update(header);
                context.SaveChanges();
            }
            catch { throw; }
        }
        public void UpdateSpecimenReceivingRemarks(string specimenNo, string batchNo, string? remarks)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);

                var record = context.Batch_Specimen_Receiving
                    .FirstOrDefault(r => r.SpecimenNo == specimenNo && r.BatchNo == batchNo)
                    ?? throw new Exception($"Receiving record for specimen '{specimenNo}' not found.");

                record.ReceivingRemarks = remarks;
                context.Batch_Specimen_Receiving.Update(record);
                context.SaveChanges();
            }
            catch { throw; }
        }

        public ReceiveResponse ReceiveNonBarcoded(ReceiveNonBarcodedRequest request)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var tx = context.Database.BeginTransaction();

                try
                {
                    var now = DateTime.Now;

                    if (request.ItemIDs == null || request.ItemIDs.Count == 0)
                        throw new Exception("No items selected for receiving.");

                    // 1. Load selected non-barcoded items
                    var ids = new HashSet<int>(request.ItemIDs);

                    var items = context.Batch_NonBarcoded
                        .Where(n => n.Status == "P")
                        .ToList()
                        .Where(n => ids.Contains(n.ItemID))
                        .ToList();

                    if (items.Count == 0)
                        throw new Exception("No valid pending items found for the selected IDs.");

                    // All items should belong to the same batch
                    var batchNos = items.Select(i => i.BatchNo).Distinct().ToList();
                    if (batchNos.Count > 1)
                        throw new Exception("Selected items belong to multiple batches.");

                    var batchNo = batchNos.First();

                    // 2. Mark each item as received
                    foreach (var item in items)
                    {
                        item.ProcReceived = now;
                        item.ProcReceivedBy = request.UserID;
                        item.Status = "R";
                        context.Batch_NonBarcoded.Update(item);
                    }

                    // 3. Stamp ProcReceived on Batch_Header if first receive action
                    var header = context.Batch_Header
                        .FirstOrDefault(b => b.BatchNo == batchNo)
                        ?? throw new Exception($"Batch '{batchNo}' not found.");

                    if (header.ProcReceived == null)
                        header.ProcReceived = now;

                    context.SaveChanges();

                    // 4. Resolve batch status
                    string newBatchStatus = ResolveBatchStatus(context, batchNo);
                    header.Status = newBatchStatus;

                    if (newBatchStatus == "C")
                        header.Completed = now;

                    context.Batch_Header.Update(header);
                    context.SaveChanges();

                    tx.Commit();

                    // 5. Build response counts
                    var allSpecimens = context.Batch_Specimen
                        .Where(s => s.BatchNo == batchNo).ToList();
                    var allNonBarcoded = context.Batch_NonBarcoded
                        .Where(n => n.BatchNo == batchNo).ToList();

                    return new ReceiveResponse
                    {
                        BatchNo = batchNo,
                        Status = newBatchStatus,
                        IsCompleted = newBatchStatus == "C",
                        TotalSpecimens = allSpecimens.Count,
                        ReceivedSpecimens = allSpecimens.Count(s => s.Status == "R"),
                        TotalNonBarcoded = allNonBarcoded.Count,
                        ReceivedNonBarcoded = allNonBarcoded.Count(n => n.Status == "R")
                    };
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
            catch { throw; }
        }

        public void UpdateNonBarcodedReceivingRemarks(int itemID, string? remarks)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);

                var record = context.Batch_NonBarcoded
                    .FirstOrDefault(n => n.ItemID == itemID)
                    ?? throw new Exception($"Non-barcoded item '{itemID}' not found.");

                record.ReceivingRemarks = remarks;
                context.Batch_NonBarcoded.Update(record);
                context.SaveChanges();
            }
            catch { throw; }
        }

        public List<PendingNonBarcodedItem> GetPendingNonBarcoded(string sectionCode)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);

                var sections = context.Section_Master
                    .ToDictionary(s => s.Code, s => s.Name);

                // Pull data into memory first, then project
                var raw = context.Batch_NonBarcoded
                    .Where(n => n.Status == "P")
                    .Join(context.Batch_Header,
                        n => n.BatchNo,
                        h => h.BatchNo,
                        (n, h) => new { NonBarcoded = n, Header = h })
                    .Where(x => x.Header.ProcDestination == sectionCode)
                    .OrderBy(x => x.Header.Location)
                    .ThenBy(x => x.NonBarcoded.BatchNo)
                    .ThenBy(x => x.NonBarcoded.Description)
                    .ToList(); // ← materialize first

                // Project in memory — safe to use TryGetValue here
                return raw.Select(x => new PendingNonBarcodedItem
                {
                    ItemID = x.NonBarcoded.ItemID,
                    BatchNo = x.NonBarcoded.BatchNo,
                    Location = x.Header.Location,
                    LocationName = sections.TryGetValue(x.Header.Location, out var name) ? name : x.Header.Location,
                    Description = x.NonBarcoded.Description,
                    Quantity = x.NonBarcoded.Quantity,
                    Endorsed = x.NonBarcoded.Endorsed,
                    EndorsedBy = x.NonBarcoded.EndorsedBy,
                    Remarks = x.NonBarcoded.Remarks,
                    ReceivingRemarks = x.NonBarcoded.ReceivingRemarks
                }).ToList();
            }
            catch { throw; }
        }

        // ── Helpers ────────────────────────────────────────────────────────

        private static string ResolveBatchStatus(AppDbContext context, string batchNo)
        {
            var allSpecimens = context.Batch_Specimen
                .Where(s => s.BatchNo == batchNo).ToList();

            var allNonBarcoded = context.Batch_NonBarcoded
                .Where(n => n.BatchNo == batchNo).ToList();

            bool allSpecimensReceived = allSpecimens.All(s => s.Status == "R");
            bool allNonBarcodedReceived = allNonBarcoded.All(n => n.Status == "R");

            if (allSpecimensReceived && allNonBarcodedReceived)
                return "C";

            bool anyReceived = allSpecimens.Any(s => s.Status == "R")
                            || allNonBarcoded.Any(n => n.Status == "R");

            return anyReceived ? "PA" : "P";
        }

        public BatchSummary GetReceiverDashboardSummary(string procSectionCode, DateTime date)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);

                var batches = context.Batch_Header
                    .Where(b => b.ProcDestination == procSectionCode &&
                                b.Endorsed.Date == date.Date)
                    .ToList();

                return new BatchSummary
                {
                    TotalEndorsed = batches.Count,
                    Pending = batches.Count(b => b.Status == "P"),
                    Received = batches.Count(b => b.Status == "C"),
                    OutsideTAT = batches.Count(b => b.Status == "PA")  // reusing OutsideTAT as Partial for now
                };
            }
            catch { throw; }
        }

        public List<MonitoringDashboardSection> GetMonitoringDashboard(string procSectionCode, DateTime date)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);

                // Load all endorser sections (category = "1")
                var endorserSections = context.Section_Master
                    .Where(s => s.Category == "1" && s.Active)
                    .OrderBy(s => s.Name)
                    .ToList();

                // Load all batches for today destined for this processing section
                var batches = context.Batch_Header
                    .Where(b => b.ProcDestination == procSectionCode &&
                                b.Endorsed.Date == date.Date)
                    .ToList();

                // Load all batch numbers first
                var batchNos = batches.Select(b => b.BatchNo).ToList();

                // Fetch specimens in memory then group
                var specimenCounts = context.Batch_Specimen
                    .Where(s => s.Status != null)   // fetch all, filter by batchNo in memory
                    .ToList()
                    .Where(s => batchNos.Contains(s.BatchNo))
                    .GroupBy(s => s.BatchNo)
                    .ToDictionary(
                        g => g.Key,
                        g => new { Total = g.Count(), Received = g.Count(s => s.Status == "R") }
                    );

                // Group batches by endorsing section
                return endorserSections.Select(section =>
                {
                    var sectionBatches = batches
                        .Where(b => b.Location == section.Code)
                        .OrderBy(b => b.Endorsed)
                        .Select(b => new MonitoringBatchItem
                        {
                            BatchNo = b.BatchNo,
                            Status = b.Status,
                            Endorsed = b.Endorsed,
                            EndorsedBy = b.EndorsedBy,
                            TotalSpecimens = specimenCounts.TryGetValue(b.BatchNo, out var c) ? c.Total : 0,
                            ReceivedSpecimens = specimenCounts.TryGetValue(b.BatchNo, out var c2) ? c2.Received : 0
                        })
                        .ToList();

                    return new MonitoringDashboardSection
                    {
                        SectionCode = section.Code,
                        SectionName = section.Name,
                        Batches = sectionBatches
                    };
                }).ToList();
            }
            catch { throw; }
        }

        public List<BatchDailyFlow> GetWeeklyReceivedFlow(string procSectionCode)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);

                var (monday, sunday) = GetCurrentWeekRange();

                // Count specimens received per day via Batch_Specimen_Receiving
                // joined to Batch_Header to filter by processing destination
                var raw = context.Batch_Specimen_Receiving
                    .Join(context.Batch_Header,
                        r => r.BatchNo,
                        h => h.BatchNo,
                        (r, h) => new { Receiving = r, Header = h })
                    .Where(x => x.Header.ProcDestination == procSectionCode &&
                                x.Receiving.ProcReceived.Date >= monday &&
                                x.Receiving.ProcReceived.Date <= sunday)
                    .ToList();

                var today = DateTime.Today;
                var result = new List<BatchDailyFlow>();

                for (int i = 0; i < 7; i++)
                {
                    var date = monday.AddDays(i);
                    var count = raw.Count(x => x.Receiving.ProcReceived.Date == date);

                    result.Add(new BatchDailyFlow
                    {
                        Day = date.ToString("ddd"),
                        Date = date.ToString("yyyy-MM-dd"),
                        Count = count,
                        IsToday = date == today
                    });
                }

                return result;
            }
            catch { throw; }
        }

        public List<HourlyFlow> GetHourlyReceivedFlow(string procSectionCode)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);

                var today = DateTime.Today;
                var currentHour = DateTime.Now.Hour;

                // Load today's receiving records for this section
                var raw = context.Batch_Specimen_Receiving
                    .Join(context.Batch_Header,
                        r => r.BatchNo,
                        h => h.BatchNo,
                        (r, h) => new { Receiving = r, Header = h })
                    .Where(x => x.Header.ProcDestination == procSectionCode &&
                                x.Receiving.ProcReceived.Date == today)
                    .ToList();

                var result = new List<HourlyFlow>();

                // 5AM to 7PM = hours 5 to 19
                for (int hour = 5; hour <= 19; hour++)
                {
                    var count = raw.Count(x => x.Receiving.ProcReceived.Hour == hour);
                    result.Add(new HourlyFlow
                    {
                        Hour = $"{hour:D2}:00",
                        Count = count,
                        IsCurrent = hour == currentHour
                    });
                }

                return result;
            }
            catch { throw; }
        }

        private static (DateTime Monday, DateTime Sunday) GetCurrentWeekRange()
        {
            var today = DateTime.Today;
            var diff = (int)today.DayOfWeek - (int)DayOfWeek.Monday;
            if (diff < 0) diff += 7;
            var monday = today.AddDays(-diff);
            return (monday, monday.AddDays(6));
        }

        public List<IncomingBatchItem> GetIncomingBatches(string procSectionCode)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);

                var sections = context.Section_Master
                    .ToDictionary(s => s.Code, s => s.Name);

                // All non-completed batches destined for this processing section
                var headers = context.Batch_Header
                    .Where(h => h.ProcDestination == procSectionCode &&
                                h.Status != "C")
                    .OrderByDescending(h => h.Endorsed)
                    .ToList();

                var batchNos = headers.Select(h => h.BatchNo).ToList();

                // Specimen counts per batch — in memory to avoid CTE error
                var specimens = context.Batch_Specimen
                    .Where(s => s.Status != null)
                    .ToList()
                    .Where(s => batchNos.Contains(s.BatchNo))
                    .GroupBy(s => s.BatchNo)
                    .ToDictionary(
                        g => g.Key,
                        g => new { Total = g.Count(), Received = g.Count(s => s.Status == "R") }
                    );

                return headers.Select(h => new IncomingBatchItem
                {
                    BatchNo = h.BatchNo,
                    Location = h.Location,
                    LocationName = sections.TryGetValue(h.Location, out var name) ? name : h.Location,
                    Endorsed = h.Endorsed,
                    EndorsedBy = h.EndorsedBy,
                    Status = h.Status,
                    TotalSpecimens = specimens.TryGetValue(h.BatchNo, out var c) ? c.Total : 0,
                    ReceivedSpecimens = specimens.TryGetValue(h.BatchNo, out var c2) ? c2.Received : 0
                }).ToList();
            }
            catch { throw; }
        }

        public List<IncomingSpecimenItem> GetIncomingSpecimens(string procSectionCode)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);

                var sections = context.Section_Master
                    .ToDictionary(s => s.Code, s => s.Name);

                // Get all non-completed batch headers for this section
                var pendingBatchHeaders = context.Batch_Header
                    .Where(h => h.ProcDestination == procSectionCode &&
                                h.Status != "C")
                    .ToList();

                var pendingBatchNos = pendingBatchHeaders
                    .Select(h => h.BatchNo)
                    .ToList();

                // Reuse the same result for locations — no second query needed
                var batchLocations = pendingBatchHeaders
                    .ToDictionary(h => h.BatchNo, h => h.Location);

                // Get all pending specimens from those batches — in memory
                var specimens = context.Batch_Specimen
                    .Where(s => s.Status == "P")
                    .ToList()
                    .Where(s => pendingBatchNos.Contains(s.BatchNo))
                    .OrderByDescending(s => s.Endorsed)
                    .ThenBy(s => s.BatchNo)
                    .ThenBy(s => s.SpecimenNo)
                    .ToList();

                return specimens.Select(s =>
                {
                    var locationCode = batchLocations.TryGetValue(s.BatchNo, out var loc) ? loc : "";
                    return new IncomingSpecimenItem
                    {
                        BatchNo = s.BatchNo,
                        Location = locationCode,
                        LocationName = sections.TryGetValue(locationCode, out var name) ? name : locationCode,
                        SpecimenNo = s.SpecimenNo,
                        LabNo = s.LabNo,
                        PID = s.PID,
                        PatientName = s.PatientName,
                        SampleTypeName = s.SampleTypeName,
                        Status = s.Status,
                        Remarks = s.Remarks
                    };
                }).ToList();
            }
            catch { throw; }
        }
    }
}