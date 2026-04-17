using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.SETSDB
{
    public class Batch_Header
    {
        public string BatchNo { get; set; }
        public string EndorsedBy { get; set; }
        public DateTime Endorsed { get; set; }
        public string Location { get; set; }
        public string ProcDestination { get; set; }
        public string Status { get; set; }
        public DateTime? ProcReceived { get; set; }
        public DateTime? Completed { get; set; }
        public string? Temp { get; set; }
        public string? TempRemarks { get; set; }
        public string? BagNo { get; set; }
    }
}
