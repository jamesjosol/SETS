using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Model.Main;
using Service;

namespace SETS.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChangelogController : ControllerBase
    {
        private readonly IConfiguration _config;

        public ChangelogController(IConfiguration config)
        {
            _config = config;
        }

        // Session-based (for admin write operations)
        private string? SessionBranch => HttpContext.Session.GetString("BranchCode");
        private string? CurrentUser => HttpContext.Session.GetString("UserID");
        private bool IsAdmin => HttpContext.Session.GetString("IsAdmin") == "True";

        // Config-based fallback — works even before session is established
        // Used by read-only endpoints called immediately after login
        private string ConfigBranch => _config["Default:Branch"] ?? string.Empty;

        // For public-facing endpoints: prefer session branch, fall back to config branch
        private string? AnyBranch => SessionBranch ?? ConfigBranch;
        private string? AnyUser => HttpContext.Session.GetString("UserID");

        private IActionResult RequireAdmin()
            => Unauthorized(new { message = "Administrator access required." });

        // ── GET api/changelog ─────────────────────────────────────────────────
        // All entries, newest-first. Admin only.
        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                if (!IsAdmin) return RequireAdmin();

                var branch = SessionBranch;
                if (string.IsNullOrEmpty(branch)) return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);
                var result = master.Changelog.GetAll();
                return Ok(result);
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // ── GET api/changelog/latest ──────────────────────────────────────────
        // Latest entry. No session required — uses config branch as fallback.
        // Called immediately after login before session is fully propagated.
        [HttpGet("latest")]
        public IActionResult GetLatest()
        {
            try
            {
                var branch = AnyBranch;
                if (string.IsNullOrEmpty(branch))
                    return Ok(new { exists = false });

                using var master = new MasterService(branch);
                var result = master.Changelog.GetLatest();

                if (result == null)
                    return Ok(new { exists = false });

                return Ok(new { exists = true, entry = result });
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // ── GET api/changelog/seen?version=1.4.0 ─────────────────────────────
        // Check if the current user has seen a specific version.
        [HttpGet("seen")]
        public IActionResult HasSeen([FromQuery] string version)
        {
            try
            {
                var branch = AnyBranch;
                var userID = AnyUser;

                // If no session yet, treat as not seen — modal will show
                if (string.IsNullOrEmpty(branch) || string.IsNullOrEmpty(userID))
                    return Ok(new { seen = false });

                using var master = new MasterService(branch);
                var seen = master.Changelog.HasSeen(userID, version);
                return Ok(new { seen });
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // ── POST api/changelog ────────────────────────────────────────────────
        // Create a new changelog entry. Admin only.
        [HttpPost]
        public IActionResult Create([FromBody] CreateChangelogRequest req)
        {
            try
            {
                if (!IsAdmin) return RequireAdmin();

                var branch = SessionBranch;
                var userID = CurrentUser;
                if (string.IsNullOrEmpty(branch) || string.IsNullOrEmpty(userID))
                    return Unauthorized(new { message = "Session expired." });

                if (string.IsNullOrWhiteSpace(req.Version))
                    return BadRequest(new { message = "Version is required." });

                if (string.IsNullOrWhiteSpace(req.Title))
                    return BadRequest(new { message = "Title is required." });

                if (req.Items == null || req.Items.Count == 0)
                    return BadRequest(new { message = "At least one changelog item is required." });

                using var master = new MasterService(branch);
                var result = master.Changelog.Create(req, userID);
                return Ok(new { success = true, entry = result });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // ── DELETE api/changelog/{id} ─────────────────────────────────────────
        // Delete an entry and all its items. Admin only.
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                if (!IsAdmin) return RequireAdmin();

                var branch = SessionBranch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);
                master.Changelog.Delete(id);
                return Ok(new { success = true, message = "Changelog entry deleted." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // ── POST api/changelog/seen ───────────────────────────────────────────
        // Mark a version as seen by the current user.
        [HttpPost("seen")]
        public IActionResult MarkSeen([FromBody] MarkChangelogSeenRequest req)
        {
            try
            {
                var branch = AnyBranch;
                var userID = AnyUser;

                // If session isn't established yet, silently succeed —
                // the modal will show again next login which is acceptable
                if (string.IsNullOrEmpty(branch) || string.IsNullOrEmpty(userID))
                    return Ok(new { success = true });

                if (string.IsNullOrWhiteSpace(req.Version))
                    return BadRequest(new { message = "Version is required." });

                using var master = new MasterService(branch);
                master.Changelog.MarkSeen(userID, req.Version);
                return Ok(new { success = true });
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }
    }
}