using Model.SETSDB;
using Reposi.Context;

namespace Reposi.Repositories
{
    public class PCSectionRepo : GenericRepo<PC_Section>
    {
        public PCSectionRepo(AppDbContext context) : base(context) { }

        public List<PC_Section> GetByPCId(int pcId)
            => dbSet.Where(ps => ps.PCId == pcId).ToList();
    }
}