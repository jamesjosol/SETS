using Model.SETSDB;
using Reposi.Context;

namespace Reposi.Repositories
{
    public class UserSectionRepo : GenericRepo<User_Section>
    {
        public UserSectionRepo(AppDbContext context) : base(context) { }

        public List<User_Section> GetByUserID(string userID)
            => dbSet.Where(us => us.UserID == userID && us.Active == true).ToList();

        public User_Section? GetByUserAndSection(string userID, string sectionCode)
            => dbSet.FirstOrDefault(us => us.UserID == userID && us.SectionCode == sectionCode && us.Active);
    }
}