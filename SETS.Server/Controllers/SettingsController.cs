using Microsoft.AspNetCore.Mvc;
using Model.SETSDB;
using Service;

namespace SETS.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SettingsController : ControllerBase
    {
        private string? Branch => HttpContext.Session.GetString("BranchCode");
        private bool IsAdmin => HttpContext.Session.GetString("IsAdmin") == "true";

        private IActionResult RequireAdmin()
            => Unauthorized(new { message = "Administrator access required." });

        // ── GET api/settings/sections ─────────────────────────────────────────
        [HttpGet("sections")]
        public IActionResult GetSections()
        {
            try
            {
                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);
                var sections = master.Section.GetAll()
                    .OrderBy(s => s.Name)
                    .Select(s => new
                    {
                        code = s.Code,
                        name = s.Name,
                        branchCode = s.BranchCode,
                        category = s.Category,
                        active = s.Active
                    })
                    .ToList();

                return Ok(sections);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ── GET api/settings/branches ─────────────────────────────────────────
        [HttpGet("branches")]
        public IActionResult GetBranches()
        {
            try
            {
                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);
                var branches = master.Branch.GetAll()
                    .OrderBy(b => b.Code)
                    .Select(b => new
                    {
                        code = b.Code,
                        name = b.Name,
                        active = b.Active
                    })
                    .ToList();

                return Ok(branches);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}