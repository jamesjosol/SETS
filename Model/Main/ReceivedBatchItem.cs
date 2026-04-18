using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Main
{
    public class ReceivedBatchItem
    {
        public string BatchNo { get; set; }
        public string Location { get; set; }   
        public DateTime Endorsed { get; set; }
        public string EndorsedBy { get; set; }
        public DateTime? ProcReceived { get; set; }
        public string? ReceivedBy { get; set; } 
        public string Status { get; set; }
    }
}