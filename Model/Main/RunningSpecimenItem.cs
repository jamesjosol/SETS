using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Main
{
    public class RunningSpecimenItem
    {
        public int HeaderId { get; set; }
        public string SpecimenNo { get; set; }
        public string SectionCode { get; set; }
        public string SampleTypeCode { get; set; }
        public string SampleTypeName { get; set; }
        public string? PatientName { get; set; }
        public string? PatientID { get; set; }
        public string? Remarks { get; set; }
        public DateTime? Received { get; set; }
        public List<RunningTestItem> Tests { get; set; } = new();
    }

    public class RunningTestItem
    {
        public int Id { get; set; }
        public string TestCode { get; set; }
        public string TestName { get; set; }
        public string? AssignedRMT { get; set; }
        public DateTime? Assigned { get; set; }
        public DateTime? RunAt { get; set; }
    }
}