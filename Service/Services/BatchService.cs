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

        public BatchService(AppDbContextFactory factory, string branch)
        {
            _factory = factory;
            _branch = SetsConnection.ConnectionString(branch);
        }

        public string Endorse(EndorseRequest request)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var tx = context.Database.BeginTransaction();

                try
                {
                    // 1. Fetch section and increment AutoNo
                    var section = context.Section_Master
                        .FirstOrDefault(s => s.Code == request.SectionCode)
                        ?? throw new Exception($"Section '{request.SectionCode}' not found.");

                    section.AutoNo += 1;

                    // 2. Generate batch number: e.g. WS26-00001
                    var yy = DateTime.Now.ToString("yy");
                    var batchNo = $"{section.Code}{yy}-{section.AutoNo:D5}";

                    var now = DateTime.Now;

                    // 3. Insert Batch_Header
                    var header = new Batch_Header
                    {
                        BatchNo = batchNo,
                        EndorsedBy = request.UserID,
                        Endorsed = now,
                        Location = request.SectionCode,
                        ProcDestination = request.ProcDestination,
                        Status = "P"
                    };
                    context.Batch_Header.Add(header);
                    context.SaveChanges();  

                    // 4. Insert Batch_Specimen records
                    foreach (var s in request.Specimens)
                    {
                        context.Batch_Specimen.Add(new Batch_Specimen
                        {
                            SpecimenNo = s.SpecimenNo,
                            BatchNo = batchNo,
                            LabNo = s.LabNo,
                            TrxDate = s.TrxDate.Kind == DateTimeKind.Unspecified        // ← fix datetime2
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

                    // 5. Insert Batch_NonBarcoded records
                    foreach (var nb in request.NonBarcoded)
                    {
                        context.Batch_NonBarcoded.Add(new Batch_NonBarcoded
                        {
                            BatchNo = batchNo,
                            Description = nb.Description,
                            Quantity = nb.Quantity,
                            Endorsed = now,
                            EndorsedBy = request.UserID,
                            Status = "P",
                            Remarks = nb.Remarks
                        });
                    }

                    // 6. Single commit — AutoNo + all inserts atomic
                    context.SaveChanges();
                    tx.Commit();

                    return batchNo;
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
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
                    OutsideTAT = 0
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
                        OutsideTAT = 0
                    };
                })
                .OrderBy(s => s.SectionCategory)
                .ThenBy(s => s.SectionCode)
                .ToList();

                return result;
            }
            catch { throw; }
        }

        public List<BatchRecentItem> GetRecentBatches(string sectionCode, int top = 5)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);

                // Load section name lookup in one query
                var sections = context.Section_Master
                    .ToDictionary(s => s.Code, s => s.Name);

                var batches = context.Batch_Header
                    .Where(b => b.Location == sectionCode)
                    .OrderByDescending(b => b.Endorsed)
                    .Take(top)
                    .ToList();

                return batches.Select(b => new BatchRecentItem
                {
                    BatchNo = b.BatchNo,
                    Location = null,
                    Endorsed = b.Endorsed,
                    EndorsedBy = b.EndorsedBy,
                    Destination = sections.TryGetValue(b.ProcDestination, out var dest) ? dest : b.ProcDestination,
                    Status = b.Status
                }).ToList();
            }
            catch { throw; }
        }

        public List<BatchRecentItem> GetAllSectionsRecentBatches(int top = 5)
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
                    where s.Active && s.Category == "1"
                    orderby b.Endorsed descending
                    select b
                )
                .Take(top)
                .ToList();

                return batches.Select(b => new BatchRecentItem
                {
                    BatchNo = b.BatchNo,
                    Location = sections.TryGetValue(b.Location, out var loc) ? loc : b.Location,
                    Endorsed = b.Endorsed,
                    EndorsedBy = b.EndorsedBy,
                    Destination = sections.TryGetValue(b.ProcDestination, out var dest) ? dest : b.ProcDestination,
                    Status = b.Status
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
                    .ToDictionary(r => r.SpecimenNo, r => r.ReceivingRemarks);

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
                        ReceivingRemarks = receivingRecords.TryGetValue(s.SpecimenNo, out var rr) ? rr : null
                    }).ToList(),
                    NonBarcoded = nonBarcoded
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
    }
}