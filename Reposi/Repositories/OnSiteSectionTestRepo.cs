using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.SETSDB;
using Reposi.Context;

namespace Reposi.Repositories
{
    public class OnSiteSectionTestRepo : GenericRepo<OnSite_Section_Test>
    {
        public OnSiteSectionTestRepo(AppDbContext context) : base(context) { }

        public List<OnSite_Section_Test> GetByHeaderId(int headerId)
            => dbSet.Where(t => t.HeaderId == headerId).OrderBy(t => t.TestCode).ToList();

        public List<OnSite_Section_Test> GetByHeaderIds(List<int> headerIds)
            => dbSet.ToList().Where(t => headerIds.Contains(t.HeaderId)).ToList();

        public List<OnSite_Section_Test> GetAllRunningTests()
            => dbSet.Where(t => t.Status == "R").ToList();

        public List<OnSite_Section_Test> GetDueScheduledTests(DateOnly today)
            => dbSet
                .Where(t => t.Status == "S"
                         && t.RunningDate != null
                         && t.RunningDate <= today
                         && (t.ScheduleTag == "END" || t.ScheduleTag == "CRD" || t.ScheduleTag == "SRD"))
                .ToList();
    }
}