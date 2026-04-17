using Microsoft.AspNetCore.Mvc;
using Model.Main;
using Service;

namespace SETS.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReceivingController : ControllerBase
    {
        // POST api/receiving/specimen
        [HttpPost("specimen")]
        public IActionResult ReceiveSpecimen([FromBody] ReceiveSpecimenRequest request)
        {
            try
            {
                var branch = HttpContext.Session.GetString("BranchCode");
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                if (string.IsNullOrEmpty(request.SpecimenNo))
                    return BadRequest(new { message = "Specimen number is required." });

                if (string.IsNullOrEmpty(request.UserID))
                    return BadRequest(new { message = "User ID is required." });

                using var master = new MasterService(branch);
                var result = master.Receiving.ReceiveSpecimen(request);

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                // Surface known business rule violations as 400
                // so the frontend can display them as prompts
                return StatusCode(400, new { message = ex.Message });
            }
        }

        // PATCH api/receiving/specimen-remarks
        [HttpPatch("specimen-remarks")]
        public IActionResult UpdateSpecimenRemarks([FromBody] UpdateSpecimenRemarksRequest request)
        {
            try
            {
                var branch = HttpContext.Session.GetString("BranchCode");
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);
                master.Receiving.UpdateSpecimenReceivingRemarks(
                    request.SpecimenNo,
                    request.BatchNo,
                    request.ReceivingRemarks
                );

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(400, new { message = ex.Message });
            }
        }

        // PATCH api/receiving/batch-temp
        [HttpPatch("batch-temp")]
        public IActionResult UpdateBatchTemp([FromBody] UpdateBatchTempRequest request)
        {
            try
            {
                var branch = HttpContext.Session.GetString("BranchCode");
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                if (string.IsNullOrEmpty(request.BatchNo))
                    return BadRequest(new { message = "Batch number is required." });

                using var master = new MasterService(branch);
                master.Receiving.UpdateBatchTemp(request);

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(400, new { message = ex.Message });
            }
        }

        // POST api/receiving/nonbarcoded
        [HttpPost("nonbarcoded")]
        public IActionResult ReceiveNonBarcoded([FromBody] ReceiveNonBarcodedRequest request)
        {
            try
            {
                var branch = HttpContext.Session.GetString("BranchCode");
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                if (string.IsNullOrEmpty(request.UserID))
                    return BadRequest(new { message = "User ID is required." });

                if (request.ItemIDs == null || request.ItemIDs.Count == 0)
                    return BadRequest(new { message = "No items selected." });

                using var master = new MasterService(branch);
                var result = master.Receiving.ReceiveNonBarcoded(request);

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(400, new { message = ex.Message });
            }
        }


        // PATCH api/receiving/nonbarcoded-remarks
        [HttpPatch("nonbarcoded-remarks")]
        public IActionResult UpdateNonBarcodedRemarks([FromBody] UpdateNonBarcodedRemarksRequest request)
        {
            try
            {
                var branch = HttpContext.Session.GetString("BranchCode");
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);
                master.Receiving.UpdateNonBarcodedReceivingRemarks(
                    request.ItemID,
                    request.ReceivingRemarks
                );

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(400, new { message = ex.Message });
            }
        }

        // GET api/receiving/pending-nonbarcoded?sectionCode=WP
        // Loads all pending non-barcoded items for the processing section
        [HttpGet("pending-nonbarcoded")]
        public IActionResult GetPendingNonBarcoded([FromQuery] string sectionCode)
        {
            try
            {
                var branch = HttpContext.Session.GetString("BranchCode");
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);

                var resolvedCode = ResolveProcessingSection(master, branch, sectionCode);
                if (resolvedCode == null)
                    return StatusCode(500, new { message = "No active Processing section found for this branch." });

                var result = master.Receiving.GetPendingNonBarcoded(resolvedCode);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }


        // GET api/receiving/dashboard-summary?sectionCode=WP
        [HttpGet("dashboard-summary")]
        public IActionResult GetDashboardSummary([FromQuery] string sectionCode)
        {
            try
            {
                var branch = HttpContext.Session.GetString("BranchCode");
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);

                // Resolve processing section code
                var resolvedCode = ResolveProcessingSection(master, branch, sectionCode);
                if (resolvedCode == null)
                    return StatusCode(500, new { message = "No active Processing section found for this branch." });

                var summary = master.Receiving.GetReceiverDashboardSummary(resolvedCode, DateTime.Today);
                return Ok(summary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // GET api/receiving/monitoring?sectionCode=WP
        [HttpGet("monitoring")]
        public IActionResult GetMonitoringDashboard([FromQuery] string sectionCode)
        {
            try
            {
                var branch = HttpContext.Session.GetString("BranchCode");
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);

                // Resolve processing section code
                var resolvedCode = ResolveProcessingSection(master, branch, sectionCode);
                if (resolvedCode == null)
                    return StatusCode(500, new { message = "No active Processing section found for this branch." });

                var data = master.Receiving.GetMonitoringDashboard(resolvedCode, DateTime.Today);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ── Helper ─────────────────────────────────────────────────────────────────

        private static string? ResolveProcessingSection(MasterService master, string branch, string sectionCode)
        {
            // If the passed sectionCode belongs to a processing section (category 2), use it directly
            var section = master.Section.GetByCode(sectionCode);
            if (section != null && section.Category == "2")
                return sectionCode;

            // Otherwise (admin logged in under a non-processing section),
            // auto-resolve the active processing section for the branch
            return master.Section
                .GetByBranch(branch)
                .FirstOrDefault(s => s.Category == "2" && s.Active)
                ?.Code;
        }

        // GET api/receiving/weekly-flow?sectionCode=WP
        [HttpGet("weekly-flow")]
        public IActionResult GetWeeklyFlow([FromQuery] string sectionCode)
        {
            try
            {
                var branch = HttpContext.Session.GetString("BranchCode");
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);

                var resolvedCode = ResolveProcessingSection(master, branch, sectionCode);
                if (resolvedCode == null)
                    return StatusCode(500, new { message = "No active Processing section found for this branch." });

                var flow = master.Receiving.GetWeeklyReceivedFlow(resolvedCode);
                return Ok(flow);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // GET api/receiving/hourly-flow?sectionCode=WP
        [HttpGet("hourly-flow")]
        public IActionResult GetHourlyFlow([FromQuery] string sectionCode)
        {
            try
            {
                var branch = HttpContext.Session.GetString("BranchCode");
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);

                var resolvedCode = ResolveProcessingSection(master, branch, sectionCode);
                if (resolvedCode == null)
                    return StatusCode(500, new { message = "No active Processing section found for this branch." });

                var flow = master.Receiving.GetHourlyReceivedFlow(resolvedCode);
                return Ok(flow);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // GET api/receiving/incoming-batches?sectionCode=WP
        [HttpGet("incoming-batches")]
        public IActionResult GetIncomingBatches([FromQuery] string sectionCode)
        {
            try
            {
                var branch = HttpContext.Session.GetString("BranchCode");
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);

                var resolvedCode = ResolveProcessingSection(master, branch, sectionCode);
                if (resolvedCode == null)
                    return StatusCode(500, new { message = "No active Processing section found for this branch." });

                var data = master.Receiving.GetIncomingBatches(resolvedCode);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // GET api/receiving/incoming-specimens?sectionCode=WP
        [HttpGet("incoming-specimens")]
        public IActionResult GetIncomingSpecimens([FromQuery] string sectionCode)
        {
            try
            {
                var branch = HttpContext.Session.GetString("BranchCode");
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);

                var resolvedCode = ResolveProcessingSection(master, branch, sectionCode);
                if (resolvedCode == null)
                    return StatusCode(500, new { message = "No active Processing section found for this branch." });

                var data = master.Receiving.GetIncomingSpecimens(resolvedCode);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}