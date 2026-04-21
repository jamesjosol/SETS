using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Main
{
    public class SaveAssignmentsRequest
    {
        public string UserID { get; set; }
        public List<TestAssignmentItem> Assignments { get; set; }
        public List<SpecimenRemarkItem> SpecimenRemarks { get; set; } = new();
    }

    public class TestAssignmentItem
    {
        public int TestId { get; set; }
        public string? AssignedRMT { get; set; }
        public string? ScheduleTag { get; set; }
        public DateOnly? RunningDate { get; set; }
    }

    public class SpecimenRemarkItem
    {
        public int HeaderId { get; set; }
        public string? Remarks { get; set; }
    }
}