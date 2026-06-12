using ClosedXML.Excel;
using HCLAB;
using Model.Main;
using Model.SETSDB;
using Reposi;
using Reposi.Context;
using Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services
{
    public class SpecimenIssueService : ISpecimenIssueService
    {
        private readonly AppDbContextFactory _factory;
        private readonly string _branch;
        private readonly string _branch_raw;

        public SpecimenIssueService(AppDbContextFactory factory, string branch)
        {
            _factory = factory;
            _branch = SetsConnection.ConnectionString(branch);
            _branch_raw = branch;
        }

        // ── Incident Types ─────────────────────────────────────────────────────

        public List<IncidentTypeListItem> GetIncidentTypes()
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);

                var types = unit.IssueIncidentTypes.GetAll();
                var subCats = unit.IssueSubCategories.Query().ToList();

                return types.Select(t => new IncidentTypeListItem
                {
                    Id = t.Id,
                    Name = t.Name,
                    IsActive = t.IsActive,
                    CreatedBy = t.CreatedBy,
                    CreatedAt = t.CreatedAt,
                    SubCategoryCount = subCats.Count(s => s.IncidentTypeId == t.Id && s.IsActive)
                }).ToList();
            }
            catch { throw; }
        }

        public void CreateIncidentType(CreateIncidentTypeRequest request)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);

                var exists = unit.IssueIncidentTypes.Query()
                    .Any(i => i.Name.ToLower() == request.Name.ToLower());

                if (exists)
                    throw new Exception($"Incident type '{request.Name}' already exists.");

                unit.IssueIncidentTypes.Add(new Issue_IncidentType
                {
                    Name = request.Name,
                    IsActive = true,
                    CreatedBy = request.UserID,
                    CreatedAt = DateTime.Now
                });
            }
            catch { throw; }
        }

        public void ToggleIncidentType(int id, ToggleActiveRequest request)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);

                var record = unit.IssueIncidentTypes.GetById(id)
                    ?? throw new Exception("Incident type not found.");

                record.IsActive = !record.IsActive;
                unit.IssueIncidentTypes.Update(record);
            }
            catch { throw; }
        }

        // ── Sub-Categories ─────────────────────────────────────────────────────

        public List<SubCategoryListItem> GetSubCategories(int incidentTypeId)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);

                var subCats = unit.IssueSubCategories.GetByIncidentTypeId(incidentTypeId);
                var allTags = unit.IssueTags.Query().ToList();
                var allEntries = unit.IssueLabEntries.Query().ToList();

                return subCats.Select(s => new SubCategoryListItem
                {
                    Id = s.Id,
                    IncidentTypeId = s.IncidentTypeId,
                    Name = s.Name,
                    IsActive = s.IsActive,
                    CreatedBy = s.CreatedBy,
                    CreatedAt = s.CreatedAt,
                    EntryCount = allEntries.Count(e => e.SubCategoryId == s.Id),
                    Tags = allTags
                        .Where(t => t.SubCategoryId == s.Id)
                        .Select(t => new TagItem { Id = t.Id, TagName = t.TagName })
                        .ToList()
                }).ToList();
            }
            catch { throw; }
        }

        public void CreateSubCategory(CreateSubCategoryRequest request)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);

                var exists = unit.IssueSubCategories.Query()
                    .Any(s => s.IncidentTypeId == request.IncidentTypeId
                           && s.Name.ToLower() == request.Name.ToLower());

                if (exists)
                    throw new Exception($"Sub-category '{request.Name}' already exists in this folder.");

                unit.IssueSubCategories.Add(new Issue_SubCategory
                {
                    IncidentTypeId = request.IncidentTypeId,
                    Name = request.Name,
                    IsActive = true,
                    CreatedBy = request.UserID,
                    CreatedAt = DateTime.Now
                });
            }
            catch { throw; }
        }

        public void ToggleSubCategory(int id, ToggleActiveRequest request)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);

                var record = unit.IssueSubCategories.GetById(id)
                    ?? throw new Exception("Sub-category not found.");

                record.IsActive = !record.IsActive;
                unit.IssueSubCategories.Update(record);
            }
            catch { throw; }
        }

        // ── Tags ───────────────────────────────────────────────────────────────

        public void AddTag(AddTagRequest request)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);

                if (unit.IssueTags.Exists(request.SubCategoryId, request.TagName))
                    throw new Exception($"Tag '{request.TagName}' already exists on this sub-category.");

                unit.IssueTags.Add(new Issue_Tag
                {
                    SubCategoryId = request.SubCategoryId,
                    TagName = request.TagName,
                    CreatedBy = request.UserID,
                    CreatedAt = DateTime.Now
                });
            }
            catch { throw; }
        }

        public void DeleteTag(int id)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                unit.IssueTags.Delete(id);
            }
            catch { throw; }
        }

        public List<string> GetAllTagNames()
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                return unit.IssueTags.GetAllTagNames();
            }
            catch { throw; }
        }

        // ── Lab Entries ────────────────────────────────────────────────────────

        public List<LabEntryItem> GetLabEntries(int subCategoryId)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);

                return unit.IssueLabEntries
                    .GetBySubCategoryId(subCategoryId)
                    .Select(e => new LabEntryItem
                    {
                        Id = e.Id,
                        SpecimenNo = e.SpecimenNo,
                        PID = e.PID,
                        PatientName = e.PatientName,
                        SampleTypeCode = e.SampleTypeCode,
                        SampleTypeName = e.SampleTypeName,
                        EntryDate = e.EntryDate,
                        LoggedBy = e.LoggedBy,
                        LoggedAt = e.LoggedAt,
                        Remarks = e.Remarks
                    }).ToList();
            }
            catch { throw; }
        }

        public async Task AddLabEntry(AddLabEntryRequest request)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);

                var oracleConn = HclabConnection.ConnectionString(_branch_raw);

                string? pid = null;
                string? patientName = null;
                string? sampleTypeCode = null;
                string? sampleTypeName = null;

                try
                {
                    var batchSpec = context.Batch_Specimen
                        .FirstOrDefault(b => b.SpecimenNo == request.SpecimenNo);

                    if (batchSpec != null)
                    {
                        pid = batchSpec.PID;
                        patientName = batchSpec.PatientName;
                        sampleTypeCode = batchSpec.SampleTypeCode;
                        sampleTypeName = batchSpec.SampleTypeName;
                    }
                    else if (request.SpecimenNo.Length >= 10)
                    {
                        var labNo = request.SpecimenNo.Substring(0, 10);
                        var fallback = context.Batch_Specimen
                            .FirstOrDefault(b => b.LabNo == labNo);

                        if (fallback != null)
                        {
                            pid = fallback.PID;
                            patientName = fallback.PatientName;
                        }
                    }
                }
                catch { /* non-fatal — log entry still proceeds */ }

                unit.IssueLabEntries.Add(new Issue_LabEntry
                {
                    SubCategoryId = request.SubCategoryId,
                    SpecimenNo = request.SpecimenNo,
                    PID = pid,
                    PatientName = patientName,
                    SampleTypeCode = sampleTypeCode,
                    SampleTypeName = sampleTypeName,
                    EntryDate = DateOnly.FromDateTime(DateTime.Today),
                    LoggedBy = request.UserID,
                    LoggedAt = DateTime.Now
                });
            }
            catch { throw; }
        }

        public void UpdateLabEntryRemark(int id, UpdateLabEntryRemarkRequest request)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);

                var record = unit.IssueLabEntries.Get(id)
                    ?? throw new Exception("Lab entry not found.");

                record.Remarks = string.IsNullOrWhiteSpace(request.Remarks)
                    ? null
                    : request.Remarks.Trim();

                unit.IssueLabEntries.Update(record);
            }
            catch { throw; }
        }

        public void DeleteLabEntry(int id)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);

                var record = unit.IssueLabEntries.Get(id)
                    ?? throw new Exception("Lab entry not found.");

                unit.IssueLabEntries.Delete(id);
            }
            catch { throw; }
        }

        // ── Comments ───────────────────────────────────────────────────────────

        public List<CommentItem> GetComments(int incidentTypeId)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);

                return unit.IssueComments
                    .GetByIncidentTypeId(incidentTypeId)
                    .Select(c => new CommentItem
                    {
                        Id = c.Id,
                        CommentText = c.CommentText,
                        CreatedBy = c.CreatedBy,
                        CreatedAt = c.CreatedAt,
                        RevisedBy = c.RevisedBy,
                        RevisedAt = c.RevisedAt
                    }).ToList();
            }
            catch { throw; }
        }

        public void AddComment(AddCommentRequest request)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);

                unit.IssueComments.Add(new Issue_Comment
                {
                    IncidentTypeId = request.IncidentTypeId,
                    CommentText = request.CommentText,
                    CreatedBy = request.UserID,
                    CreatedAt = DateTime.Now
                });
            }
            catch { throw; }
        }

        public void EditComment(int id, EditCommentRequest request)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);

                var record = unit.IssueComments.GetById(id)
                    ?? throw new Exception("Comment not found.");

                record.CommentText = request.CommentText;
                record.RevisedBy = request.UserID;
                record.RevisedAt = DateTime.Now;
                unit.IssueComments.Update(record);
            }
            catch { throw; }
        }

        public byte[] ExportIncidentTypeExcel(int incidentTypeId)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);

                // ── 1. Fetch incident type name ───────────────────────────────
                var incidentType = unit.IssueIncidentTypes.GetById(incidentTypeId)
                    ?? throw new Exception("Incident type not found.");

                // ── 2. Fetch all sub-categories for this incident type ─────────
                var subCats = unit.IssueSubCategories
                    .GetByIncidentTypeId(incidentTypeId)
                    .OrderBy(s => s.Name)
                    .ToList();

                // ── 3. Fetch all lab entries for each sub-category ────────────
                var subCatIds = subCats.Select(s => s.Id).ToHashSet();
                var allEntries = unit.IssueLabEntries
                    .Query()
                    .Where(e => subCatIds.Contains(e.SubCategoryId))
                    .OrderBy(e => e.SubCategoryId)
                    .ThenBy(e => e.EntryDate)
                    .ThenBy(e => e.LoggedAt)
                    .ToList();

                var subCatNameMap = subCats.ToDictionary(s => s.Id, s => s.Name);

                // ── 4. Build workbook ─────────────────────────────────────────
                using var wb = new XLWorkbook();
                var ws = wb.Worksheets.Add("Issues Log");

                // Title block
                ws.Cell("A1").Value = $"SPECIMEN ISSUES LOG — {incidentType.Name.ToUpper()}";
                ws.Cell("A1").Style.Font.Bold = true;
                ws.Cell("A1").Style.Font.FontSize = 14;
                ws.Cell("A1").Style.Font.FontName = "Arial";
                ws.Range("A1:K1").Merge();

                ws.Cell("A2").Value = $"Generated: {DateTime.Now:MM/dd/yyyy HH:mm}";
                ws.Cell("A2").Style.Font.FontName = "Arial";
                ws.Cell("A2").Style.Font.FontSize = 10;
                ws.Range("A2:K2").Merge();

                ws.Cell("A3").Value = $"Total Entries: {allEntries.Count}";
                ws.Cell("A3").Style.Font.FontName = "Arial";
                ws.Cell("A3").Style.Font.FontSize = 10;
                ws.Range("A3:K3").Merge();

                // Column headers (row 5)
                int headerRow = 5;
                var headers = new[]
                {
                    "#", "Sub-Category", "Specimen No.", "Lab No.",
                    "PID", "Patient Name", "Sample Type",
                    "Entry Date", "Logged By", "Logged At", "Remarks"
                };

                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = ws.Cell(headerRow, i + 1);
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Font.FontName = "Arial";
                    cell.Style.Font.FontSize = 10;
                    cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#2E4057");
                    cell.Style.Font.FontColor = XLColor.White;
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    cell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    cell.Style.Border.BottomBorderColor = XLColor.FromHtml("#1a2a38");
                }

                // Data rows
                int dataRow = headerRow + 1;
                int rowNum = 1;
                string? lastSubCat = null;

                foreach (var e in allEntries)
                {
                    var subCatName = subCatNameMap.TryGetValue(e.SubCategoryId, out var n) ? n : "—";
                    var labNo = e.SpecimenNo?.Length >= 10 ? e.SpecimenNo.Substring(0, 10) : e.SpecimenNo;

                    // Alternate row shading, with a slightly different shade when sub-category changes
                    bool isNewSubCat = subCatName != lastSubCat;
                    var bgColor = rowNum % 2 == 0
                        ? XLColor.FromHtml("#F2F4F6")
                        : XLColor.White;

                    if (isNewSubCat && lastSubCat != null)
                    {
                        // Thin separator line above first row of a new sub-category group
                        for (int col = 1; col <= headers.Length; col++)
                            ws.Cell(dataRow, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    }

                    ws.Cell(dataRow, 1).Value = rowNum;
                    ws.Cell(dataRow, 2).Value = subCatName;
                    ws.Cell(dataRow, 3).Value = e.SpecimenNo ?? "—";
                    ws.Cell(dataRow, 4).Value = labNo ?? "—";
                    ws.Cell(dataRow, 5).Value = e.PID ?? "—";
                    ws.Cell(dataRow, 6).Value = e.PatientName ?? "—";
                    ws.Cell(dataRow, 7).Value = !string.IsNullOrEmpty(e.SampleTypeName)
                        ? e.SampleTypeName
                        : (e.SampleTypeCode ?? "—");
                    ws.Cell(dataRow, 8).Value = e.EntryDate.ToString("MM/dd/yyyy");
                    ws.Cell(dataRow, 9).Value = e.LoggedBy ?? "—";
                    ws.Cell(dataRow, 10).Value = e.LoggedAt.ToString("MM/dd/yyyy HH:mm");
                    ws.Cell(dataRow, 11).Value = e.Remarks ?? "";

                    // Apply styling to all cells in this row
                    for (int col = 1; col <= headers.Length; col++)
                    {
                        var cell = ws.Cell(dataRow, col);
                        cell.Style.Font.FontName = "Arial";
                        cell.Style.Font.FontSize = 10;
                        cell.Style.Fill.BackgroundColor = bgColor;
                        cell.Style.Border.BottomBorder = XLBorderStyleValues.Hair;
                        cell.Style.Border.BottomBorderColor = XLColor.FromHtml("#DDDDDD");
                    }

                    // Left-align text columns, center numeric/date columns
                    ws.Cell(dataRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;  // #
                    ws.Cell(dataRow, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;  // Entry Date
                    ws.Cell(dataRow, 10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center; // Logged At

                    lastSubCat = subCatName;
                    dataRow++;
                    rowNum++;
                }

                // Empty state row
                if (!allEntries.Any())
                {
                    ws.Cell(dataRow, 1).Value = "No entries found for this incident type.";
                    ws.Range(dataRow, 1, dataRow, headers.Length).Merge();
                    ws.Cell(dataRow, 1).Style.Font.FontName = "Arial";
                    ws.Cell(dataRow, 1).Style.Font.Italic = true;
                    ws.Cell(dataRow, 1).Style.Font.FontColor = XLColor.Gray;
                    ws.Cell(dataRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }

                // Outer border on data range
                if (allEntries.Any())
                {
                    var dataRange = ws.Range(headerRow, 1, dataRow - 1, headers.Length);
                    dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
                    dataRange.Style.Border.OutsideBorderColor = XLColor.FromHtml("#2E4057");
                }

                // Column widths
                ws.Column(1).Width = 6;   // #
                ws.Column(2).Width = 24;  // Sub-Category
                ws.Column(3).Width = 20;  // Specimen No.
                ws.Column(4).Width = 14;  // Lab No.
                ws.Column(5).Width = 14;  // PID
                ws.Column(6).Width = 28;  // Patient Name
                ws.Column(7).Width = 18;  // Sample Type
                ws.Column(8).Width = 14;  // Entry Date
                ws.Column(9).Width = 16;  // Logged By
                ws.Column(10).Width = 18; // Logged At
                ws.Column(11).Width = 30; // Remarks

                // Freeze header row
                ws.SheetView.FreezeRows(headerRow);

                using var ms = new MemoryStream();
                wb.SaveAs(ms);
                return ms.ToArray();
            }
            catch { throw; }
        }
    }
}