using Microsoft.AspNetCore.Mvc;
using Model.Main;
using Service;

namespace SETS.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : ControllerBase
    {
        private string? Branch => HttpContext.Session.GetString("BranchCode");
        private string? UserID => HttpContext.Session.GetString("UserID");
        private bool IsAdmin => HttpContext.Session.GetString("IsAdmin") == "True";
        private string? Category => HttpContext.Session.GetString("SectionCategory");
        private int? RoleID => HttpContext.Session.GetInt32("RoleID");

        private bool IsTLorAdmin => IsAdmin || RoleID == 2;
        private bool IsProcessingOrAbove => IsAdmin || RoleID == 2 || Category == "2";

        private IActionResult? SessionGuard()
        {
            if (string.IsNullOrEmpty(UserID)) return Unauthorized(new { message = "Session expired." });
            return null;
        }

        // ══════════════════════════════════════════════════════════════════════════
        // R4 — Test Management
        // POST api/report/test-management
        // ══════════════════════════════════════════════════════════════════════════

        [HttpPost("test-management")]
        public IActionResult GetTestManagement([FromBody] TestManagementRequest request)
        {
            try
            {
                var guard = SessionGuard(); if (guard != null) return guard;
                if (!IsTLorAdmin) return Forbid();

                // Non-admin TL is locked to their own section
                if (!IsAdmin)
                    request.SectionCode = HttpContext.Session.GetString("SectionCode");

                var branch = Branch!;
                using var master = new MasterService(branch);
                var result = master.Report.GetTestManagement(request);
                return Ok(result);
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // POST api/report/test-management/export
        [HttpPost("test-management/export")]
        public IActionResult ExportTestManagement([FromBody] TestManagementRequest request)
        {
            try
            {
                var guard = SessionGuard(); if (guard != null) return guard;
                if (!IsTLorAdmin) return Forbid();

                if (!IsAdmin)
                    request.SectionCode = HttpContext.Session.GetString("SectionCode");

                var branch = Branch!;
                using var master = new MasterService(branch);
                var bytes = master.Report.ExportTestManagementExcel(request);

                var fileName = $"TestManagementReport_{DateTime.Now:yyyyMMdd}.xlsx";
                return File(bytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName);
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // ══════════════════════════════════════════════════════════════════════
        // R5 — Batch Summary data preview
        // POST api/report/batch-summary
        // ══════════════════════════════════════════════════════════════════════

        [HttpPost("batch-summary")]
        public IActionResult GetBatchSummary([FromBody] BatchSummaryReportRequest request)
        {
            try
            {
                var guard = SessionGuard(); if (guard != null) return guard;
                if (!IsProcessingOrAbove) return Forbid();

                var branch = Branch!;
                using var master = new MasterService(branch);
                var result = master.Report.GetBatchSummary(request);
                return Ok(result);
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // ══════════════════════════════════════════════════════════════════════
        // R5 — Batch Summary Excel export (server-side ClosedXML)
        // POST api/report/batch-summary/export
        // ══════════════════════════════════════════════════════════════════════

        [HttpPost("batch-summary/export")]
        public IActionResult ExportBatchSummary([FromBody] BatchSummaryReportRequest request)
        {
            try
            {
                var guard = SessionGuard(); if (guard != null) return guard;
                if (!IsProcessingOrAbove) return Forbid();

                var branch = Branch!;
                using var master = new MasterService(branch);
                var bytes = master.Report.ExportBatchSummaryExcel(request);

                var fileName = $"BatchSummaryReport_{DateTime.Now:yyyyMMdd}.xlsx";
                return File(bytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName);
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // ══════════════════════════════════════════════════════════════════════
        // R1 — Unprocessed Specimen
        // POST api/report/unprocessed-specimen
        // ══════════════════════════════════════════════════════════════════════

        [HttpPost("unprocessed-specimen")]
        public IActionResult GetUnprocessedSpecimens([FromBody] UnprocessedSpecimenRequest request)
        {
            try
            {
                var guard = SessionGuard(); if (guard != null) return guard;
                var branch = Branch!;
                using var master = new MasterService(branch);
                var result = master.Report.GetUnprocessedSpecimens(request);
                return Ok(result);
            }
            catch (NotImplementedException ex) { return StatusCode(501, new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // ══════════════════════════════════════════════════════════════════════
        // R2 — Specimen Not Endorsed
        // POST api/report/specimen-not-endorsed
        // ══════════════════════════════════════════════════════════════════════

        [HttpPost("specimen-not-endorsed")]
        public IActionResult GetSpecimensNotEndorsed([FromBody] SpecimenNotEndorsedRequest request)
        {
            try
            {
                var guard = SessionGuard(); if (guard != null) return guard;
                var branch = Branch!;
                using var master = new MasterService(branch);
                var result = master.Report.GetSpecimensNotEndorsed(request);
                return Ok(result);
            }
            catch (NotImplementedException ex) { return StatusCode(501, new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // ══════════════════════════════════════════════════════════════════════
        // R3 — Specimen Not Received / Pending
        // Access: Processing + Admin (Endorser locked out)
        // POST api/report/specimen-not-received
        // ══════════════════════════════════════════════════════════════════════

        [HttpPost("specimen-not-received")]
        public IActionResult GetSpecimensNotReceived([FromBody] SpecimenNotReceivedRequest request)
        {
            try
            {
                var guard = SessionGuard(); if (guard != null) return guard;

                if (Category == "1" && !IsAdmin) return Forbid();
                if (Category == "3" && !IsAdmin) return Forbid();

                var branch = Branch!;
                using var master = new MasterService(branch);
                var result = master.Report.GetSpecimensNotReceived(request);
                return Ok(result);
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // POST api/report/specimen-not-received/export
        [HttpPost("specimen-not-received/export")]
        public IActionResult ExportSpecimensNotReceived([FromBody] SpecimenNotReceivedRequest request)
        {
            try
            {
                var guard = SessionGuard(); if (guard != null) return guard;

                if (Category == "1" && !IsAdmin) return Forbid();
                if (Category == "3" && !IsAdmin) return Forbid();

                var branch = Branch!;
                using var master = new MasterService(branch);
                var bytes = master.Report.ExportSpecimensNotReceivedExcel(request);

                var fileName = $"SpecimenNotReceivedReport_{DateTime.Now:yyyyMMdd}.xlsx";
                return File(bytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName);
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // ══════════════════════════════════════════════════════════════════════
        // R6 — Specimen Receipt (Laboratory Section) data preview
        // Access: Admin, Processing (all), Lab Section (own section only — enforced in service via SectionCode)
        // POST api/report/specimen-receipt-section
        // ══════════════════════════════════════════════════════════════════════

        [HttpPost("specimen-receipt-section")]
        public IActionResult GetSpecimenReceiptSection([FromBody] SpecimenReceiptSectionRequest request)
        {
            try
            {
                var guard = SessionGuard(); if (guard != null) return guard;

                // Endorser has no access
                if (Category == "1" && !IsAdmin) return Forbid();

                // Lab section users are locked to their own section
                if (Category == "3" && !IsAdmin)
                    request.SectionCode = HttpContext.Session.GetString("SectionCode");

                var branch = Branch!;
                using var master = new MasterService(branch);
                var result = master.Report.GetSpecimenReceiptSection(request);
                return Ok(result);
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // ══════════════════════════════════════════════════════════════════════
        // R6 — Specimen Receipt (Laboratory Section) Excel export
        // POST api/report/specimen-receipt-section/export
        // ══════════════════════════════════════════════════════════════════════

        [HttpPost("specimen-receipt-section/export")]
        public IActionResult ExportSpecimenReceiptSection([FromBody] SpecimenReceiptSectionRequest request)
        {
            try
            {
                var guard = SessionGuard(); if (guard != null) return guard;

                if (Category == "1" && !IsAdmin) return Forbid();

                if (Category == "3" && !IsAdmin)
                    request.SectionCode = HttpContext.Session.GetString("SectionCode");

                var branch = Branch!;
                using var master = new MasterService(branch);
                var bytes = master.Report.ExportSpecimenReceiptSectionExcel(request);

                var fileName = $"SpecimenReceiptLabSection_{DateTime.Now:yyyyMMdd}.xlsx";
                return File(bytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName);
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // ══════════════════════════════════════════════════════════════════════
        // R7 — Duplicate Endorsement
        // Access: All roles — Endorser scoped to own section via LocationCode
        // POST api/report/duplicate-endorsement
        // ══════════════════════════════════════════════════════════════════════

        [HttpPost("duplicate-endorsement")]
        public IActionResult GetDuplicateEndorsements([FromBody] DuplicateEndorsementRequest request)
        {
            try
            {
                var guard = SessionGuard(); if (guard != null) return guard;

                // Endorser users are locked to their own section
                if (Category == "1" && !IsAdmin)
                    request.LocationCode = HttpContext.Session.GetString("SectionCode");

                var branch = Branch!;
                using var master = new MasterService(branch);
                var result = master.Report.GetDuplicateEndorsements(request);
                return Ok(result);
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // POST api/report/duplicate-endorsement/export
        [HttpPost("duplicate-endorsement/export")]
        public IActionResult ExportDuplicateEndorsements([FromBody] DuplicateEndorsementRequest request)
        {
            try
            {
                var guard = SessionGuard(); if (guard != null) return guard;

                if (Category == "1" && !IsAdmin)
                    request.LocationCode = HttpContext.Session.GetString("SectionCode");

                var branch = Branch!;
                using var master = new MasterService(branch);
                var bytes = master.Report.ExportDuplicateEndorsementsExcel(request);

                var fileName = $"DuplicateEndorsementReport_{DateTime.Now:yyyyMMdd}.xlsx";
                return File(bytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName);
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // ══════════════════════════════════════════════════════════════════════
        // R8 — Transaction Beyond 14 Days
        // Access: Endorser (own section) + Processing + Admin
        // POST api/report/beyond-14-days
        // ══════════════════════════════════════════════════════════════════════

        [HttpPost("beyond-14-days")]
        public IActionResult GetBeyond14Days([FromBody] Beyond14DaysRequest request)
        {
            try
            {
                var guard = SessionGuard(); if (guard != null) return guard;

                // Lab section has no access
                if (Category == "3" && !IsAdmin) return Forbid();

                // Endorser locked to own section
                if (Category == "1" && !IsAdmin)
                    request.LocationCode = HttpContext.Session.GetString("SectionCode");

                var branch = Branch!;
                using var master = new MasterService(branch);
                var result = master.Report.GetBeyond14Days(request);
                return Ok(result);
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // POST api/report/beyond-14-days/export
        [HttpPost("beyond-14-days/export")]
        public IActionResult ExportBeyond14Days([FromBody] Beyond14DaysRequest request)
        {
            try
            {
                var guard = SessionGuard(); if (guard != null) return guard;

                if (Category == "3" && !IsAdmin) return Forbid();

                if (Category == "1" && !IsAdmin)
                    request.LocationCode = HttpContext.Session.GetString("SectionCode");

                var branch = Branch!;
                using var master = new MasterService(branch);
                var bytes = master.Report.ExportBeyond14DaysExcel(request);

                var fileName = $"Beyond14DaysReport_{DateTime.Now:yyyyMMdd}.xlsx";
                return File(bytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName);
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // ══════════════════════════════════════════════════════════════════════
        // R9 — Monthly Endorsement Summary
        // POST api/report/monthly-summary
        // ══════════════════════════════════════════════════════════════════════

        [HttpPost("monthly-summary")]
        public IActionResult GetMonthlyEndorsementSummary([FromBody] MonthlyEndorsementSummaryRequest request)
        {
            try
            {
                var guard = SessionGuard(); if (guard != null) return guard;
                if (!IsTLorAdmin) return Forbid();
                var branch = Branch!;
                using var master = new MasterService(branch);
                var result = master.Report.GetMonthlyEndorsementSummary(request);
                return Ok(result);
            }
            catch (NotImplementedException ex) { return StatusCode(501, new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }
    }
}