using HCLAB;
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
        private bool IsAdmin => HttpContext.Session.GetString("IsAdmin") == "True";

        private readonly IConfiguration _configuration;

        public SettingsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

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

        // GET api/settings/branches/config-status
        [HttpGet("branches/config-status")]
        public IActionResult GetBranchConfigStatus()
        {
            try
            {
                if (string.IsNullOrEmpty(Branch))
                    return Unauthorized(new { message = "Session expired." });

                if (!IsAdmin) return RequireAdmin();

                var setsKeys = _configuration.GetSection("ConnectionStrings")
                    .GetChildren()
                    .Select(c => c.Key.ToUpper())
                    .ToHashSet();

                var hclabKeys = HclabConnection.GetKnownBranches()
                    .Select(b => b.ToUpper())
                    .ToHashSet();

                using var master = new MasterService(Branch);
                var branches = master.Branch.GetAll();

                var result = branches.Select(b => new
                {
                    code = b.Code,
                    active = b.Active,
                    created = b.Created,
                    createdBy = b.CreatedBy,
                    updated = b.Updated,
                    updatedBy = b.UpdatedBy,
                    inSets = setsKeys.Contains(b.Code.ToUpper()),
                    inHclab = hclabKeys.Contains(b.Code.ToUpper()),
                }).OrderBy(b => b.code).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // POST api/settings/branches
        [HttpPost("branches")]
        public IActionResult AddBranch([FromBody] AddBranchRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(Branch))
                    return Unauthorized(new { message = "Session expired." });

                if (!IsAdmin) return RequireAdmin();

                if (string.IsNullOrWhiteSpace(request.Code))
                    return BadRequest(new { message = "Branch code is required." });

                var code = request.Code.Trim().ToUpper();

                using var master = new MasterService(Branch);

                if (master.Branch.CodeExists(code))
                    return BadRequest(new { message = $"Branch '{code}' already exists." });

                master.Branch.Add(new Branch_Master
                {
                    Code = code,
                    Name = code,
                    Active = true,
                    Created = DateTime.Now,
                    CreatedBy = HttpContext.Session.GetString("UserID") ?? "SYSTEM",
                });

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // PATCH api/settings/branches/{code}/toggle
        [HttpPatch("branches/{code}/toggle")]
        public IActionResult ToggleBranch(string code)
        {
            try
            {
                if (string.IsNullOrEmpty(Branch))
                    return Unauthorized(new { message = "Session expired." });

                if (!IsAdmin) return RequireAdmin();

                var userID = HttpContext.Session.GetString("UserID") ?? "SYSTEM";

                using var master = new MasterService(Branch);
                master.Branch.Toggle(code, userID);

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // GET api/settings/branches/check-config?code=WES
        [HttpGet("branches/check-config")]
        public IActionResult CheckBranchConfig([FromQuery] string code)
        {
            try
            {
                if (string.IsNullOrEmpty(Branch))
                    return Unauthorized(new { message = "Session expired." });

                if (!IsAdmin) return RequireAdmin();

                var upper = code.Trim().ToUpper();

                var inSets = _configuration.GetSection("ConnectionStrings")
                    .GetChildren()
                    .Any(c => c.Key.ToUpper() == upper);

                var inHclab = HclabConnection.GetKnownBranches()
                    .Any(b => b.ToUpper() == upper);

                return Ok(new { inSets, inHclab });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        public record AddBranchRequest(string Code);
    }
}