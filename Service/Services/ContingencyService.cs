using ClosedXML.Excel;
using Model.Main;
using Model.SETSDB;
using Reposi;
using Reposi.Context;
using Service.Interfaces;

namespace Service.Services
{
    public class ContingencyService : IContingencyService
    {
        private readonly AppDbContextFactory _factory;
        private readonly string _branch;
        private readonly string _branch_raw;

        public ContingencyService(AppDbContextFactory factory, string branch)
        {
            _factory = factory;
            _branch = SetsConnection.ConnectionString(branch);
            _branch_raw = branch;
        }

        // ── Config ─────────────────────────────────────────────────────────

        public ContingencyConfigResponse GetConfig()
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                var config = unit.ContingencyConfig.GetConfig();
                return new ContingencyConfigResponse
                {
                    IsEnabled = config?.IsEnabled ?? false,
                    HasPassword = !string.IsNullOrEmpty(config?.MasterPassword),
                    UpdatedBy = config?.UpdatedBy,
                    UpdatedAt = config?.UpdatedAt
                };
            }
            catch { throw; }
        }

        public void UpsertConfig(UpsertContingencyConfigRequest request, string userID)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                var config = unit.ContingencyConfig.GetConfig();
                if (config == null)
                {
                    config = new Contingency_Config
                    {
                        Id = 1,
                        IsEnabled = request.IsEnabled,
                        MasterPassword = request.MasterPassword ?? "",
                        UpdatedBy = userID,
                        UpdatedAt = DateTime.Now
                    };
                    unit.ContingencyConfig.Add(config);
                }
                else
                {
                    config.IsEnabled = request.IsEnabled;
                    if (!string.IsNullOrEmpty(request.MasterPassword))
                        config.MasterPassword = request.MasterPassword;
                    config.UpdatedBy = userID;
                    config.UpdatedAt = DateTime.Now;
                    unit.ContingencyConfig.Update(config);
                }
            }
            catch { throw; }
        }

        public bool ValidateMasterPassword(string password)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                var config = unit.ContingencyConfig.GetConfig();
                if (config == null || !config.IsEnabled) return false;
                return config.MasterPassword == password;
            }
            catch { throw; }
        }

        // ── Sample Types ───────────────────────────────────────────────────

        public List<SampleTypeItem> GetSampleTypes()
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                return unit.SampleTypes.GetAll()
                    .Where(s => s.Active)
                    .Select(s => new SampleTypeItem { Code = s.Code, Name = s.Name })
                    .ToList();
            }
            catch { throw; }
        }

        // ── Endorse ────────────────────────────────────────────────────────

        public string Endorse(ContingencyEndorseRequest request, string userID, string branch, string sectionCode)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);

                var batchNo = unit.ContingencyBatches.GenerateNextBatchNo(branch);
                var now = DateTime.Now;

                var batch = new Contingency_Batch
                {
                    BatchNo = batchNo,
                    EndorsingBranch = branch,
                    EndorsingSection = sectionCode,
                    EndorsedTo = request.EndorsedTo,
                    EndorsedBy = userID,
                    EndorsedAt = now,
                    Status = "Endorsed",
                    IsImported = false,
                    CreatedAt = now
                };
                unit.ContingencyBatches.Add(batch);

                foreach (var sp in request.Specimens)
                {
                    unit.ContingencySpecimens.Add(new Contingency_Specimen
                    {
                        BatchId = batch.Id,
                        SpecimenNo = sp.SpecimenNo,
                        SampleTypeCode = sp.SampleTypeCode,
                        SampleTypeName = sp.SampleTypeName,
                        IsReceived = false
                    });
                }

                return batchNo;
            }
            catch { throw; }
        }

        public List<ContingencyBatchListItem> GetBatchesByEndorsingBranch(string branchCode)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);

                var batches = unit.ContingencyBatches
                    .Query()
                    .Where(b => b.EndorsingBranch == branchCode)
                    .OrderByDescending(b => b.EndorsedAt)
                    .ToList();

                var result = new List<ContingencyBatchListItem>();
                foreach (var b in batches)
                {
                    var specimens = unit.ContingencySpecimens.GetByBatchId(b.Id);
                    result.Add(new ContingencyBatchListItem
                    {
                        Id = b.Id,
                        BatchNo = b.BatchNo,
                        EndorsingBranch = b.EndorsingBranch,
                        EndorsingSection = b.EndorsingSection,
                        EndorsedTo = b.EndorsedTo,
                        EndorsedBy = b.EndorsedBy,
                        EndorsedAt = b.EndorsedAt,
                        Status = b.Status,
                        IsImported = b.IsImported,
                        TotalSpecimens = specimens.Count,
                        ReceivedCount = specimens.Count(s => s.IsReceived)
                    });
                }
                return result;
            }
            catch { throw; }
        }

        // ── Excel Export ───────────────────────────────────────────────────

        public byte[] ExportBatchExcel(string batchNo)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);

                var batch = unit.ContingencyBatches.GetByBatchNo(batchNo)
                    ?? throw new Exception($"Batch '{batchNo}' not found.");

                var specimens = unit.ContingencySpecimens.GetByBatchId(batch.Id);

                using var wb = new XLWorkbook();
                var ws = wb.Worksheets.Add("Contingency");

                // Header info block
                ws.Cell("A1").Value = "CONTINGENCY ENDORSEMENT";
                ws.Cell("A1").Style.Font.Bold = true;
                ws.Cell("A1").Style.Font.FontSize = 14;
                ws.Range("A1:G1").Merge();

                ws.Cell("A2").Value = "Batch No:"; ws.Cell("B2").Value = batch.BatchNo;
                ws.Cell("A3").Value = "Endorsing Branch:"; ws.Cell("B3").Value = batch.EndorsingBranch;
                ws.Cell("A4").Value = "Endorsing Section:"; ws.Cell("B4").Value = batch.EndorsingSection;
                ws.Cell("A5").Value = "Endorsed To:"; ws.Cell("B5").Value = batch.EndorsedTo;
                ws.Cell("A6").Value = "Endorsed By:"; ws.Cell("B6").Value = batch.EndorsedBy;
                ws.Cell("A7").Value = "Endorsement Date:"; ws.Cell("B7").Value = batch.EndorsedAt.ToString("yyyy-MM-dd HH:mm:ss");

                // Style info block labels
                for (int r = 2; r <= 7; r++)
                    ws.Cell(r, 1).Style.Font.Bold = true;

                // Column headers row 9
                int headerRow = 9;
                ws.Cell(headerRow, 1).Value = "Batch No.";
                ws.Cell(headerRow, 2).Value = "Specimen No.";
                ws.Cell(headerRow, 3).Value = "Sample Type Code";
                ws.Cell(headerRow, 4).Value = "Sample Type";
                ws.Cell(headerRow, 5).Value = "Endorsed To";
                ws.Cell(headerRow, 6).Value = "Endorsed By";
                ws.Cell(headerRow, 7).Value = "Endorsement Date";

                var headerRange = ws.Range(headerRow, 1, headerRow, 7);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#1e3a5f");
                headerRange.Style.Font.FontColor = XLColor.White;

                // Data rows
                int row = headerRow + 1;
                foreach (var sp in specimens)
                {
                    ws.Cell(row, 1).Value = batch.BatchNo;
                    ws.Cell(row, 2).Value = sp.SpecimenNo;
                    ws.Cell(row, 3).Value = sp.SampleTypeCode;
                    ws.Cell(row, 4).Value = sp.SampleTypeName;
                    ws.Cell(row, 5).Value = batch.EndorsedTo;
                    ws.Cell(row, 6).Value = batch.EndorsedBy;
                    ws.Cell(row, 7).Value = batch.EndorsedAt.ToString("yyyy-MM-dd HH:mm:ss");

                    if (row % 2 == 0)
                        ws.Range(row, 1, row, 7).Style.Fill.BackgroundColor = XLColor.FromHtml("#f0f4f8");
                    row++;
                }

                ws.Columns().AdjustToContents();

                using var ms = new MemoryStream();
                wb.SaveAs(ms);
                return ms.ToArray();
            }
            catch { throw; }
        }

        // ── Batch List (Receiver) ──────────────────────────────────────────

        public List<ContingencyBatchListItem> GetBatchesByBranch(string branchCode)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);

                var batches = unit.ContingencyBatches.GetByEndorsedTo(branchCode);
                var result = new List<ContingencyBatchListItem>();

                foreach (var b in batches)
                {
                    var specimens = unit.ContingencySpecimens.GetByBatchId(b.Id);
                    result.Add(new ContingencyBatchListItem
                    {
                        Id = b.Id,
                        BatchNo = b.BatchNo,
                        EndorsingBranch = b.EndorsingBranch,
                        EndorsingSection = b.EndorsingSection,
                        EndorsedTo = b.EndorsedTo,
                        EndorsedBy = b.EndorsedBy,
                        EndorsedAt = b.EndorsedAt,
                        Status = b.Status,
                        IsImported = b.IsImported,
                        TotalSpecimens = specimens.Count,
                        ReceivedCount = specimens.Count(s => s.IsReceived)
                    });
                }

                return result;
            }
            catch { throw; }
        }

        public ContingencyBatchDetail GetBatchDetail(int batchId)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);

                var batch = unit.ContingencyBatches.Get(batchId)
                    ?? throw new Exception("Batch not found.");

                var specimens = unit.ContingencySpecimens.GetByBatchId(batchId);

                return new ContingencyBatchDetail
                {
                    Id = batch.Id,
                    BatchNo = batch.BatchNo,
                    EndorsingBranch = batch.EndorsingBranch,
                    EndorsingSection = batch.EndorsingSection,
                    EndorsedTo = batch.EndorsedTo,
                    EndorsedBy = batch.EndorsedBy,
                    EndorsedAt = batch.EndorsedAt,
                    Status = batch.Status,
                    Specimens = specimens.Select(s => new ContingencySpecimenDetail
                    {
                        Id = s.Id,
                        SpecimenNo = s.SpecimenNo,
                        SampleTypeCode = s.SampleTypeCode,
                        SampleTypeName = s.SampleTypeName,
                        IsReceived = s.IsReceived,
                        ReceivedBy = s.ReceivedBy,
                        ReceivedAt = s.ReceivedAt
                    }).ToList()
                };
            }
            catch { throw; }
        }

        // ── Scan / Receive ─────────────────────────────────────────────────

        public ContingencyScanResponse ScanSpecimen(ContingencyScanRequest request, string userID)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);

                var specimen = unit.ContingencySpecimens
                    .GetByBatchAndSpecimenNo(request.BatchId, request.SpecimenNo);

                if (specimen == null)
                    return new ContingencyScanResponse
                    {
                        Found = false,
                        Message = $"'{request.SpecimenNo}' is not in this batch."
                    };

                if (specimen.IsReceived)
                    return new ContingencyScanResponse
                    {
                        Found = true,
                        AlreadyReceived = true,
                        SpecimenId = specimen.Id,
                        Message = $"'{request.SpecimenNo}' was already received."
                    };

                specimen.IsReceived = true;
                specimen.ReceivedBy = userID;
                specimen.ReceivedAt = DateTime.Now;
                unit.ContingencySpecimens.Update(specimen);

                // Check if all received → complete batch
                var total = unit.ContingencySpecimens.CountTotal(request.BatchId);
                var received = unit.ContingencySpecimens.CountReceived(request.BatchId);
                bool allDone = received >= total;

                if (allDone)
                {
                    var batch = unit.ContingencyBatches.Get(request.BatchId);
                    if (batch != null)
                    {
                        batch.Status = "Completed";
                        unit.ContingencyBatches.Update(batch);
                    }
                }

                return new ContingencyScanResponse
                {
                    Found = true,
                    AlreadyReceived = false,
                    SpecimenId = specimen.Id,
                    BatchCompleted = allDone,
                    Message = allDone ? "All specimens received. Batch completed!" : "Specimen received."
                };
            }
            catch { throw; }
        }

        // ── Import from Excel ──────────────────────────────────────────────

        public ContingencyImportResult ImportFromExcel(byte[] fileBytes, string branchCode, string userID)
        {
            try
            {
                using var ms = new MemoryStream(fileBytes);
                using var wb = new XLWorkbook(ms);
                var ws = wb.Worksheets.First();

                // Read header block
                var batchNo = ws.Cell("B2").GetString().Trim();
                var endorsingBranch = ws.Cell("B3").GetString().Trim();
                var endorsingSection = ws.Cell("B4").GetString().Trim();
                var endorsedTo = ws.Cell("B5").GetString().Trim();
                var endorsedBy = ws.Cell("B6").GetString().Trim();
                var endorsedAtStr = ws.Cell("B7").GetString().Trim();

                if (string.IsNullOrEmpty(batchNo))
                    throw new Exception("Invalid file: Batch No. not found.");

                // Validate endorsed to matches this branch
                if (!endorsedTo.Equals(branchCode, StringComparison.OrdinalIgnoreCase))
                    throw new Exception($"This file is endorsed to '{endorsedTo}', not your branch ('{branchCode}'). Import blocked.");

                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);

                // Check if already imported
                var existing = unit.ContingencyBatches.GetByBatchNo(batchNo);
                if (existing != null)
                    return new ContingencyImportResult
                    {
                        Success = true,
                        BatchNo = batchNo,
                        AlreadyExists = true,
                        Message = $"Batch '{batchNo}' already exists in the system."
                    };

                // Parse endorsed date
                DateTime.TryParse(endorsedAtStr, out var endorsedAt);

                var batch = new Contingency_Batch
                {
                    BatchNo = batchNo,
                    EndorsingBranch = endorsingBranch,
                    EndorsingSection = endorsingSection,
                    EndorsedTo = endorsedTo,
                    EndorsedBy = endorsedBy,
                    EndorsedAt = endorsedAt,
                    Status = "Endorsed",
                    IsImported = true,
                    ImportedBy = userID,
                    ImportedAt = DateTime.Now,
                    CreatedAt = DateTime.Now
                };
                unit.ContingencyBatches.Add(batch);

                // Read specimen rows starting at row 10
                int specimenCount = 0;
                int row = 10;
                while (!ws.Cell(row, 2).IsEmpty())
                {
                    var specimenNo = ws.Cell(row, 2).GetString().Trim();
                    var sampleCode = ws.Cell(row, 3).GetString().Trim();
                    var sampleName = ws.Cell(row, 4).GetString().Trim();

                    if (!string.IsNullOrEmpty(specimenNo))
                    {
                        unit.ContingencySpecimens.Add(new Contingency_Specimen
                        {
                            BatchId = batch.Id,
                            SpecimenNo = specimenNo,
                            SampleTypeCode = sampleCode,
                            SampleTypeName = sampleName,
                            IsReceived = false
                        });
                        specimenCount++;
                    }
                    row++;
                }

                return new ContingencyImportResult
                {
                    Success = true,
                    BatchNo = batchNo,
                    SpecimenCount = specimenCount,
                    AlreadyExists = false,
                    Message = $"Imported {specimenCount} specimen(s) from batch '{batchNo}'."
                };
            }
            catch { throw; }
        }
    }
}