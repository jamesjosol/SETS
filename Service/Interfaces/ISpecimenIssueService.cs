using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Main;

namespace Service.Interfaces
{
    public interface ISpecimenIssueService
    {
        // Incident Types
        List<IncidentTypeListItem> GetIncidentTypes();
        void CreateIncidentType(CreateIncidentTypeRequest request);
        void ToggleIncidentType(int id, ToggleActiveRequest request);

        // Sub-Categories
        List<SubCategoryListItem> GetSubCategories(int incidentTypeId);
        void CreateSubCategory(CreateSubCategoryRequest request);
        void ToggleSubCategory(int id, ToggleActiveRequest request);

        // Tags
        void AddTag(AddTagRequest request);
        void DeleteTag(int id);
        List<string> GetAllTagNames();

        // Lab Entries
        List<LabEntryItem> GetLabEntries(int subCategoryId);
        Task AddLabEntry(AddLabEntryRequest request);
        void UpdateLabEntryRemark(int id, UpdateLabEntryRemarkRequest request);
        void DeleteLabEntry(int id);

        // Comments
        List<CommentItem> GetComments(int incidentTypeId);
        void AddComment(AddCommentRequest request);
        void EditComment(int id, EditCommentRequest request);
    }
}