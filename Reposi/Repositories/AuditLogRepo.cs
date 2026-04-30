using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.SETSDB;
using Reposi.Context;

namespace Reposi.Repositories
{
    public class AuditLogRepo : GenericRepo<Audit_Log>
    {
        public AuditLogRepo(AppDbContext context) : base(context) { }

        public List<Audit_Log> GetByUser(string userID, DateTime dateFrom, DateTime dateTo)
            => dbSet.Where(a => a.UserID == userID
                             && a.LoggedAt >= dateFrom
                             && a.LoggedAt < dateTo.AddDays(1))
                    .OrderByDescending(a => a.LoggedAt)
                    .ToList();

        public List<Audit_Log> GetBySpecimenNo(string specimenNo)
            => dbSet.Where(a => a.SpecimenNo == specimenNo)
                    .OrderByDescending(a => a.LoggedAt)
                    .ToList();

        public List<Audit_Log> GetByBatchNo(string batchNo)
            => dbSet.Where(a => a.BatchNo == batchNo)
                    .OrderByDescending(a => a.LoggedAt)
                    .ToList();
    }
}