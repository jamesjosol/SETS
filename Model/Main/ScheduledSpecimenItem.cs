using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Main
{
    public class ScheduledSpecimenItem
    {
        public int HeaderId { get; set; }
        public string SpecimenNo { get; set; }
        public string SectionCode { get; set; }
        public string SampleTypeCode { get; set; }
        public string SampleTypeName { get; set; }
        public string? PatientName { get; set; }
        public string? PatientID { get; set; }
        public string? Remarks { get; set; }
        public string? ReceivedBy { get; set; }
        public DateTime? Received { get; set; }
        public List<ScheduledTestItem> Tests { get; set; } = new();
        public bool IsOnSite { get; set; }
    }

    public class ScheduledTestItem
    {
        public int Id { get; set; }
        public string TestCode { get; set; }
        public string TestName { get; set; }
        public string Status { get; set; }
        public string? ScheduleTag { get; set; }
        public DateOnly? RunningDate { get; set; }
        public string? AssignedRMT { get; set; }
    }
}