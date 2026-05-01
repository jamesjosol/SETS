using Model.Main;
using Model.SETSDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.SETSDB
{
    public class Audit_Log
    {
        public int Id { get; set; }
        public string EventCode { get; set; } = string.Empty;
        public string? SpecimenNo { get; set; }
        public string? BatchNo { get; set; }
        public string? PatientName { get; set; }
        public string? PID { get; set; }
        public string? FromLocation { get; set; }
        public string? ToLocation { get; set; }
        public string? Status { get; set; }
        public DateTime? RunningDate { get; set; }
        public DateTime? TxDate { get; set; }
        public string UserID { get; set; } = string.Empty;
        public DateTime LoggedAt { get; set; }
        public string? Remarks { get; set; }
        public DateTime? RunAt { get; set; }
        public string? Tests { get; set; }
        public bool IsOutsideTat { get; set; }
        public bool IsOutsideProcTat { get; set; }

        // ── Factories ──────────────────────────────────────────────────────────

        public static Audit_Log Endorsed(EndorseSpecimen s, string batchNo, string fromLocation, string toLocation, string userID, bool isOutsideTat = false)
            => new()
            {
                EventCode = s.IsDuplicate ? AuditEvents.EndorsedDuplicate
                             : s.IsBeyond14Days ? AuditEvents.Endorsed14Days
                             : AuditEvents.Endorsed,
                SpecimenNo = s.SpecimenNo,
                BatchNo = batchNo,
                PatientName = s.PatientName,
                PID = s.PID,
                FromLocation = fromLocation,
                ToLocation = toLocation,
                TxDate = s.TrxDate,
                Remarks = s.Remarks,
                UserID = userID,
                IsOutsideTat = isOutsideTat
            };
        
        public static Audit_Log ProcReceived(Batch_Specimen specimen, string fromLocation, string toLocation, string userID, bool isOutsideProcTat = false)
            => new()
            {
                EventCode = AuditEvents.ProcReceived,
                SpecimenNo = specimen.SpecimenNo,
                BatchNo = specimen.BatchNo,
                PatientName = specimen.PatientName,
                PID = specimen.PID,
                FromLocation = fromLocation,
                ToLocation = toLocation,
                UserID = userID,
                IsOutsideProcTat = isOutsideProcTat
            };

        public static Audit_Log SpecimenCancelled(string specimenNo, string batchNo, string patientName, string pid, string fromLocation, string toLocation, string cancelReason, string userID)
            => new()
            {
                EventCode = AuditEvents.SpecimenCancelled,
                SpecimenNo = specimenNo,
                BatchNo = batchNo,
                PatientName = patientName,
                PID = pid,
                FromLocation = fromLocation,
                ToLocation = toLocation,
                Remarks = cancelReason,
                UserID = userID
            };

        public static Audit_Log SectionReceived(string specimenNo, string fromLocation, string toLocation, string userID)
            => new()
            {
                EventCode = AuditEvents.SectionReceived,
                SpecimenNo = specimenNo,
                FromLocation = fromLocation,
                ToLocation = toLocation,
                UserID = userID
            };

        public static Audit_Log SpecimenStored(string specimenNo, string toLocation, string userID)
            => new()
            {
                EventCode = AuditEvents.SpecimenStored,
                SpecimenNo = specimenNo,
                ToLocation = toLocation,
                UserID = userID
            };


        public static Audit_Log TestRescheduled(string specimenNo, string toLocation, string testName, string previousTag, string newTag, DateTime runningDate, string userID)
            => new()
            {
                EventCode = AuditEvents.TestRescheduled,
                SpecimenNo = specimenNo,
                ToLocation = toLocation,
                Tests = testName,
                Status = $"{previousTag} → {newTag}",
                RunningDate = runningDate,
                UserID = userID
            };

        public static Audit_Log TestScheduled(string specimenNo, string toLocation, string testName, string scheduleTag, DateTime runningDate, string userID)
            => new()
            {
                EventCode = AuditEvents.TestScheduled,
                SpecimenNo = specimenNo,
                ToLocation = toLocation,
                Tests = testName,
                Status = scheduleTag,
                RunningDate = runningDate,
                UserID = userID
            };

        public static Audit_Log ResultReleased(string specimenNo, string toLocation, string testCode, string testName)
            => new()
            {
                EventCode = AuditEvents.ResultReleased,
                SpecimenNo = specimenNo,
                ToLocation = toLocation,
                Tests = $"{testCode}:{testName}",
                UserID = "MIDDLEWARE"
            };

        public static Audit_Log ScheduleDue(string specimenNo, string toLocation, string testCode, string testName, string scheduleTag, DateTime runningDate)
            => new()
            {
                EventCode = AuditEvents.ScheduleDue,
                SpecimenNo = specimenNo,
                ToLocation = toLocation,
                Tests = $"{testCode}:{testName}",
                Status = scheduleTag,
                RunningDate = runningDate,
                UserID = "MIDDLEWARE"
            };

        public static Audit_Log TestRun(string specimenNo, string toLocation, string testNames, string rmtUserID, string userID, DateTime runAt)
            => new()
            {
                EventCode = AuditEvents.TestRun,
                SpecimenNo = specimenNo,
                ToLocation = toLocation,
                Tests = testNames,
                RunAt = runAt,
                Remarks = $"RMT: {rmtUserID}",
                UserID = userID
            };
    }
}