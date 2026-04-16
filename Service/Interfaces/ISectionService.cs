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
    }
}