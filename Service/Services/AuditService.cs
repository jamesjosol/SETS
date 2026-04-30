using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.SETSDB;
using Reposi;
using Reposi.Context;
using Service.Interfaces;

namespace Service.Services
{
    public class AuditService : IAuditService
    {
        private readonly AppDbContextFactory _factory;
        private readonly string _branch;

        public AuditService(AppDbContextFactory factory, string branch)
        {
            _factory = factory;
            _branch = SetsConnection.ConnectionString(branch);
        }

        public void Log(Audit_Log entry)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                entry.LoggedAt = DateTime.Now;
                unit.AuditLogs.Add(entry);
            }
            catch { throw; }
        }

        public List<Audit_Log> GetByUser(string userID, DateTime dateFrom, DateTime dateTo)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                return unit.AuditLogs.GetByUser(userID, dateFrom, dateTo);
            }
            catch { throw; }
        }

        public List<Audit_Log> GetBySpecimenNo(string specimenNo)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                return unit.AuditLogs.GetBySpecimenNo(specimenNo);
            }
            catch { throw; }
        }

        public List<Audit_Log> GetByBatchNo(string batchNo)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                return unit.AuditLogs.GetByBatchNo(batchNo);
            }
            catch { throw; }
        }
    }
}