using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Main;
using Model.SETSDB;

namespace Service.Interfaces
{
    public interface IAuditService
    {
        void Log(Audit_Log entry);
        List<AuditLogDto> GetByUser(string userID, DateTime dateFrom, DateTime dateTo);
        List<AuditLogDto> GetBySpecimenNo(string specimenNo);
        List<AuditLogDto> GetByBatchNo(string batchNo);
    }
}