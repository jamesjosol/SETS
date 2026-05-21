using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.SETSDB
{
    public static class AuditEvents
    {
        public const string Endorsed = "ENDORSED";
        public const string Endorsed14Days = "ENDORSED_14DAYS";
        public const string EndorsedDuplicate = "ENDORSED_DUPLICATE";
        public const string ProcReceived = "PROC_RECEIVED";
        public const string SpecimenCancelled = "SPECIMEN_CANCELLED";
        public const string SectionReceived = "SECTION_RECEIVED";
        public const string SpecimenStored = "SPECIMEN_STORED";
        public const string TestScheduled = "TEST_SCHEDULED";
        public const string TestRescheduled = "TEST_RESCHEDULED";
        public const string TestRun = "TEST_RUN";
        public const string ResultReleased = "RESULT_RELEASED";
        public const string ScheduleDue = "SCHEDULE_DUE";
        public const string SpecimenCancelledSection = "SPECIMEN_CANCELLED_SECTION";
        public const string TestAborted = "TEST_ABORTED";
        public const string SpecimenFlagged = "SPECIMEN_FLAGGED";
        public const string SpecimenUnflagged = "SPECIMEN_UNFLAGGED";
        public const string FlaggedSpecimenReceived = "FLAGGED_SPECIMEN_RECEIVED";
        public const string FlaggedSpecimenDeclined = "FLAGGED_SPECIMEN_DECLINED";
        public const string SpecimenAlertSet = "SPECIMEN_ALERT_SET";
        public const string SpecimenAlertCleared = "SPECIMEN_ALERT_CLEARED";
    }
}