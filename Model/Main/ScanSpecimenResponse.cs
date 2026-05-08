using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Main
{
    public class ScanSpecimenResponse
    {
        public int HeaderId { get; set; }
        public string SpecimenNo { get; set; }
        public string SectionCode { get; set; }
        public string TestGroupCode { get; set; }
        public string SampleTypeCode { get; set; }
        public string? SampleTypeName { get; set; }
        public string Status { get; set; }
        public bool FirstScan { get; set; }     
        public DateTime? Received { get; set; }
        public string? ReceivedBy { get; set; }
        public string? Remarks { get; set; }
        public string? PatientName { get; set; } 
        public string? PID { get; set; }
        public string? CutOffTime { get; set; }
        public List<ScanSpecimenTestItem> Tests { get; set; }
    }

    public class ScanSpecimenTestItem
    {
        public int Id { get; set; }
        public string TestCode { get; set; }
        public string TestName { get; set; }
        public string Status { get; set; }
        public string? ScheduleTag { get; set; }
        public DateOnly? RunningDate { get; set; }
        public string? AssignedRMT { get; set; }
        public DateTime? Assigned { get; set; }
        public DateTime? RunAt { get; set; }
        public bool HasRunningDay { get; set; }
        public bool IsTodayRunningDay { get; set; } 
    }
}