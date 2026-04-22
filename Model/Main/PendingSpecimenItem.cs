using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Main
{
    public class PendingSpecimenItem
    {
        public int Id { get; set; }
        public string SpecimenNo { get; set; }
        public string SectionCode { get; set; }
        public string TestGroupCode { get; set; }
        public string SampleTypeCode { get; set; }
        public string SampleTypeName { get; set; }
        public string Status { get; set; }
        public string RoutedBy { get; set; }
        public DateTime Routed { get; set; }
        public string? ReceivedBy { get; set; }
        public DateTime? Received { get; set; }
        public string? Remarks { get; set; }
        public string? PatientName { get; set; }
        public string? PatientID { get; set; }
    }
}