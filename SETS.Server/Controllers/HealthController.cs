using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service;
using System.Diagnostics;

namespace SETS.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        // GET api/health/ping
        // SETS Host reachability (measured client-side) + SETS Database check
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            var branch = HttpContext.Session.GetString("BranchCode");
            if (string.IsNullOrEmpty(branch))
                return Unauthorized(new { message = "Session expired." });

            using var master = new MasterService(branch);
            var db = master.Health.CheckDatabase();

            return Ok(new { db });
        }

        // GET api/health/hclab
        // HCLAB (Oracle) connectivity check
        [HttpGet("hclab")]
        public IActionResult HcLab()
        {
            var branch = HttpContext.Session.GetString("BranchCode");
            if (string.IsNullOrEmpty(branch))
                return Unauthorized(new { message = "Session expired." });

            using var master = new MasterService(branch);
            var result = master.Health.CheckHcLab();

            return Ok(result);
        }

        // GET api/health/middleware
        [HttpGet("middleware")]
        public async Task<IActionResult> Middleware()
        {
            var branch = HttpContext.Session.GetString("BranchCode");
            if (string.IsNullOrEmpty(branch))
                return Unauthorized(new { message = "Session expired." });

            try
            {
                using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };
                var resp = await http.GetAsync("http://localhost:5100/health");
                var json = await resp.Content.ReadAsStringAsync();
                return Content(json, "application/json");
            }
            catch
            {
                return Ok(new { online = false, branch, tasks = Array.Empty<object>() });
            }
        }

        [HttpGet("hclab-prelogin")]
        public IActionResult HcLabPreLogin([FromQuery] string branch)
        {
            if (string.IsNullOrEmpty(branch))
                return BadRequest(new { message = "Branch required." });

            using var master = new MasterService(branch);
            var result = master.Health.CheckHcLab();
            return Ok(result);
        }

        [HttpGet("status")]
        [AllowAnonymous]
        public IActionResult Status() => Ok(new { online = true });
    }
}