using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Model.Main;
using Service;
using SETS.Server.Hubs;

namespace SETS.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnnouncementController : ControllerBase
    {
        private readonly IHubContext<AnnouncementHub> _hub;

        private string? Branch => HttpContext.Session.GetString("BranchCode");
        private string? SessionUserID => HttpContext.Session.GetString("UserID");
        private bool IsAdmin => HttpContext.Session.GetString("IsAdmin") == "True";

        public AnnouncementController(IHubContext<AnnouncementHub> hub)
        {
            _hub = hub;
        }

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
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
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
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // ── POST api/announcement ─────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAnnouncementRequest req)
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

                // Push to all clients on this branch immediately
                var active = master.Announcement.GetActive();
                await _hub.Clients.Group($"branch-{branch}")
                    .SendAsync("AnnouncementUpdated", active);
                    
                return Ok(new { success = true });
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // ── PATCH api/announcement/{id}/deactivate ────────────────────────────
        [HttpPatch("{id}/deactivate")]
        public async Task<IActionResult> Deactivate(int id)
        {
            try
            {
                if (!IsAdmin) return RequireAdmin();

                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);
                master.Announcement.Deactivate(id, SessionUserID ?? string.Empty);

                // Notify all clients to clear the banner
                await _hub.Clients.Group($"branch-{branch}")
                    .SendAsync("AnnouncementUpdated", null);

                return Ok(new { success = true });
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }
    }
}