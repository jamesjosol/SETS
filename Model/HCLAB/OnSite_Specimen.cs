using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.HCLAB
{
    public class OnSite_Specimen
    {
        public string SpecimenNo { get; set; }
        public string LabNo { get; set; }
        public string SampleTypeCode { get; set; }
        public string? PatientName { get; set; }
        public string? PID { get; set; }
        public DateTime? TrxDate { get; set; }
        public string TestCode { get; set; }
        public string TestName { get; set; }
        public string TestGroup { get; set; }
    }
}