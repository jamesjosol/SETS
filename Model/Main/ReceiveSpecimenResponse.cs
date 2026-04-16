using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Main
{
    public class ReceiveSpecimenResponse
    {
        public string SpecimenNo { get; set; }
        public string BatchNo { get; set; }
        public string PID { get; set; }        // ← add
        public string PatientName { get; set; }
        public string SampleTypeName { get; set; }
        public string Location { get; set; }   // ← add this too while we're here
        public string LocationName { get; set; } // ← and this
        public DateTime ProcReceived { get; set; }
        public string ProcReceivedBy { get; set; }
        public string BatchStatus { get; set; }
    }
}