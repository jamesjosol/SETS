using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.SETSDB
{
    public class Processing_Options
    {
        public string BranchCode { get; set; }
        public bool ShowTemperature { get; set; }
        public bool ShowTempRemarks { get; set; }
        public bool ShowBagNo { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? Updated { get; set; }
    }
}