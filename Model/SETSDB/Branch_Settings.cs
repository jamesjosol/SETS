using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.SETSDB
{
    public class Branch_Settings
    {
        public int Id { get; set; }
        public bool IsOutboundEnabled { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public bool OutboundTatEnabled { get; set; }        
        public bool OutboundTatAppealEnabled { get; set; }  
    }
}