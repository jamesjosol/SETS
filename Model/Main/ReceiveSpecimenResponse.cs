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
        public string PID { get; set; }
        public string PatientName { get; set; }
        public string SampleTypeName { get; set; }
        public string Location { get; set; }
        public string LocationName { get; set; }
        public DateTime ProcReceived { get; set; }
        public string ProcReceivedBy { get; set; }
        public string BatchStatus { get; set; }
        public string? Temp { get; set; }           // ← add
        public string? TempRemarks { get; set; }    // ← add
        public string? BagNo { get; set; }          // ← add
    }
}