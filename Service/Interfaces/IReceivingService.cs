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
        Task<ReceiveSpecimenResponse> ReceiveSpecimen(ReceiveSpecimenRequest request);
        void UpdateSpecimenReceivingRemarks(string specimenNo, string batchNo, string? remarks);
        void UpdateBatchTemp(UpdateBatchTempRequest request);
        void UpdateNonBarcodedReceivingRemarks(int itemID, string? remarks);
        ReceiveResponse ReceiveNonBarcoded(ReceiveNonBarcodedRequest request);
        List<PendingNonBarcodedItem> GetPendingNonBarcoded(string sectionCode);
        List<MonitoringDashboardSection> GetMonitoringDashboard(string procSectionCode, DateTime date);
        BatchSummary GetReceiverDashboardSummary(string procSectionCode, DateTime date);
        List<BatchDailyFlow> GetWeeklyReceivedFlow(string procSectionCode);
        List<HourlyFlow> GetHourlyReceivedFlow(string procSectionCode);
        List<IncomingBatchItem> GetIncomingBatches(string procSectionCode);
        List<IncomingSpecimenItem> GetIncomingSpecimens(string procSectionCode);
        List<ReceivedBatchItem> GetReceivedBatches(string? sectionCode, DateTime dateFrom, DateTime dateTo);
        CancelSpecimenResult CancelSpecimen(CancelSpecimenRequest request);
        void CancelNonBarcodedItem(CancelNonBarcodedRequest request);
        CheckSpecimenResponse CheckSpecimen(string specimenNo, string? currentBatchNo);
        void SetSpecimenAlert(SetSpecimenAlertRequest request);
        void ClearSpecimenAlert(ClearSpecimenAlertRequest request);
        List<Batch_Specimen> GetReceivedWithoutSection(int lookbackHours);
        Task<bool> RetryRouteSpecimen(string specimenNo, Dictionary<string, string> mappingCache, HashSet<string> unmappedGroups, HashSet<string> emptyOracleKeys);

    }
}