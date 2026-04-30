using Microsoft.AspNetCore.Mvc;
using Model.Main;
using Service;

namespace SETS.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProcessingOptionsController : ControllerBase
    {
        private string? Branch => HttpContext.Session.GetString("BranchCode");
        private string? CurrentUserID => HttpContext.Session.GetString("UserID");
        private bool IsAdmin => HttpContext.Session.GetString("IsAdmin") == "True";

        private IActionResult RequireAdmin()
            => Unauthorized(new { message = "Administrator access required." });

        // ── GET api/processingoptions ─────────────────────────────────────────
        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);
                var options = master.ProcessingOptions.GetOptions();

                return Ok(new
                {
                    showTemperature = options.ShowTemperature,
                    showTempRemarks = options.ShowTempRemarks,
                    showBagNo = options.ShowBagNo,
                    updatedBy = options.UpdatedBy,
                    updated = options.Updated
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ── PUT api/processingoptions ─────────────────────────────────────────
        [HttpPut]
        public IActionResult Upsert([FromBody] ProcessingOptionsRequest request)
        {
            try
            {
                if (!IsAdmin) return RequireAdmin();

                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                var userID = CurrentUserID;
                if (string.IsNullOrEmpty(userID))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);
                master.ProcessingOptions.UpsertOptions(
                    request.ShowTemperature,
                    request.ShowTempRemarks,
                    request.ShowBagNo,
                    userID
                );

                return Ok(new { message = "Processing options saved." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}