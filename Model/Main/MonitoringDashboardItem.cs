using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Main
{
    public class MonitoringDashboardSection
    {
        public string SectionCode { get; set; }
        public string SectionName { get; set; }
        public List<MonitoringBatchItem> Batches { get; set; } = new();
    }

    public class MonitoringBatchItem
    {
        public string BatchNo { get; set; }
        public string Status { get; set; }      // P, PA, C
        public DateTime Endorsed { get; set; }
        public string EndorsedBy { get; set; }
        public int TotalSpecimens { get; set; }
        public int ReceivedSpecimens { get; set; }
    }
}