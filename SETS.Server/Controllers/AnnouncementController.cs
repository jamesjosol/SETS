using Microsoft.AspNetCore.Mvc;
using Model.Main;
using Service;

namespace SETS.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnnouncementController : ControllerBase
    {
        private string? Branch => HttpContext.Session.GetString("BranchCode");
        private string? SessionUserID => HttpContext.Session.GetString("UserID");
        private bool IsAdmin => HttpContext.Session.GetString("IsAdmin") == "True";

        private IActionResult RequireAdmin()
            => Unauthorized(new { message = "Administrator access required." });

        // ── GET api/announcement/active ───────────────────────────────────────
        [HttpGet("active")]
        public IActionResult GetActive()
        {
            try
            {
                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);
                var result = master.Announcement.GetActive();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ── GET api/announcement ──────────────────────────────────────────────
        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                if (!IsAdmin) return RequireAdmin();

                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);
                var result = master.Announcement.GetAll();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ── POST api/announcement ─────────────────────────────────────────────
        [HttpPost]
        public IActionResult Create([FromBody] CreateAnnouncementRequest req)
        {
            try
            {
                if (!IsAdmin) return RequireAdmin();

                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                req.CreatedBy = SessionUserID ?? string.Empty;

                using var master = new MasterService(branch);
                master.Announcement.Create(req);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ── PATCH api/announcement/{id}/deactivate ────────────────────────────
        [HttpPatch("{id}/deactivate")]
        public IActionResult Deactivate(int id)
        {
            try
            {
                if (!IsAdmin) return RequireAdmin();

                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);
                master.Announcement.Deactivate(id, SessionUserID ?? string.Empty);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}