using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Main;
using Model.SETSDB;

namespace Service.Interfaces
{
    public interface IReceivingService
    {
        ReceiveSpecimenResponse ReceiveSpecimen(ReceiveSpecimenRequest request);
        ReceiveResponse ReceiveNonBarcoded(ReceiveNonBarcodedRequest request);
        List<PendingNonBarcodedItem> GetPendingNonBarcoded(string sectionCode);
        List<MonitoringDashboardSection> GetMonitoringDashboard(string procSectionCode, DateTime date);
        BatchSummary GetReceiverDashboardSummary(string procSectionCode, DateTime date);
        List<BatchDailyFlow> GetWeeklyReceivedFlow(string procSectionCode);
    }
}