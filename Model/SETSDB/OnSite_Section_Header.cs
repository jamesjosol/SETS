using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.SETSDB
{
    public class OnSite_Section_Header
    {
        public int Id { get; set; }
        public string SpecimenNo { get; set; }
        public string SectionCode { get; set; }
        public string TestGroupCode { get; set; }
        public string SampleTypeCode { get; set; }
        public string? PatientName { get; set; }
        public string? PID { get; set; }
        public DateTime? TransactionDate { get; set; }
        public string Status { get; set; }
        public string? Remarks { get; set; }
        public string? ReceivedBy { get; set; }
        public DateTime? Received { get; set; }
        public DateTime Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Updated { get; set; }
        public string? UpdatedBy { get; set; }
    }
}