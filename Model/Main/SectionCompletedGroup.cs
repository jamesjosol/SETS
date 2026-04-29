using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Model.Main
{
    public class SectionCompletedGroup
    {
        public string SectionCode { get; set; }
        public string SectionName { get; set; }
        public List<CompletedSpecimenItem> Specimens { get; set; }
    }

    public class CompletedSpecimenItem
    {
        public int HeaderId { get; set; }
        public string SpecimenNo { get; set; }
        public string PID { get; set; }
        public string PatientName { get; set; }
        public string SampleTypeCode { get; set; }
        public string SampleTypeName { get; set; }
        public DateTime? Received { get; set; }
        public string? ReceivedBy { get; set; }
        public DateTime? Completed { get; set; }
        public string? CompletedBy { get; set; }
        public List<CompletedTestItem> Tests { get; set; }
        public bool IsOnSite { get; set; }
    }

    public class CompletedTestItem
    {
        public int Id { get; set; }
        public string TestCode { get; set; }
        public string TestName { get; set; }
        public string? AssignedRMT { get; set; }
        public DateTime? RunAt { get; set; }
        public DateTime? Assigned { get; set; }
    }
}