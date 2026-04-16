using Model.SETSDB;
using Reposi.Context;

namespace Reposi.Repositories
{
    public class UserRepo : GenericRepo<User_Master>
    {
        public UserRepo(AppDbContext context) : base(context) { }

        public User_Master? GetByUserID(string userid)
            => dbSet.FirstOrDefault(u => u.UserID == userid);
    }
}