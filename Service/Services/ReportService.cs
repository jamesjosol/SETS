using ClosedXML.Excel;
using Model.Main;
using Model.SETSDB;
using Reposi;
using Reposi.Context;
using Service.Interfaces;

namespace Service.Services
{
    public class ReportService : IReportService
    {
        private readonly AppDbContextFactory _factory;
        private readonly string _branch;
        private readonly string _branch_raw;

        public ReportService(AppDbContextFactory factory, string branch)
        {
            _factory = factory;
            _branch = SetsConnection.ConnectionString(branch);
            _branch_raw = branch;
        }

        // ── TAT helpers ───────────────────────────────────────────────────────

        private static string? FormatTat(DateTime? from, DateTime? to)
        {
            if (from == null || to == null) return null;
            var span = to.Value - from.Value;
            if (span.TotalMinutes < 0) return null;
            return $"{(int)span.TotalHours:D2}:{span.Minutes:D2}";
        }

        private static double? TatMinutes(DateTime? from, DateTime? to)
        {
            if (from == null || to == null) return null;
            var minutes = (to.Value - from.Value).TotalMinutes;
            return minutes < 0 ? null : minutes;
        }

        private static string? MinutesToHhMm(double? minutes)
        {
            if (minutes == null) return null;
            var h = (int)(minutes.Value / 60);
            var m = (int)(minutes.Value % 60);
            return $"{h:D2}:{m:D2}";
        }

        private static string StatusLabel(string status) => status switch
        {
            "C" => "Complete",
            "PA" => "Partial",
            "P" => "Pending",
            "X" => "Cancelled",
            _ => status
        };

        // ══════════════════════════════════════════════════════════════════════
        // R5 — Batch Summary (fully implemented)
        // ══════════════════════════════════════════════════════════════════════

        public BatchSummaryReportResult GetBatchSummary(BatchSummaryReportRequest request)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);

                // ── 1. Endorser sections (Category = "1") for location name lookup ──
                var sections = context.Section_Master
                    .Where(s => s.Category == "1")
                    .ToList()
                    .ToDictionary(s => s.Code, s => s.Name);

                // ── 2. Batch headers — materialise first (avoids EF CTE issue) ──
                var allHeaders = context.Batch_Header
                    .Where(h =>
                        h.Endorsed.Date >= request.DateFrom.Date &&
                        h.Endorsed.Date <= request.DateTo.Date)
                    .ToList();

                // Optional location filter in-memory
                if (!string.IsNullOrEmpty(request.LocationCode))
                    allHeaders = allHeaders.Where(h => h.Location == request.LocationCode).ToList();

                if (!allHeaders.Any())
                    return new BatchSummaryReportResult();

                var batchNos = allHeaders.Select(h => h.BatchNo).ToList();

                // ── 3. Receiving records grouped by BatchNo for receiver lookup ──
                var receivingRecords = context.Batch_Specimen_Receiving
                    .ToList()
                    .Where(r => batchNos.Contains(r.BatchNo))
                    .GroupBy(r => r.BatchNo)
                    .ToDictionary(
                        g => g.Key,
                        g => string.Join(", ", g.Select(r => r.ProcReceivedBy)
                                                  .Distinct()
                                                  .OrderBy(u => u))
                    );

                // ── 4. Build rows ─────────────────────────────────────────────
                var rows = allHeaders
                    .OrderByDescending(h => h.Endorsed)
                    .Select(h => new BatchSummaryReportRow
                    {
                        BatchNo = h.BatchNo,
                        LocationCode = h.Location,
                        Location = sections.TryGetValue(h.Location, out var loc) ? loc : h.Location,
                        Endorsed = h.Endorsed,
                        EndorsedBy = h.EndorsedBy,
                        ProcReceived = h.ProcReceived,
                        ReceivedBy = receivingRecords.TryGetValue(h.BatchNo, out var rcvBy) ? rcvBy : null,
                        Completed = h.Completed,
                        Temp = h.Temp,
                        Status = StatusLabel(h.Status),
                        TatEndorsement = FormatTat(h.Endorsed, h.ProcReceived),
                        TatCompletion = FormatTat(h.ProcReceived, h.Completed),
                    })
                    .ToList();

                // ── 5. Averages ───────────────────────────────────────────────
                var tatEndMinutes = allHeaders
                    .Select(h => TatMinutes(h.Endorsed, h.ProcReceived))
                    .Where(m => m != null)
                    .Select(m => m!.Value)
                    .ToList();

                var tatCompMinutes = allHeaders
                    .Select(h => TatMinutes(h.ProcReceived, h.Completed))
                    .Where(m => m != null)
                    .Select(m => m!.Value)
                    .ToList();

                return new BatchSummaryReportResult
                {
                    Rows = rows,
                    TotalBatches = rows.Count,
                    CompletedBatches = rows.Count(r => r.Status == "Complete"),
                    PendingBatches = rows.Count(r => r.Status is "Pending" or "Partial"),
                    AvgTatEndorsement = tatEndMinutes.Any() ? MinutesToHhMm(tatEndMinutes.Average()) : null,
                    AvgTatCompletion = tatCompMinutes.Any() ? MinutesToHhMm(tatCompMinutes.Average()) : null,
                };
            }
            catch { throw; }
        }

        // ══════════════════════════════════════════════════════════════════════
        // R5 — Batch Summary Excel Export (ClosedXML — same library as Contingency)
        // ══════════════════════════════════════════════════════════════════════

        public byte[] ExportBatchSummaryExcel(BatchSummaryReportRequest request)
        {
            try
            {
                var data = GetBatchSummary(request);

                using var wb = new XLWorkbook();
                var ws = wb.Worksheets.Add("Batch Summary");

                // ── Title block ───────────────────────────────────────────────
                ws.Cell("A1").Value = "BATCH SUMMARY REPORT";
                ws.Cell("A1").Style.Font.Bold = true;
                ws.Cell("A1").Style.Font.FontSize = 14;
                ws.Range("A1:K1").Merge();

                ws.Cell("A2").Value = $"Generated: {DateTime.Now:MM/dd/yyyy HH:mm}";
                ws.Range("A2:K2").Merge();

                ws.Cell("A3").Value = $"Location: {(string.IsNullOrEmpty(request.LocationCode) ? "ALL" : request.LocationCode)}";
                ws.Cell("C3").Value = $"Date: {request.DateFrom:MM/dd/yyyy} – {request.DateTo:MM/dd/yyyy}";

                // ── Summary strip ─────────────────────────────────────────────
                ws.Cell("A5").Value = "Total Batches"; ws.Cell("A5").Style.Font.Bold = true;
                ws.Cell("B5").Value = data.TotalBatches;
                ws.Cell("C5").Value = "Completed"; ws.Cell("C5").Style.Font.Bold = true;
                ws.Cell("D5").Value = data.CompletedBatches;
                ws.Cell("E5").Value = "Pending / Partial"; ws.Cell("E5").Style.Font.Bold = true;
                ws.Cell("F5").Value = data.PendingBatches;
                ws.Cell("G5").Value = "Avg TAT (Endorsement)"; ws.Cell("G5").Style.Font.Bold = true;
                ws.Cell("H5").Value = data.AvgTatEndorsement ?? "—";
                ws.Cell("I5").Value = "Avg TAT (Completion)"; ws.Cell("I5").Style.Font.Bold = true;
                ws.Cell("J5").Value = data.AvgTatCompletion ?? "—";

                // ── Column headers ────────────────────────────────────────────
                int headerRow = 7;
                var headers = new[]
                {
                    "Batch No.", "Location",
                    "Batch Endorsed (Date & Time)", "Endorsed By",
                    "Batch Received (Date & Time)", "Received By",
                    "Batch Completed", "Temp", "Status",
                    "TAT (Batch Endorsement)", "TAT (Batch Completion)"
                };

                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = ws.Cell(headerRow, i + 1);
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#4472C4");
                    cell.Style.Font.FontColor = XLColor.White;
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }

                // ── Data rows ─────────────────────────────────────────────────
                int dataRow = headerRow + 1;
                foreach (var r in data.Rows)
                {
                    ws.Cell(dataRow, 1).Value = r.BatchNo;
                    ws.Cell(dataRow, 2).Value = r.Location;
                    ws.Cell(dataRow, 3).Value = r.Endorsed.ToString("MM/dd/yyyy HH:mm");
                    ws.Cell(dataRow, 4).Value = r.EndorsedBy;
                    ws.Cell(dataRow, 5).Value = r.ProcReceived.HasValue ? r.ProcReceived.Value.ToString("MM/dd/yyyy HH:mm") : "—";
                    ws.Cell(dataRow, 6).Value = r.ReceivedBy ?? "—";
                    ws.Cell(dataRow, 7).Value = r.Completed.HasValue ? r.Completed.Value.ToString("MM/dd/yyyy HH:mm") : "—";
                    ws.Cell(dataRow, 8).Value = r.Temp ?? "—";
                    ws.Cell(dataRow, 9).Value = r.Status;
                    ws.Cell(dataRow, 10).Value = r.TatEndorsement ?? "—";
                    ws.Cell(dataRow, 11).Value = r.TatCompletion ?? "—";

                    if (dataRow % 2 == 0)
                        ws.Row(dataRow).Style.Fill.BackgroundColor = XLColor.FromHtml("#F2F2F2");

                    ws.Range(dataRow, 1, dataRow, 11).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    dataRow++;
                }

                // ── Average footer ────────────────────────────────────────────
                ws.Cell(dataRow, 1).Value = "AVERAGE TAT";
                ws.Cell(dataRow, 10).Value = data.AvgTatEndorsement ?? "—";
                ws.Cell(dataRow, 11).Value = data.AvgTatCompletion ?? "—";
                ws.Range(dataRow, 1, dataRow, 11).Style.Font.Bold = true;
                ws.Range(dataRow, 1, dataRow, 11).Style.Fill.BackgroundColor = XLColor.FromHtml("#D9E1F2");
                ws.Range(dataRow, 1, dataRow, 11).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                // ── Column widths + freeze ────────────────────────────────────
                ws.Column(1).Width = 22;
                ws.Column(2).Width = 18;
                ws.Column(3).Width = 24;
                ws.Column(4).Width = 16;
                ws.Column(5).Width = 24;
                ws.Column(6).Width = 16;
                ws.Column(7).Width = 24;
                ws.Column(8).Width = 8;
                ws.Column(9).Width = 12;
                ws.Column(10).Width = 24;
                ws.Column(11).Width = 24;
                ws.SheetView.FreezeRows(headerRow);

                using var ms = new MemoryStream();
                wb.SaveAs(ms);
                return ms.ToArray();
            }
            catch { throw; }
        }

        // ══════════════════════════════════════════════════════════════════════
        // R1 — Unprocessed Specimen (TODO)
        // ══════════════════════════════════════════════════════════════════════

        public List<UnprocessedSpecimenRow> GetUnprocessedSpecimens(UnprocessedSpecimenRequest request)
        {
            // TODO: Query Specimen_Section_Test where ScheduleTag IN ('ERD','CRD','SRD')
            //       and Status IN ('P','S') within the date range.
            //       Join to Section_Master for section name.
            throw new NotImplementedException("Unprocessed Specimen report is not yet implemented.");
        }

        // ══════════════════════════════════════════════════════════════════════
        // R2 — Specimen Not Endorsed (TODO — pending OIC clarification)
        // ══════════════════════════════════════════════════════════════════════

        public List<SpecimenNotEndorsedRow> GetSpecimensNotEndorsed(SpecimenNotEndorsedRequest request)
        {
            // TODO: Pending OIC clarification on event code and trigger condition.
            //       Expected: Query Audit_Log where EventCode = AuditEvents.NotEndorsed
            //       within date range, optionally filtered by FromLocation.
            throw new NotImplementedException("Specimen Not Endorsed report is pending clarification with OIC.");
        }

        // ══════════════════════════════════════════════════════════════════════
        // R3 — Specimen Not Received / Pending (TODO)
        // ══════════════════════════════════════════════════════════════════════

        public List<SpecimenNotReceivedRow> GetSpecimensNotReceived(SpecimenNotReceivedRequest request)
        {
            // TODO: Query Batch_Specimen where Status = 'P' (not received),
            //       join Batch_Header for date range and location filter.
            //       Include LabNo, PatientName, SampleTypeName.
            throw new NotImplementedException("Specimen Not Received report is not yet implemented.");
        }

        // ══════════════════════════════════════════════════════════════════════
        // R4 — Test Management (TODO)
        // ══════════════════════════════════════════════════════════════════════

        public List<TestManagementRow> GetTestManagement(TestManagementRequest request)
        {
            // TODO: Query Section_TestGroup joined to test master data.
            //       Include running days, TAT, and Active/Inactive status.
            throw new NotImplementedException("Test Management report is not yet implemented.");
        }

        // ══════════════════════════════════════════════════════════════════════
        // R6 — Specimen Receipt (Laboratory Section)
        // ══════════════════════════════════════════════════════════════════════

        public List<SpecimenReceiptSectionRow> GetSpecimenReceiptSection(SpecimenReceiptSectionRequest request)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);

                // ── 1. Section name lookups ───────────────────────────────────
                var allSections = context.Section_Master
                    .ToList()
                    .ToDictionary(s => s.Code, s => s.Name);

                // ── 2. Specimen_Section_Headers with a section received timestamp
                //       filtered by ProcReceived date range via Batch_Specimen_Receiving ──
                var sectionHeaders = context.Specimen_Section_Header
                    .Where(h => h.Received != null)
                    .ToList();

                // Optional lab section filter
                if (!string.IsNullOrEmpty(request.SectionCode))
                    sectionHeaders = sectionHeaders
                        .Where(h => h.SectionCode == request.SectionCode)
                        .ToList();

                if (!sectionHeaders.Any())
                    return new List<SpecimenReceiptSectionRow>();

                var specimenNos = sectionHeaders.Select(h => h.SpecimenNo).Distinct().ToList();

                // ── 3. Batch_Specimen_Receiving — for ProcReceived + RoutedBy ─
                //       RoutedBy in the receiving record = ProcReceivedBy (the processor
                //       who received and routed the specimen to the lab section)
                var receivingRecords = context.Batch_Specimen_Receiving
                    .ToList()
                    .Where(r => specimenNos.Contains(r.SpecimenNo))
                    .GroupBy(r => r.SpecimenNo)
                    .ToDictionary(
                        g => g.Key,
                        g => g.OrderByDescending(r => r.ProcReceived).First()
                    );

                // ── 4. Apply date range filter on ProcReceived ────────────────
                var filteredSpecimenNos = receivingRecords
                    .Where(kv =>
                        kv.Value.ProcReceived.Date >= request.DateFrom.Date &&
                        kv.Value.ProcReceived.Date <= request.DateTo.Date)
                    .Select(kv => kv.Key)
                    .ToHashSet();

                sectionHeaders = sectionHeaders
                    .Where(h => filteredSpecimenNos.Contains(h.SpecimenNo))
                    .ToList();

                if (!sectionHeaders.Any())
                    return new List<SpecimenReceiptSectionRow>();

                // ── 5. Batch_Specimen — for BatchNo, Location, PatientName, SampleType ──
                var batchSpecimens = context.Batch_Specimen
                    .ToList()
                    .Where(s => filteredSpecimenNos.Contains(s.SpecimenNo))
                    .GroupBy(s => s.SpecimenNo)
                    .ToDictionary(
                        g => g.Key,
                        g => g.OrderByDescending(s => s.Id).First()
                    );

                // ── 6. Batch_Header — for Location (endorser section code) ───
                var batchNos = batchSpecimens.Values
                    .Select(s => s.BatchNo)
                    .Distinct()
                    .ToList();

                var batchHeaders = context.Batch_Header
                    .ToList()
                    .Where(h => batchNos.Contains(h.BatchNo))
                    .ToDictionary(h => h.BatchNo);

                // ── 7. Optional endorser location filter ─────────────────────
                if (!string.IsNullOrEmpty(request.LocationCode))
                {
                    var allowedSpecimenNos = batchSpecimens
                        .Where(kv =>
                        {
                            batchHeaders.TryGetValue(kv.Value.BatchNo, out var bh);
                            return bh?.Location == request.LocationCode;
                        })
                        .Select(kv => kv.Key)
                        .ToHashSet();

                    sectionHeaders = sectionHeaders
                        .Where(h => allowedSpecimenNos.Contains(h.SpecimenNo))
                        .ToList();
                }

                // ── 8. Build rows ─────────────────────────────────────────────
                var rows = sectionHeaders
                    .OrderBy(h => h.SectionCode)
                    .ThenBy(h => h.Received)
                    .Select(h =>
                    {
                        batchSpecimens.TryGetValue(h.SpecimenNo, out var bs);
                        receivingRecords.TryGetValue(h.SpecimenNo, out var rcv);
                        batchHeaders.TryGetValue(bs?.BatchNo ?? "", out var bh);

                        var locationCode = bh?.Location ?? "";

                        return new SpecimenReceiptSectionRow
                        {
                            BatchNo = bs?.BatchNo,
                            Location = allSections.TryGetValue(locationCode, out var ln) ? ln : locationCode,
                            SpecimenNo = h.SpecimenNo,
                            PatientName = bs?.PatientName,
                            SampleTypeName = bs?.SampleTypeName,
                            ProcReceived = rcv?.ProcReceived,
                            RoutedBy = rcv?.ProcReceivedBy,
                            SectionReceived = h.Received,
                            SectionReceivedBy = h.ReceivedBy,
                            LabSection = allSections.TryGetValue(h.SectionCode, out var sn) ? sn : h.SectionCode,
                            TatSection = FormatTat(rcv?.ProcReceived, h.Received),
                        };
                    })
                    .ToList();

                return rows;
            }
            catch { throw; }
        }

        public byte[] ExportSpecimenReceiptSectionExcel(SpecimenReceiptSectionRequest request)
        {
            try
            {
                var data = GetSpecimenReceiptSection(request);

                using var wb = new XLWorkbook();
                var ws = wb.Worksheets.Add("Specimen Receipt - Lab Section");

                // ── Title block ───────────────────────────────────────────────
                ws.Cell("A1").Value = "SPECIMEN RECEIPT (LABORATORY SECTION) REPORT";
                ws.Cell("A1").Style.Font.Bold = true;
                ws.Cell("A1").Style.Font.FontSize = 14;
                ws.Range("A1:K1").Merge();

                ws.Cell("A2").Value = $"Generated: {DateTime.Now:MM/dd/yyyy HH:mm}";
                ws.Range("A2:K2").Merge();

                ws.Cell("A3").Value = $"Lab Section: {(string.IsNullOrEmpty(request.SectionCode) ? "ALL" : request.SectionCode)}";
                ws.Cell("D3").Value = $"Location: {(string.IsNullOrEmpty(request.LocationCode) ? "ALL" : request.LocationCode)}";
                ws.Cell("G3").Value = $"Date: {request.DateFrom:MM/dd/yyyy} – {request.DateTo:MM/dd/yyyy}";

                // ── Column headers ────────────────────────────────────────────
                int headerRow = 5;
                var headers = new[]
                {
                    "Batch No.", "Location", "Specimen No.", "Patient Name", "Sample Type",
                    "Processing Received", "Routed By", "Section Received", "Received By (Section)",
                    "Lab Section", "TAT (Section)"
                };

                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = ws.Cell(headerRow, i + 1);
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#4472C4");
                    cell.Style.Font.FontColor = XLColor.White;
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }

                // ── Data rows ─────────────────────────────────────────────────
                int dataRow = headerRow + 1;
                foreach (var r in data)
                {
                    ws.Cell(dataRow, 1).Value = r.BatchNo ?? "";
                    ws.Cell(dataRow, 2).Value = r.Location ?? "";
                    ws.Cell(dataRow, 3).Value = r.SpecimenNo ?? "";
                    ws.Cell(dataRow, 4).Value = r.PatientName ?? "";
                    ws.Cell(dataRow, 5).Value = r.SampleTypeName ?? "";
                    ws.Cell(dataRow, 6).Value = r.ProcReceived.HasValue ? r.ProcReceived.Value.ToString("MM/dd/yyyy HH:mm") : "—";
                    ws.Cell(dataRow, 7).Value = r.RoutedBy ?? "—";
                    ws.Cell(dataRow, 8).Value = r.SectionReceived.HasValue ? r.SectionReceived.Value.ToString("MM/dd/yyyy HH:mm") : "—";
                    ws.Cell(dataRow, 9).Value = r.SectionReceivedBy ?? "—";
                    ws.Cell(dataRow, 10).Value = r.LabSection ?? "";
                    ws.Cell(dataRow, 11).Value = r.TatSection ?? "—";

                    if (dataRow % 2 == 0)
                        ws.Row(dataRow).Style.Fill.BackgroundColor = XLColor.FromHtml("#F2F2F2");

                    ws.Range(dataRow, 1, dataRow, 11).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    dataRow++;
                }

                // ── Column widths ─────────────────────────────────────────────
                ws.Column(1).Width = 18;
                ws.Column(2).Width = 20;
                ws.Column(3).Width = 18;
                ws.Column(4).Width = 22;
                ws.Column(5).Width = 16;
                ws.Column(6).Width = 22;
                ws.Column(7).Width = 14;
                ws.Column(8).Width = 22;
                ws.Column(9).Width = 20;
                ws.Column(10).Width = 20;
                ws.Column(11).Width = 14;
                ws.SheetView.FreezeRows(headerRow);

                using var ms = new MemoryStream();
                wb.SaveAs(ms);
                return ms.ToArray();
            }
            catch { throw; }
        }

        // ══════════════════════════════════════════════════════════════════════
        // R7 — Duplicate Endorsement (Re-endorsed specimens)
        // ══════════════════════════════════════════════════════════════════════

        public List<DuplicateEndorsementRow> GetDuplicateEndorsements(DuplicateEndorsementRequest request)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);

                // ── 1. Section name lookup ────────────────────────────────────
                var sections = context.Section_Master
                    .ToList()
                    .ToDictionary(s => s.Code, s => s.Name);

                // ── 2. All ENDORSED_DUPLICATE entries in date range ───────────
                var duplicateLogs = unit.AuditLogs.GetAll()
                    .Where(l => l.EventCode == AuditEvents.EndorsedDuplicate
                             && l.LoggedAt.Date >= request.DateFrom.Date
                             && l.LoggedAt.Date <= request.DateTo.Date)
                    .ToList();

                // Optional location filter
                if (!string.IsNullOrEmpty(request.LocationCode))
                    duplicateLogs = duplicateLogs
                        .Where(l => l.FromLocation == request.LocationCode)
                        .ToList();

                if (!duplicateLogs.Any())
                    return new List<DuplicateEndorsementRow>();

                var specimenNos = duplicateLogs.Select(l => l.SpecimenNo).Distinct().ToList();

                // ── 3. All prior endorsement logs for those specimens ─────────
                //       Pull ENDORSED + ENDORSED_DUPLICATE so we can pair them
                var allEndorseLogs = unit.AuditLogs.GetAll()
                    .Where(l => l.EventCode == AuditEvents.Endorsed ||
                                l.EventCode == AuditEvents.EndorsedDuplicate)
                    .Where(l => l.SpecimenNo != null)
                    .ToList()
                    .Where(l => specimenNos.Contains(l.SpecimenNo))
                    .OrderBy(l => l.SpecimenNo)
                    .ThenBy(l => l.LoggedAt)
                    .ToList();

                // ── 4. Build rows — one per ENDORSED_DUPLICATE entry ──────────
                var rows = duplicateLogs
                    .OrderBy(l => l.LoggedAt)
                    .Select(dup =>
                    {
                        // Most recent prior endorsement log for this specimen
                        var prior = allEndorseLogs
                            .Where(l => l.SpecimenNo == dup.SpecimenNo
                                     && l.LoggedAt < dup.LoggedAt)
                            .OrderByDescending(l => l.LoggedAt)
                            .FirstOrDefault();

                        var locationCode = dup.FromLocation ?? "";

                        return new DuplicateEndorsementRow
                        {
                            BatchNo = dup.BatchNo,
                            Location = sections.TryGetValue(locationCode, out var ln) ? ln : locationCode,
                            SpecimenNo = dup.SpecimenNo,
                            PatientName = dup.PatientName,
                            FirstEndorsedAt = prior?.LoggedAt,
                            FirstEndorsedBy = prior?.UserID,
                            SecondEndorsedAt = dup.LoggedAt,
                            SecondEndorsedBy = dup.UserID,
                            Reason = dup.Remarks,
                        };
                    })
                    .ToList();

                return rows;
            }
            catch { throw; }
        }

        public byte[] ExportDuplicateEndorsementsExcel(DuplicateEndorsementRequest request)
        {
            try
            {
                var data = GetDuplicateEndorsements(request);

                using var wb = new XLWorkbook();
                var ws = wb.Worksheets.Add("Duplicate Endorsement");

                // ── Title block ───────────────────────────────────────────────
                ws.Cell("A1").Value = "DUPLICATE ENDORSEMENT REPORT";
                ws.Cell("A1").Style.Font.Bold = true;
                ws.Cell("A1").Style.Font.FontSize = 14;
                ws.Range("A1:I1").Merge();

                ws.Cell("A2").Value = $"Generated: {DateTime.Now:MM/dd/yyyy HH:mm}";
                ws.Range("A2:I2").Merge();

                ws.Cell("A3").Value = $"Location: {(string.IsNullOrEmpty(request.LocationCode) ? "ALL" : request.LocationCode)}";
                ws.Cell("D3").Value = $"Date: {request.DateFrom:MM/dd/yyyy} – {request.DateTo:MM/dd/yyyy}";

                // ── Column headers ────────────────────────────────────────────
                int headerRow = 5;
                var headers = new[]
                {
                    "Batch No.", "Location", "Specimen No.", "Patient Name",
                    "1st Endorsed", "By (1st)",
                    "2nd Endorsed", "By (2nd)",
                    "Reason"
                };

                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = ws.Cell(headerRow, i + 1);
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#4472C4");
                    cell.Style.Font.FontColor = XLColor.White;
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }

                // ── Data rows ─────────────────────────────────────────────────
                int dataRow = headerRow + 1;
                foreach (var r in data)
                {
                    ws.Cell(dataRow, 1).Value = r.BatchNo ?? "";
                    ws.Cell(dataRow, 2).Value = r.Location ?? "";
                    ws.Cell(dataRow, 3).Value = r.SpecimenNo ?? "";
                    ws.Cell(dataRow, 4).Value = r.PatientName ?? "";
                    ws.Cell(dataRow, 5).Value = r.FirstEndorsedAt.HasValue ? r.FirstEndorsedAt.Value.ToString("MM/dd/yyyy HH:mm") : "—";
                    ws.Cell(dataRow, 6).Value = r.FirstEndorsedBy ?? "—";
                    ws.Cell(dataRow, 7).Value = r.SecondEndorsedAt.HasValue ? r.SecondEndorsedAt.Value.ToString("MM/dd/yyyy HH:mm") : "—";
                    ws.Cell(dataRow, 8).Value = r.SecondEndorsedBy ?? "—";
                    ws.Cell(dataRow, 9).Value = r.Reason ?? "—";

                    if (dataRow % 2 == 0)
                        ws.Row(dataRow).Style.Fill.BackgroundColor = XLColor.FromHtml("#F2F2F2");

                    ws.Range(dataRow, 1, dataRow, 9).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    dataRow++;
                }

                // ── Column widths ─────────────────────────────────────────────
                ws.Column(1).Width = 18;
                ws.Column(2).Width = 20;
                ws.Column(3).Width = 18;
                ws.Column(4).Width = 22;
                ws.Column(5).Width = 20;
                ws.Column(6).Width = 14;
                ws.Column(7).Width = 20;
                ws.Column(8).Width = 14;
                ws.Column(9).Width = 30;
                ws.SheetView.FreezeRows(headerRow);

                using var ms = new MemoryStream();
                wb.SaveAs(ms);
                return ms.ToArray();
            }
            catch { throw; }
        }

        // ══════════════════════════════════════════════════════════════════════
        // R8 — Transaction Beyond 14 Days
        // ══════════════════════════════════════════════════════════════════════

        public List<Beyond14DaysRow> GetBeyond14Days(Beyond14DaysRequest request)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);

                // ── 1. Section name lookup ────────────────────────────────────
                var sections = context.Section_Master
                    .ToList()
                    .ToDictionary(s => s.Code, s => s.Name);

                // ── 2. All ENDORSED_14DAYS entries in date range ──────────────
                var logs = unit.AuditLogs.GetAll()
                    .Where(l => l.EventCode == AuditEvents.Endorsed14Days
                             && l.LoggedAt.Date >= request.DateFrom.Date
                             && l.LoggedAt.Date <= request.DateTo.Date)
                    .ToList();

                // Optional location filter
                if (!string.IsNullOrEmpty(request.LocationCode))
                    logs = logs
                        .Where(l => l.FromLocation == request.LocationCode)
                        .ToList();

                if (!logs.Any())
                    return new List<Beyond14DaysRow>();

                // ── 3. Build rows ─────────────────────────────────────────────
                var rows = logs
                    .OrderBy(l => l.LoggedAt)
                    .Select(l =>
                    {
                        var locationCode = l.FromLocation ?? "";
                        return new Beyond14DaysRow
                        {
                            BatchNo = l.BatchNo,
                            Location = sections.TryGetValue(locationCode, out var ln) ? ln : locationCode,
                            SpecimenNo = l.SpecimenNo,
                            PatientName = l.PatientName,
                            EndorsedAt = l.LoggedAt,
                            EndorsedBy = l.UserID,
                            Reason = l.Remarks,
                        };
                    })
                    .ToList();

                return rows;
            }
            catch { throw; }
        }

        public byte[] ExportBeyond14DaysExcel(Beyond14DaysRequest request)
        {
            try
            {
                var data = GetBeyond14Days(request);

                using var wb = new XLWorkbook();
                var ws = wb.Worksheets.Add("Beyond 14 Days");

                // ── Title block ───────────────────────────────────────────────
                ws.Cell("A1").Value = "TRANSACTION BEYOND 14 DAYS REPORT";
                ws.Cell("A1").Style.Font.Bold = true;
                ws.Cell("A1").Style.Font.FontSize = 14;
                ws.Range("A1:G1").Merge();

                ws.Cell("A2").Value = $"Generated: {DateTime.Now:MM/dd/yyyy HH:mm}";
                ws.Range("A2:G2").Merge();

                ws.Cell("A3").Value = $"Location: {(string.IsNullOrEmpty(request.LocationCode) ? "ALL" : request.LocationCode)}";
                ws.Cell("D3").Value = $"Date: {request.DateFrom:MM/dd/yyyy} – {request.DateTo:MM/dd/yyyy}";

                // ── Column headers ────────────────────────────────────────────
                int headerRow = 5;
                var headers = new[]
                {
                    "Batch No.", "Location", "Specimen No.",
                    "Patient Name", "Endorsed", "Endorsed By", "Reason"
                };

                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = ws.Cell(headerRow, i + 1);
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#4472C4");
                    cell.Style.Font.FontColor = XLColor.White;
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }

                // ── Data rows ─────────────────────────────────────────────────
                int dataRow = headerRow + 1;
                foreach (var r in data)
                {
                    ws.Cell(dataRow, 1).Value = r.BatchNo ?? "";
                    ws.Cell(dataRow, 2).Value = r.Location ?? "";
                    ws.Cell(dataRow, 3).Value = r.SpecimenNo ?? "";
                    ws.Cell(dataRow, 4).Value = r.PatientName ?? "";
                    ws.Cell(dataRow, 5).Value = r.EndorsedAt.HasValue ? r.EndorsedAt.Value.ToString("MM/dd/yyyy HH:mm") : "—";
                    ws.Cell(dataRow, 6).Value = r.EndorsedBy ?? "—";
                    ws.Cell(dataRow, 7).Value = r.Reason ?? "—";

                    if (dataRow % 2 == 0)
                        ws.Row(dataRow).Style.Fill.BackgroundColor = XLColor.FromHtml("#F2F2F2");

                    ws.Range(dataRow, 1, dataRow, 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    dataRow++;
                }

                // ── Column widths ─────────────────────────────────────────────
                ws.Column(1).Width = 18;
                ws.Column(2).Width = 20;
                ws.Column(3).Width = 18;
                ws.Column(4).Width = 22;
                ws.Column(5).Width = 20;
                ws.Column(6).Width = 14;
                ws.Column(7).Width = 32;
                ws.SheetView.FreezeRows(headerRow);

                using var ms = new MemoryStream();
                wb.SaveAs(ms);
                return ms.ToArray();
            }
            catch { throw; }
        }

        // ══════════════════════════════════════════════════════════════════════
        // R9 — Monthly Endorsement Summary (TODO)
        // ══════════════════════════════════════════════════════════════════════

        public List<MonthlyEndorsementSummaryRow> GetMonthlyEndorsementSummary(MonthlyEndorsementSummaryRequest request)
        {
            // TODO: Aggregate per location over the date range:
            //   TotalBatches          = COUNT(Batch_Header) grouped by Location
            //   BatchesOutsideTat     = COUNT where IsOutsideTat = true
            //   BatchesWithinTat      = TotalBatches - BatchesOutsideTat
            //   DuplicateEndorsements = COUNT(Audit_Log where EventCode = EndorsedDuplicate)
            //   NotEndorsed           = COUNT(Audit_Log where EventCode = NotEndorsed)
            //   Beyond14Days          = COUNT(Audit_Log where EventCode = Endorsed14Days)
            throw new NotImplementedException("Monthly Endorsement Summary report is not yet implemented.");
        }
    }
}