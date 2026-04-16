using Model.SETSDB;
using Reposi;
using Reposi.Context;
using Service.Interfaces;

namespace Service.Services
{
    public class UserSectionService : IUserSectionService
    {
        private readonly AppDbContextFactory _factory;
        private readonly string _branch;

        public UserSectionService(AppDbContextFactory factory, string branch)
        {
            _factory = factory;
            _branch = SetsConnection.ConnectionString(branch);
        }

        public List<User_Section> GetByUserID(string userID)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                return unit.UserSections.GetByUserID(userID);
            }
            catch { throw; }
        }

        public User_Section? GetByUserAndSection(string userID, string sectionCode)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                return unit.UserSections.GetByUserAndSection(userID, sectionCode);
            }
            catch { throw; }
        }

        public void Add(User_Section userSection)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                unit.UserSections.Add(userSection);
            }
            catch { throw; }
        }

        public void Update(User_Section userSection)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                unit.UserSections.Update(userSection);
            }
            catch { throw; }
        }

        public void Delete(int id)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                unit.UserSections.Delete(id);
            }
            catch { throw; }
        }
    }
}