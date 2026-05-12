using Model.SETSDB;

namespace Service.Interfaces
{
    public interface IBranchService
    {
        List<Branch_Master> GetAll();
        List<Branch_Master> GetActive();
        Branch_Master? GetByCode(string code);
        void Add(Branch_Master branch);
        void Update(Branch_Master branch);
        bool CodeExists(string code);
        void Toggle(string code, string updatedBy);
        List<Branch_Master> GetExternal();
        List<Branch_Master> GetActiveExternal();
        List<Branch_Master> GetEligibleForPartner(string localBranchCode);
        void RegisterAsExternal(string code, string updatedBy);
        void ToggleExternal(string code, string updatedBy);
        void UnregisterExternal(string code, string updatedBy);
    }
}