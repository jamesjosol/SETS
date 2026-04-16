using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Main
{
    public class EndorseRequest
    {
        public string SectionCode { get; set; }
        public string UserID { get; set; }
        public string ProcDestination { get; set; }
        public List<EndorseSpecimen> Specimens { get; set; } = new();
        public List<EndorseNonBarcoded> NonBarcoded { get; set; } = new();
    }

    public class EndorseSpecimen
    {
        public string SpecimenNo { get; set; }
        public string LabNo { get; set; }
        public DateTime TrxDate { get; set; }
        public string PID { get; set; }
        public string PatientName { get; set; }
        public string SampleTypeCode { get; set; }
        public string SampleTypeName { get; set; }
        public string? Remarks { get; set; }
    }

    public class EndorseNonBarcoded
    {
        public string Description { get; set; }
        public int Quantity { get; set; }
        public string? Remarks { get; set; }
    }
}