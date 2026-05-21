using Model.HCLAB;
using Model.Main;
using Model.SETSDB;

namespace Service.Interfaces
{
    public interface IUserService
    {
        bool Login(string userid, string password, string branch);
        User_Master GetUser(string userid, string branch);
        List<User_Master> GetAll();
        void Add(User_Master user);
        void Update(User_Master user);
        Task<List<User>> GetHCLABUsers(string param);
        UserProfileResult GetProfile(string userID);
        void UpdateProfilePicture(string userID, string? base64Image);
    }
}