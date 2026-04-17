using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Main
{
    public class ReceiveSpecimenRequest
    {
        public string UserID { get; set; }
        public string SpecimenNo { get; set; }
        public string? CurrentBatchNo { get; set; }    // ← add — null on first scan
        public string? ReceivingRemarks { get; set; }
    }
}