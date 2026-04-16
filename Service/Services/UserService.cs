using HCLAB;
using Model.SETSDB;
using Reposi;
using Reposi.Context;
using Service.Interfaces;

namespace Service.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContextFactory _factory;
        private readonly string _branch;

        public UserService(AppDbContextFactory factory, string branch)
        {
            _factory = factory;
            _branch = SetsConnection.ConnectionString(branch);
        }

        public bool Login(string userid, string password, string branch)
        {
            try
            {
                int _auth = HclabMaster.HCLABUsers.Auth(
                    userid,
                    password,
                    HclabConnection.ConnectionString(branch)
                );
                return _auth != 0;
            }
            catch
            {
                throw;
            }
        }

        public User_Master GetUser(string userid, string branch)
        {
            try
            {
                using var context = _factory.CreateContext(SetsConnection.ConnectionString(branch));
                using var unit = new UnitOfWork(context);
                return unit.Users.GetByUserID(userid);
            }
            catch
            {
                throw;
            }
        }

        public void Update(User_Master user)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                unit.Users.Update(user);
            }
            catch { throw; }
        }
    }
}