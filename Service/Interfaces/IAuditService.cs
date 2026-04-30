using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.SETSDB;

namespace Service.Interfaces
{
    public interface IAuditService
    {
        void Log(Audit_Log entry);
        List<Audit_Log> GetByUser(string userID, DateTime dateFrom, DateTime dateTo);
        List<Audit_Log> GetBySpecimenNo(string specimenNo);
        List<Audit_Log> GetByBatchNo(string batchNo);
    }
}