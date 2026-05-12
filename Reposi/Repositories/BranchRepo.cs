using Model.SETSDB;
using Reposi.Context;

namespace Reposi.Repositories
{
    public class BranchRepo : GenericRepo<Branch_Master>
    {
        public BranchRepo(AppDbContext context) : base(context) { }

        public List<Branch_Master> GetActive()
            => dbSet.Where(b => b.Active).ToList();

        public Branch_Master? GetByCode(string code)
            => dbSet.FirstOrDefault(b => b.Code == code);

        public bool CodeExists(string code)
            => dbSet.Any(b => b.Code == code);

        public List<Branch_Master> GetExternal()
            => dbSet.Where(b => b.IsExternal).ToList();

        public List<Branch_Master> GetActiveExternal()
            => dbSet.Where(b => b.IsExternal && b.Active).ToList();

        public List<Branch_Master> GetEligibleForPartner()
            => dbSet.Where(b => !b.IsExternal).ToList();
    }
}