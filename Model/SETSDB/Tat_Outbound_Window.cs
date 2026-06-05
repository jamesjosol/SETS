using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.SETSDB
{
    public class Tat_Outbound_Window
    {
        public int Id { get; set; }
        public TimeSpan WindowStart { get; set; }
        public TimeSpan WindowEnd { get; set; }
        public string ScheduleType { get; set; }   // "Weekday" | "Sunday"
        public bool IsActive { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime Created { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? Updated { get; set; }
    }
}