using Model.SETSDB;

namespace Service.Interfaces
{
    public interface ISectionService
    {
        List<Section_Master> GetAll();
        List<Section_Master> GetActive();
        Section_Master? GetByCode(string code);
        List<Section_Master> GetByBranch(string branchCode);
        void Add(Section_Master section);
        void Update(Section_Master section);
        List<Section_Master> GetByBranchIncludeInactive(string branchCode);
        Section_Master? LookupFromRemote(string partnerBranchCode, string sectionCode);
        void AddForeignSection(string partnerBranchCode, string sectionCode, string sectionName, string category, string createdBy);
        void RemoveForeignSection(string partnerBranchCode, string sectionCode, string updatedBy);
    }
}