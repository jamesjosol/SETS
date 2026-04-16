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
        public string? Temp { get; set; }
        public string? TempRemarks { get; set; }
        public string? BagNo { get; set; }
        public string? ReceivingRemarks { get; set; }
    }
}