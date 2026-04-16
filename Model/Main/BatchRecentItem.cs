using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Main
{
    public class BatchRecentItem
    {
        public string BatchNo { get; set; }
        public string? Location { get; set; }         // admin only — endorser section full name
        public DateTime Endorsed { get; set; }
        public string EndorsedBy { get; set; }
        public string Destination { get; set; }       // processing section full name
        public string Status { get; set; }
    }
}
