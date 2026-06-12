using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Service;

namespace SETS.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MonitorController : ControllerBase
    {
        private readonly IConfiguration _config;

        public MonitorController(IConfiguration config)
        {
            _config = config;
        }

        private string? SessionBranch => HttpContext.Session.GetString("BranchCode");
        private string? CurrentUser => HttpContext.Session.GetString("UserID");

        // ── GET api/monitor/sets?requests=1 ───────────────────────────────────
        // SETS-only server monitor snapshot, proxied from the local middleware.
        // Requires a logged-in session; developer-only visibility is enforced
        // on the frontend (authStore.isDeveloper), same as the Changelog tab.
        [HttpGet("sets")]
        public async Task<IActionResult> GetSetsMonitor([FromQuery] int requests = 1)
        {
            try
            {
                var branch = SessionBranch;
                if (string.IsNullOrEmpty(branch) || string.IsNullOrEmpty(CurrentUser))
                    return Unauthorized(new { message = "Session expired." });

                var baseUrl = _config["Middleware:BaseUrl"] ?? "http://localhost:5100";
                var key = _config["Middleware:DeployerKey"] ?? string.Empty;

                using var master = new MasterService(branch);
                var (status, json) = await master.Monitor
                    .GetSetsSnapshotAsync(baseUrl, key, requests == 1);

                // Pass the middleware's response through untouched
                return new ContentResult
                {
                    Content = json,
                    ContentType = "application/json",
                    StatusCode = status
                };
            }
            catch (HttpRequestException)
            {
                // Middleware not running on this server — soft payload, not a 500,
                // so the dashboard renders an "offline" state instead of an error
                return Ok(new { available = false, message = "Middleware is unreachable on this server." });
            }
            catch (TaskCanceledException)
            {
                return Ok(new { available = false, message = "Middleware timed out." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}