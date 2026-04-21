using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.SETSDB;
using Reposi.Context;

namespace Reposi.Repositories
{
    public class SpecimenSectionTestRepo : GenericRepo<Specimen_Section_Test>
    {
        public SpecimenSectionTestRepo(AppDbContext context) : base(context) { }

        public List<Specimen_Section_Test> GetByHeaderId(int headerId)
            => dbSet.Where(t => t.HeaderId == headerId).ToList();

        public List<Specimen_Section_Test> GetPendingByHeader(int headerId)
            => dbSet.Where(t => t.HeaderId == headerId && t.Status == "P").ToList();

        public List<Specimen_Section_Test> GetByAssignedRMT(string userID)
            => dbSet.Where(t => t.AssignedRMT == userID && t.Status == "R").ToList();

        public List<Specimen_Section_Test> GetSavedDueToday(DateOnly today)
            => dbSet.Where(t => t.Status == "S" && t.RunningDate == today).ToList();

        public bool AllCompleted(int headerId)
            => dbSet.Where(t => t.HeaderId == headerId).All(t => t.Status == "X");
    }
}