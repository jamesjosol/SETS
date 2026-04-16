using Model.SETSDB;

namespace Service.Interfaces
{
    public interface IPCService
    {
        PC_Master? GetByIP(string ipAddress);
        List<PC_Section> GetSectionsByIP(string ipAddress);
        void Add(PC_Master pc);
        void Update(PC_Master pc);
        void AddSection(PC_Section pcSection);
        void DeleteSection(int id);
    }
}