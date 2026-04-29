using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Main
{
    public class ScanOnSiteSpecimenResponse
    {
        public int HeaderId { get; set; }
        public string SpecimenNo { get; set; }
        public string SectionCode { get; set; }
        public string TestGroupCode { get; set; }
        public string SampleTypeCode { get; set; }
        public string SampleTypeName { get; set; }
        public string? PatientName { get; set; }
        public string? PID { get; set; }
        public DateTime? TransactionDate { get; set; }
        public string Status { get; set; }
        public bool FirstScan { get; set; }
        public string? ReceivedBy { get; set; }
        public DateTime? Received { get; set; }
        public string? Remarks { get; set; }
        public List<ScanSpecimenTestItem> Tests { get; set; }
    }
}