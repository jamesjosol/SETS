using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.SETSDB
{
    public class Specimen_Section_Header
    {
        public int Id { get; set; }
        public string SpecimenNo { get; set; }
        public string SectionCode { get; set; }
        public string TestGroupCode { get; set; }
        public string SampleTypeCode { get; set; }
        public string Status { get; set; }
        public string RoutedBy { get; set; }
        public DateTime Routed { get; set; }
        public string? ReceivedBy { get; set; }
        public DateTime? Received { get; set; }
        public string? Remarks { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? Updated { get; set; }
    }
}
