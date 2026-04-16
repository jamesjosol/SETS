using Model.SETSDB;

namespace Service.Interfaces
{
    public interface IUserSectionService
    {
        List<User_Section> GetByUserID(string userID);
        User_Section? GetByUserAndSection(string userID, string sectionCode);
        void Add(User_Section userSection);
        void Update(User_Section userSection);
        void Delete(int id);
    }
}