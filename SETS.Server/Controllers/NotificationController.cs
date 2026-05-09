using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Model.Main;
using Model.SETSDB;
using Service;
using SETS.Server.Hubs;

namespace SETS.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private string? Branch => HttpContext.Session.GetString("BranchCode");
        private string? UserID => HttpContext.Session.GetString("UserID");
        private string? Category => HttpContext.Session.GetString("SectionCategory");
        private string? SectionCode => HttpContext.Session.GetString("SectionCode");
        private bool IsAdmin => HttpContext.Session.GetString("IsAdmin") == "True";

        private readonly IHubContext<NotificationHub> _hub;
        private readonly IConfiguration _config;

        public NotificationController(IHubContext<NotificationHub> hub, IConfiguration config)
        {
            _hub = hub;
            _config = config;
        }

        // ── GET api/notification ──────────────────────────────────────────────
        // Returns all notifications for the current user (read + unread)
        [HttpGet]
        public IActionResult GetForUser()
        {
            try
            {
                var branch = Branch;
                var userID = UserID;
                if (string.IsNullOrEmpty(branch) || string.IsNullOrEmpty(userID))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);
                var result = master.Notification.GetForUser(userID, Category, SectionCode, IsAdmin);
                return Ok(result);
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // ── PATCH api/notification/{id}/read ──────────────────────────────────
        [HttpPatch("{id}/read")]
        public IActionResult MarkRead(int id)
        {
            try
            {
                var branch = Branch;
                var userID = UserID;
                if (string.IsNullOrEmpty(branch) || string.IsNullOrEmpty(userID))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);
                master.Notification.MarkRead(id);
                return Ok(new { success = true });
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // ── PATCH api/notification/read-all ───────────────────────────────────
        [HttpPatch("read-all")]
        public IActionResult MarkAllRead()
        {
            try
            {
                var branch = Branch;
                var userID = UserID;
                if (string.IsNullOrEmpty(branch) || string.IsNullOrEmpty(userID))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);
                master.Notification.MarkAllRead(userID, Category, SectionCode, IsAdmin);
                return Ok(new { success = true });
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // POST api/notification/middleware
        // Internal endpoint — called by SETSMiddleware only
        // Secured by a shared secret key from appsettings
        [HttpPost("middleware")]
        public async Task<IActionResult> MiddlewareNotification([FromBody] MiddlewareNotificationRequest req,
            [FromHeader(Name = "X-Middleware-Key")] string? middlewareKey)
        {
            try
            {
                // Simple shared secret check
                var expectedKey = _config["MiddlewareKey"];
                if (string.IsNullOrEmpty(expectedKey) || middlewareKey != expectedKey)
                    return Unauthorized(new { message = "Invalid middleware key." });

                var branch = req.Branch;
                if (string.IsNullOrEmpty(branch))
                    return BadRequest(new { message = "Branch is required." });

                using var master = new MasterService(branch);

                var notif = master.Notification.Create(new CreateNotificationRequest
                {
                    NotifType = req.NotifType,
                    Title = req.Title,
                    Message = req.Message,
                    TargetCategory = req.TargetCategory,
                    TargetSection = req.TargetSection,
                    TargetUserID = req.TargetUserID,
                    IsAdmin = req.IsAdmin,
                    ReferenceID = req.ReferenceID
                });

                // Route to correct SignalR group
                if (req.IsAdmin)
                {
                    await _hub.Clients
                        .Group($"notif-branch-{branch}")
                        .SendAsync("NewNotification", BuildPayload(notif));
                }
                else if (!string.IsNullOrEmpty(req.TargetSection))
                {
                    await _hub.Clients
                        .Group($"notif-{branch}-sec-{req.TargetSection}")
                        .SendAsync("NewNotification", BuildPayload(notif));
                }
                else if (!string.IsNullOrEmpty(req.TargetUserID))
                {
                    await _hub.Clients
                        .Group($"notif-user-{req.TargetUserID}")
                        .SendAsync("NewNotification", BuildPayload(notif));
                }
                else if (!string.IsNullOrEmpty(req.TargetCategory))
                {
                    await _hub.Clients
                        .Group($"notif-{branch}-cat-{req.TargetCategory}")
                        .SendAsync("NewNotification", BuildPayload(notif));
                }

                return Ok(new { success = true });
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // POST api/notification/middleware-issue
        // Called by frontend health monitor when middleware goes offline
        [HttpPost("middleware-issue")]
        public async Task<IActionResult> ReportMiddlewareIssue()
        {
            try
            {
                var branch = Branch;
                var userID = UserID;
                if (string.IsNullOrEmpty(branch) || string.IsNullOrEmpty(userID))
                    return Unauthorized(new { message = "Session expired." });

                if (!IsAdmin)
                    return Unauthorized(new { message = "Admin access required." });

                using var master = new MasterService(branch);

                var notif = master.Notification.Create(new CreateNotificationRequest
                {
                    NotifType = NotifType.MiddlewareIssue,
                    Title = "Middleware Offline",
                    Message = "SETSMiddleware is not responding. Background tasks may be affected.",
                    IsAdmin = true,
                    ReferenceID = null
                });

                await _hub.Clients
                    .Group($"notif-branch-{branch}")
                    .SendAsync("NewNotification", BuildPayload(notif));

                return Ok(new { success = true });
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        private static object BuildPayload(Notification_Log notif) => new
        {
            notif.NotifID,
            notif.NotifType,
            notif.Title,
            notif.Message,
            notif.IsRead,
            notif.ReferenceID,
            notif.CreatedAt
        };
    }
}