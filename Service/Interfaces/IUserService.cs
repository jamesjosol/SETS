using Model.SETSDB;

namespace Service.Interfaces
{
    public interface IUserService
    {
        bool Login(string userid, string password, string branch);
        User_Master GetUser(string userid, string branch);
        void Update(User_Master user);
    }
}