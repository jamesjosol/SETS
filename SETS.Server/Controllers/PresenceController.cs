using Microsoft.AspNetCore.Mvc;
using SETS.Server.Services;

namespace SETS.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PresenceController : ControllerBase
    {
        private readonly IUserPresenceService _presence;

        private string? Branch => HttpContext.Session.GetString("BranchCode");
        private bool IsAdmin => HttpContext.Session.GetString("IsAdmin") == "True";

        public PresenceController(IUserPresenceService presence)
        {
            _presence = presence;
        }

        // GET api/presence
        // Returns the current online user list for the caller's branch.
        // Admin-only — non-admins receive 403.
        [HttpGet]
        public IActionResult GetOnlineUsers()
        {
            if (!IsAdmin)
                return StatusCode(403, new { message = "Admin access required." });

            var branch = Branch;
            if (string.IsNullOrEmpty(branch))
                return Unauthorized(new { message = "Session expired." });

            var users = _presence.GetOnlineUsers(branch)
                .Select(e => new
                {
                    e.UserID,
                    e.UserName,
                    e.BranchCode,
                    e.SectionCode,
                    e.SectionName,
                    e.Category,
                    e.IsAdmin,
                    connectedAt = e.ConnectedAt.ToString("yyyy-MM-ddTHH:mm:ss"),
                })
                .ToList();

            return Ok(users);
        }
    }
}