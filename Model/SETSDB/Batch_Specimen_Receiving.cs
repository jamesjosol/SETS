using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.SETSDB
{
    public class Batch_Specimen_Receiving
    {
        public int Id { get; set; }
        public string SpecimenNo { get; set; }
        public string BatchNo { get; set; }
        public DateTime ProcReceived { get; set; }
        public string ProcReceivedBy { get; set; }
        public string? ReceivingRemarks { get; set; }
    }
}