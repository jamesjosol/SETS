using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Main
{
    // ── Requests ───────────────────────────────────────────────

    public class CreateIncidentTypeRequest
    {
        public string Name { get; set; }
        public string? UserID { get; set; }
    }

    public class CreateSubCategoryRequest
    {
        public int IncidentTypeId { get; set; }
        public string Name { get; set; }
        public string? UserID { get; set; }
    }

    public class ToggleActiveRequest
    {
        public string UserID { get; set; }
    }

    public class AddTagRequest
    {
        public int SubCategoryId { get; set; }
        public string TagName { get; set; }
        public string? UserID { get; set; }
    }

    public class AddLabEntryRequest
    {
        public int SubCategoryId { get; set; }
        public string SpecimenNo { get; set; }
        public string? UserID { get; set; }
    }

    public class AddCommentRequest
    {
        public int IncidentTypeId { get; set; }
        public string CommentText { get; set; }
        public string? UserID { get; set; }
    }

    public class EditCommentRequest
    {
        public string CommentText { get; set; }
        public string? UserID { get; set; }
    }

    // ── Responses ──────────────────────────────────────────────

    public class IncidentTypeListItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public int SubCategoryCount { get; set; }
    }

    public class SubCategoryListItem
    {
        public int Id { get; set; }
        public int IncidentTypeId { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public int EntryCount { get; set; }
        public List<TagItem> Tags { get; set; }
    }

    public class TagItem
    {
        public int Id { get; set; }
        public string TagName { get; set; }
    }

    public class LabEntryItem
    {
        public int Id { get; set; }
        public string SpecimenNo { get; set; }
        public string? PID { get; set; }
        public string? PatientName { get; set; }
        public string? SampleTypeCode { get; set; }
        public string? SampleTypeName { get; set; }
        public DateOnly EntryDate { get; set; }
        public string LoggedBy { get; set; }
        public DateTime LoggedAt { get; set; }

        public string? Remarks { get; set; }
    }

    public class CommentItem
    {
        public int Id { get; set; }
        public string CommentText { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? RevisedBy { get; set; }
        public DateTime? RevisedAt { get; set; }
    }
}