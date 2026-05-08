using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Main
{
    // ── Config ─────────────────────────────────────────────────────────────
    public class ContingencyConfigResponse
    {
        public bool IsEnabled { get; set; }
        public bool HasPassword { get; set; } // never expose actual password
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class UpsertContingencyConfigRequest
    {
        public bool IsEnabled { get; set; }
        public string? MasterPassword { get; set; } // null = don't change
    }

    // ── Login ──────────────────────────────────────────────────────────────
    public class ContingencyLoginRequest
    {
        public string UserID { get; set; }
        public string Password { get; set; }   // master password
        public string Branch { get; set; }
        public string SectionCode { get; set; }
    }

    // ── Endorse ────────────────────────────────────────────────────────────
    public class ContingencyEndorseRequest
    {
        public string EndorsedTo { get; set; }
        public List<ContingencySpecimenItem> Specimens { get; set; }
    }

    public class ContingencySpecimenItem
    {
        public string SpecimenNo { get; set; }
        public string SampleTypeCode { get; set; }
        public string SampleTypeName { get; set; }
    }

    // ── Batch list / detail ────────────────────────────────────────────────
    public class ContingencyBatchListItem
    {
        public int Id { get; set; }
        public string BatchNo { get; set; }
        public string EndorsingBranch { get; set; }
        public string EndorsingSection { get; set; }
        public string EndorsedTo { get; set; }
        public string EndorsedBy { get; set; }
        public DateTime EndorsedAt { get; set; }
        public string Status { get; set; }
        public bool IsImported { get; set; }
        public int TotalSpecimens { get; set; }
        public int ReceivedCount { get; set; }
    }

    public class ContingencyBatchDetail
    {
        public int Id { get; set; }
        public string BatchNo { get; set; }
        public string EndorsingBranch { get; set; }
        public string EndorsingSection { get; set; }
        public string EndorsedTo { get; set; }
        public string EndorsedBy { get; set; }
        public DateTime EndorsedAt { get; set; }
        public string Status { get; set; }
        public List<ContingencySpecimenDetail> Specimens { get; set; }
    }

    public class ContingencySpecimenDetail
    {
        public int Id { get; set; }
        public string SpecimenNo { get; set; }
        public string SampleTypeCode { get; set; }
        public string SampleTypeName { get; set; }
        public bool IsReceived { get; set; }
        public string? ReceivedBy { get; set; }
        public DateTime? ReceivedAt { get; set; }
    }

    // ── Receive ────────────────────────────────────────────────────────────
    public class ContingencyScanRequest
    {
        public int BatchId { get; set; }
        public string SpecimenNo { get; set; }
    }

    public class ContingencyScanResponse
    {
        public bool Found { get; set; }
        public bool AlreadyReceived { get; set; }
        public int SpecimenId { get; set; }
        public bool BatchCompleted { get; set; }
        public string Message { get; set; }
    }

    // ── Import ─────────────────────────────────────────────────────────────
    public class ContingencyImportResult
    {
        public bool Success { get; set; }
        public string BatchNo { get; set; }
        public int SpecimenCount { get; set; }
        public bool AlreadyExists { get; set; }
        public string Message { get; set; }
    }
}