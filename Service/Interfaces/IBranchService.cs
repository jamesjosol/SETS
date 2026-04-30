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
    }
}