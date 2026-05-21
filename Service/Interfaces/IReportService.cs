using Model.Main;

namespace Service.Interfaces
{
    public interface IReportService
    {
        // R1 — Unprocessed Specimen
        List<UnprocessedSpecimenRow> GetUnprocessedSpecimens(UnprocessedSpecimenRequest request);

        // R2 — Specimen Not Endorsed
        List<SpecimenNotEndorsedRow> GetSpecimensNotEndorsed(SpecimenNotEndorsedRequest request);

        // R3 — Specimen Not Received / Pending
        List<SpecimenNotReceivedRow> GetSpecimensNotReceived(SpecimenNotReceivedRequest request);
        byte[] ExportSpecimensNotReceivedExcel(SpecimenNotReceivedRequest request);

        // R4 — Test Management
        List<TestManagementRow> GetTestManagement(TestManagementRequest request);
        byte[] ExportTestManagementExcel(TestManagementRequest request);

        // R5 — Batch Summary (data + Excel export)
        BatchSummaryReportResult GetBatchSummary(BatchSummaryReportRequest request);
        byte[] ExportBatchSummaryExcel(BatchSummaryReportRequest request);

        // R6 — Specimen Receipt (Laboratory Section)
        List<SpecimenReceiptSectionRow> GetSpecimenReceiptSection(SpecimenReceiptSectionRequest request);
        byte[] ExportSpecimenReceiptSectionExcel(SpecimenReceiptSectionRequest request);

        // R7 — Duplicate Endorsement
        List<DuplicateEndorsementRow> GetDuplicateEndorsements(DuplicateEndorsementRequest request);
        byte[] ExportDuplicateEndorsementsExcel(DuplicateEndorsementRequest request);

        // R8 — Transaction Beyond 14 Days
        List<Beyond14DaysRow> GetBeyond14Days(Beyond14DaysRequest request);
        byte[] ExportBeyond14DaysExcel(Beyond14DaysRequest request);

        // R9 — Monthly Endorsement Summary
        List<MonthlyEndorsementSummaryRow> GetMonthlyEndorsementSummary(MonthlyEndorsementSummaryRequest request);
    }
}