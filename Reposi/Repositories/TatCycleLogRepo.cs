using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.SETSDB;
using Reposi.Context;

namespace Reposi.Repositories
{
    public class TatCycleLogRepo : GenericRepo<Tat_Cycle_Log>
    {
        public TatCycleLogRepo(AppDbContext context) : base(context) { }

        public Tat_Cycle_Log? GetOpenCycle(string sectionCode)
            => dbSet.FirstOrDefault(c => c.SectionCode == sectionCode && c.CycleEnd == null);

        public List<Tat_Cycle_Log> GetBySection(string sectionCode)
            => dbSet.Where(c => c.SectionCode == sectionCode)
                    .OrderByDescending(c => c.CycleStart)
                    .ToList();

        public Tat_Cycle_Log? GetLastClosed(string sectionCode)
            => dbSet.Where(c => c.SectionCode == sectionCode && c.CycleEnd != null)
                    .OrderByDescending(c => c.CycleEnd)
                    .FirstOrDefault();
    }
}