using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Main;
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

        public List<AuditLogDto> GetByUser(string userID, DateTime dateFrom, DateTime dateTo)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                var logs = unit.AuditLogs.GetByUser(userID, dateFrom, dateTo);
                return MapToDto(context, logs);
            }
            catch { throw; }
        }

        public List<AuditLogDto> GetBySpecimenNo(string specimenNo)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                var logs = unit.AuditLogs.GetBySpecimenNo(specimenNo);
                return MapToDto(context, logs);
            }
            catch { throw; }
        }

        public List<AuditLogDto> GetByBatchNo(string batchNo)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                var logs = unit.AuditLogs.GetByBatchNo(batchNo);
                return MapToDto(context, logs);
            }
            catch { throw; }
        }

        // ── Private helpers ───────────────────────────────────────────────────

        private static List<AuditLogDto> MapToDto(AppDbContext context, List<Audit_Log> logs)
        {
            // Load ALL sections into memory first — avoid EF Core CTE issue with Contains()
            var sections = context.Section_Master
                .ToList()
                .ToDictionary(s => s.Code, s => s.Name);

            return logs.Select(l => new AuditLogDto
            {
                Id = l.Id,
                EventCode = l.EventCode,
                EventLabel = ResolveEventLabel(l.EventCode),
                SpecimenNo = l.SpecimenNo,
                BatchNo = l.BatchNo,
                PatientName = l.PatientName,
                PID = l.PID,
                FromLocation = l.FromLocation,
                FromLocationName = l.FromLocation != null && sections.TryGetValue(l.FromLocation, out var fn) ? fn : l.FromLocation,
                ToLocation = l.ToLocation,
                ToLocationName = l.ToLocation != null && sections.TryGetValue(l.ToLocation, out var tn) ? tn : l.ToLocation,
                Status = l.Status,
                RunningDate = l.RunningDate,
                TxDate = l.TxDate,
                UserID = l.UserID,
                LoggedAt = l.LoggedAt,
                Remarks = l.Remarks,
                Tests = l.Tests,
                RunAt = l.RunAt,
                IsOutsideTat = l.IsOutsideTat,
                IsOutsideProcTat = l.IsOutsideProcTat,
            }).ToList();
        }

        private static string ResolveEventLabel(string eventCode) => eventCode switch
        {
            AuditEvents.Endorsed => "Endorsed",
            AuditEvents.Endorsed14Days => "Endorsed (>14 Days)",
            AuditEvents.EndorsedDuplicate => "Re-endorsed",
            AuditEvents.ProcReceived => "Processing Received",
            AuditEvents.SpecimenCancelled => "Specimen Cancelled",
            AuditEvents.SectionReceived => "Section Received",
            AuditEvents.ResultReleased => "Result Released",
            AuditEvents.ScheduleDue => "Schedule Due",
            AuditEvents.TestRun => "Test Run",
            AuditEvents.SpecimenStored => "Specimen Stored",
            AuditEvents.TestScheduled => "Test Scheduled",
            AuditEvents.TestRescheduled => "Test Rescheduled",
            AuditEvents.SpecimenCancelledSection => "Specimen Cancelled (Section)",
            AuditEvents.TestAborted => "Test Aborted",
            _ => eventCode
        };
    }
}