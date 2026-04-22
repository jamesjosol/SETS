using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Main
{
    public class RunnerDashboardSummary
    {
        public int Pending { get; set; }
        public int Scheduled { get; set; }
        public int Running { get; set; }
        public int CompletedToday { get; set; }
    }
}