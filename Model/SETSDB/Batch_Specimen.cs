using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.SETSDB
{
    public class Batch_Specimen
    {
        public int Id { get; set; }
        public string SpecimenNo { get; set; }
        public string BatchNo { get; set; }
        public string LabNo { get; set; }
        public DateTime TrxDate { get; set; }
        public string PID { get; set; }
        public string PatientName { get; set; }
        public string SampleTypeCode { get; set; }
        public string SampleTypeName { get; set; }
        public DateTime Endorsed { get; set; }
        public string EndorsedBy { get; set; }
        public string Status { get; set; }
        public string? Remarks { get; set; }
    }
}
