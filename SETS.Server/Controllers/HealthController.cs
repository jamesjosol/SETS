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
    }
}