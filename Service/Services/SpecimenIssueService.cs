using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HCLAB;
using Model.Main;
using Model.SETSDB;
using Reposi;
using Reposi.Context;
using Service.Interfaces;

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
                        LoggedAt = e.LoggedAt
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
    }
}