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
        public const string TestRescheduled = "TEST_RESCHEDULED";
        public const string ResultReleased = "RESULT_RELEASED";
        public const string SpecimenReleased = "SPECIMEN_RELEASED";
    }
}