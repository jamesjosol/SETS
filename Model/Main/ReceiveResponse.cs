using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Main
{
    public class ReceiveResponse
    {
        public string BatchNo { get; set; }
        public string Status { get; set; }        // P, PA, C
        public bool IsCompleted { get; set; }     // true if all specimens + non-barcoded received
        public int TotalSpecimens { get; set; }
        public int ReceivedSpecimens { get; set; }
        public int TotalNonBarcoded { get; set; }
        public int ReceivedNonBarcoded { get; set; }
    }
}