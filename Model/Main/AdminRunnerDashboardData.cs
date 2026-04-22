using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Main
{
    public class SectionRunningGroup
    {
        public string SectionCode { get; set; }
        public string SectionName { get; set; }
        public List<RunningSpecimenItem> Specimens { get; set; } = new();
    }

    public class SectionPendingGroup
    {
        public string SectionCode { get; set; }
        public string SectionName { get; set; }
        public List<PendingSpecimenItem> Specimens { get; set; } = new();
    }

    public class SectionScheduledGroup
    {
        public string SectionCode { get; set; }
        public string SectionName { get; set; }
        public List<ScheduledSpecimenItem> Specimens { get; set; } = new();
    }
}