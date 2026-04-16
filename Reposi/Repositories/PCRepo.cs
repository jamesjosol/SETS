using Model.SETSDB;
using Reposi.Context;

namespace Reposi.Repositories
{
    public class PCRepo : GenericRepo<PC_Master>
    {
        public PCRepo(AppDbContext context) : base(context) { }

        public PC_Master? GetByIP(string ipAddress)
            => dbSet.FirstOrDefault(p => p.IpAddress == ipAddress && p.Active == true);
    }
}