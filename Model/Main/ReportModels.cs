using System;
using System.Collections.Generic;

namespace Model.Main
{
    // ── Shared filter base ────────────────────────────────────────────────────

    public class ReportDateRangeRequest
    {
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // R1 — Unprocessed Specimen Report
    // ══════════════════════════════════════════════════════════════════════════

    public class UnprocessedSpecimenRequest : ReportDateRangeRequest { }

    public class UnprocessedSpecimenRow
    {
        public string LabNo { get; set; } = string.Empty;
        public string? PatientName { get; set; }
        public string? TestName { get; set; }
        public string? ScheduleTag { get; set; }       // ERD / CRD / SRD
        public string? RunningDate { get; set; }        // formatted date string when tag is CRD/SRD
        public string SectionCode { get; set; } = string.Empty;
        public string SectionName { get; set; } = string.Empty;
    }

    // ══════════════════════════════════════════════════════════════════════════
    // R2 — Specimen Not Endorsed
    // ══════════════════════════════════════════════════════════════════════════

    public class SpecimenNotEndorsedRequest : ReportDateRangeRequest
    {
        public string? LocationCode { get; set; }       // null = ALL
    }

    public class SpecimenNotEndorsedRow
    {
        public string? BatchNo { get; set; }
        public string? Location { get; set; }
        public string? SpecimenNo { get; set; }
        public string? PatientName { get; set; }
        public DateTime? LoggedAt { get; set; }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // R3 — Specimen Not Received / Pending
    // ══════════════════════════════════════════════════════════════════════════

    public class SpecimenNotReceivedRequest : ReportDateRangeRequest
    {
        public string? LocationCode { get; set; }       // endorser section (Category 1), null = ALL
    }

    public class SpecimenNotReceivedRow
    {
        public string BatchNo { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;   // endorser section name
        public string? SpecimenNo { get; set; }
        public string? PatientName { get; set; }
        public string? SampleTypeName { get; set; }
        public DateTime Endorsed { get; set; }
        public string? EndorsedBy { get; set; }
        public string? Remarks { get; set; }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // R4 — Test Management Report
    // ══════════════════════════════════════════════════════════════════════════

    public class TestManagementRequest
    {
        public string? SectionCode { get; set; }   // null = ALL (admin only)
    }

    public class TestManagementRow
    {
        public string TestCode { get; set; } = string.Empty;
        public string TestName { get; set; } = string.Empty;
        public string? RunningDays { get; set; }        // formatted: "Mon, Wed, Fri"
        public string SectionCode { get; set; } = string.Empty;
        public string SectionName { get; set; } = string.Empty;
    }

    // ══════════════════════════════════════════════════════════════════════════
    // R5 — Batch Summary Report
    // ══════════════════════════════════════════════════════════════════════════

    public class BatchSummaryReportRequest : ReportDateRangeRequest
    {
        public string? LocationCode { get; set; }       // null = ALL (endorser section code)
    }

    public class BatchSummaryReportRow
    {
        public string BatchNo { get; set; } = string.Empty;
        public string LocationCode { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime Endorsed { get; set; }
        public string EndorsedBy { get; set; } = string.Empty;
        public DateTime? ProcReceived { get; set; }
        public string? ReceivedBy { get; set; }
        public DateTime? Completed { get; set; }
        public string? Temp { get; set; }
        public string Status { get; set; } = string.Empty;
        // TAT (Batch Endorsement) = ProcReceived – Endorsed   (how long specimen arrives from branch)
        public string? TatEndorsement { get; set; }     // hh:mm, null if ProcReceived is null
        // TAT (Batch Completion)   = Completed – ProcReceived (how long batch is fully received)
        public string? TatCompletion { get; set; }      // hh:mm, null if either timestamp is null
    }

    public class BatchSummaryReportResult
    {
        public List<BatchSummaryReportRow> Rows { get; set; } = new();
        public int TotalBatches { get; set; }
        public int CompletedBatches { get; set; }
        public int PendingBatches { get; set; }
        public string? AvgTatEndorsement { get; set; }  // hh:mm average across rows that have it
        public string? AvgTatCompletion { get; set; }   // hh:mm average across rows that have it
    }

    // ══════════════════════════════════════════════════════════════════════════
    // R6 — Specimen Receipt (Section) Report
    // ══════════════════════════════════════════════════════════════════════════

    public class SpecimenReceiptSectionRequest : ReportDateRangeRequest
    {
        public string? SectionCode { get; set; }        // lab section (Category 3), null = ALL
        public string? LocationCode { get; set; }       // endorser section (Category 1), null = ALL
    }

    public class SpecimenReceiptSectionRow
    {
        public string? BatchNo { get; set; }
        public string? Location { get; set; }           // endorser section name
        public string? SpecimenNo { get; set; }
        public string? PatientName { get; set; }
        public string? SampleTypeName { get; set; }
        public DateTime? ProcReceived { get; set; }     // when processing received it
        public string? RoutedBy { get; set; }           // who routed it (processing staff)
        public DateTime? SectionReceived { get; set; }  // when lab section received it
        public string? SectionReceivedBy { get; set; }  // who received it in lab section
        public string? LabSection { get; set; }         // lab section name
        public string? TatSection { get; set; }         // hh:mm — SectionReceived – ProcReceived
    }

    // ══════════════════════════════════════════════════════════════════════════
    // R7 — Duplicate Endorsement Report
    // ══════════════════════════════════════════════════════════════════════════

    public class DuplicateEndorsementRequest : ReportDateRangeRequest
    {
        public string? LocationCode { get; set; }       // endorser section (Category 1), null = ALL
    }

    public class DuplicateEndorsementRow
    {
        public string? BatchNo { get; set; }
        public string? Location { get; set; }           // endorser section name
        public string? SpecimenNo { get; set; }
        public string? PatientName { get; set; }
        public DateTime? FirstEndorsedAt { get; set; }
        public string? FirstEndorsedBy { get; set; }
        public DateTime? SecondEndorsedAt { get; set; }
        public string? SecondEndorsedBy { get; set; }
        public string? Reason { get; set; }             // Remarks from the duplicate log entry
    }

    // ══════════════════════════════════════════════════════════════════════════
    // R8 — Transaction Beyond 14 Days Report
    // ══════════════════════════════════════════════════════════════════════════

    public class Beyond14DaysRequest : ReportDateRangeRequest
    {
        public string? LocationCode { get; set; }       // endorser section (Category 1), null = ALL
    }

    public class Beyond14DaysRow
    {
        public string? BatchNo { get; set; }
        public string? Location { get; set; }           // endorser section name
        public string? SpecimenNo { get; set; }
        public string? PatientName { get; set; }
        public DateTime? EndorsedAt { get; set; }
        public string? EndorsedBy { get; set; }
        public string? Reason { get; set; }             // Remarks from the log entry
    }

    // ══════════════════════════════════════════════════════════════════════════
    // R9 — Monthly Endorsement Summary
    // ══════════════════════════════════════════════════════════════════════════

    public class MonthlyEndorsementSummaryRequest : ReportDateRangeRequest
    {
        public string? LocationCode { get; set; }       // null = ALL
        public string? SectionCode { get; set; }        // null = ALL
    }

    public class MonthlyEndorsementSummaryRow
    {
        public string Location { get; set; } = string.Empty;
        public int TotalBatches { get; set; }
        public int BatchesOutsideTat { get; set; }
        public int BatchesWithinTat { get; set; }
        public int DuplicateEndorsements { get; set; }
        public int NotEndorsed { get; set; }
        public int Beyond14Days { get; set; }
    }
}