using Microsoft.AspNetCore.Mvc;
using Service;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuditController : ControllerBase
    {
        // GET api/audit/by-user?userID=JMJOSOL&dateFrom=2026-01-01&dateTo=2026-04-30
        [HttpGet("by-user")]
        public IActionResult GetByUser([FromQuery] string userID,
                                       [FromQuery] string dateFrom,
                                       [FromQuery] string dateTo)
        {
            try
            {
                var branch = HttpContext.Session.GetString("BranchCode");
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });
                if (string.IsNullOrEmpty(userID))
                    return BadRequest(new { message = "User ID is required." });

                var from = DateTime.TryParse(dateFrom, out var df) ? df : DateTime.Today;
                var to = DateTime.TryParse(dateTo, out var dt) ? dt : DateTime.Today;

                using var master = new MasterService(branch);
                var logs = master.Audit.GetByUser(userID, from, to);
                return Ok(logs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // GET api/audit/by-specimen?specimenNo=2654070457
        [HttpGet("by-specimen")]
        public IActionResult GetBySpecimen([FromQuery] string specimenNo)
        {
            try
            {
                var branch = HttpContext.Session.GetString("BranchCode");
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });
                if (string.IsNullOrEmpty(specimenNo))
                    return BadRequest(new { message = "Specimen number is required." });

                using var master = new MasterService(branch);
                var logs = master.Audit.GetBySpecimenNo(specimenNo);
                return Ok(logs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // GET api/audit/by-batch?batchNo=WA26-00001
        [HttpGet("by-batch")]
        public IActionResult GetByBatch([FromQuery] string batchNo)
        {
            try
            {
                var branch = HttpContext.Session.GetString("BranchCode");
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });
                if (string.IsNullOrEmpty(batchNo))
                    return BadRequest(new { message = "Batch number is required." });

                using var master = new MasterService(branch);
                var logs = master.Audit.GetByBatchNo(batchNo);
                return Ok(logs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}