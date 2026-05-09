using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Main
{
    public class EndorseResult
    {
        public string BatchNo { get; set; }
        public string ProcDestination { get; set; }  // processing section code
        public string LocationName { get; set; }      // endorsing section name (for notif message)
    }

    public class CancelSpecimenResult
    {
        public string EndorsedBy { get; set; }
    }

    public class SaveAssignmentsResult
    {
        public List<CompletedHeaderInfo> CompletedHeaders { get; set; } = new();
    }

    public class CompletedHeaderInfo
    {
        public string SpecimenNo { get; set; }
        public string SectionCode { get; set; }
    }

    public class MiddlewareNotificationRequest
    {
        public string Branch { get; set; }
        public string NotifType { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string? TargetCategory { get; set; }
        public string? TargetSection { get; set; }
        public string? TargetUserID { get; set; }
        public bool IsAdmin { get; set; } = false;
        public string? ReferenceID { get; set; }
    }
}
