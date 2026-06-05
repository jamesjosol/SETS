using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Main
{
    public class OutboundTatSettingsRequest
    {
        public bool OutboundTatEnabled { get; set; }
        public bool OutboundTatAppealEnabled { get; set; }
    }

    public class OutboundWindowUpsertRequest
    {
        public int Id { get; set; }           // 0 = new
        public string WindowStart { get; set; }   // "HH:mm"
        public string WindowEnd { get; set; }     // "HH:mm"
        public string ScheduleType { get; set; }  // "Weekday" | "Sunday"
        public bool IsActive { get; set; }
    }
}
