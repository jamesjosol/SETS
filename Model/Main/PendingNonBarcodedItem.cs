using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Main
{
    public class PendingNonBarcodedItem
    {
        public int ItemID { get; set; }
        public string BatchNo { get; set; }
        public string Location { get; set; }
        public string LocationName { get; set; }
        public string Type { get; set; }         
        public string? LabNo { get; set; }   
        public string Description { get; set; }
        public int Quantity { get; set; }
        public DateTime Endorsed { get; set; }
        public string EndorsedBy { get; set; }
        public string? Remarks { get; set; }
        public string? ReceivingRemarks { get; set; }
    }
}