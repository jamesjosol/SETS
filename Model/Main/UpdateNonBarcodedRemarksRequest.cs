using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Main
{
    public class UpdateNonBarcodedRemarksRequest
    {
        public int ItemID { get; set; }
        public string? ReceivingRemarks { get; set; }
    }
}