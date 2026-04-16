using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.SETSDB
{
    public class Batch_NonBarcoded
    {
        public int ItemID { get; set; }
        public string BatchNo { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public DateTime Endorsed { get; set; }
        public string EndorsedBy { get; set; }
        public DateTime? ProcReceived { get; set; }
        public string? ProcReceivedBy { get; set; }
        public string Status { get; set; }
        public string? Remarks { get; set; }
    }
}
