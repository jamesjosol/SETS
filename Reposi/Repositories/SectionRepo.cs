using Model.SETSDB;
using Reposi.Context;

namespace Reposi.Repositories
{
    public class SectionRepo : GenericRepo<Section_Master>
    {
        public SectionRepo(AppDbContext context) : base(context) { }

        public List<Section_Master> GetActive()
            => dbSet.Where(s => s.Active == true).ToList();

        public List<Section_Master> GetByBranch(string branchCode)
            => dbSet.Where(s => s.BranchCode == branchCode && s.Active == true).ToList();

        public Section_Master? GetByCode(string code)
            => dbSet.FirstOrDefault(s => s.Code == code);
    }
}