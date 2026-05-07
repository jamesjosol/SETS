using Model.SETSDB;

namespace Service.Interfaces
{
    public interface IPCService
    {
        List<PC_Master> GetAll();
        PC_Master? GetByIP(string ipAddress);
        List<PC_Section> GetSectionsByIP(string ipAddress);
        List<PC_Section> GetSectionsByPCId(int pcId);
        void Add(PC_Master pc);
        void Update(PC_Master pc);
        void Delete(int id);
        void AddSection(PC_Section pcSection);
        void DeleteSection(int id);
        void DeleteSectionsByPCId(int pcId);
    }
}