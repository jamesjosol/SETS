using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Main
{
    public class IncomingBatchItem
    {
        public string BatchNo { get; set; }
        public string Location { get; set; }
        public string EndorsedBy { get; set; }
        public DateTime Endorsed { get; set; }
        public int TotalSpecimens { get; set; }
        public int PendingSpecimens { get; set; }
        public int TotalNonBarcoded { get; set; }
        public int PendingNonBarcoded { get; set; }
        public string Status { get; set; }
    }

    public class IncomingSpecimenItem
    {
        public string BatchNo { get; set; }
        public string Location { get; set; }
        public string SpecimenNo { get; set; }
        public string LabNo { get; set; }
        public string PID { get; set; }
        public string PatientName { get; set; }
        public string SampleTypeName { get; set; }
        public string Status { get; set; }
    }
}