using Model.SETSDB;
using Reposi.Context;

namespace Reposi.Repositories
{
    public class BranchRepo : GenericRepo<Branch_Master>
    {
        public BranchRepo(AppDbContext context) : base(context) { }

        public List<Branch_Master> GetActive()
            => dbSet.Where(b => b.Active == true).ToList();

        public Branch_Master? GetByCode(string code)
            => dbSet.FirstOrDefault(b => b.Code == code);
    }
}