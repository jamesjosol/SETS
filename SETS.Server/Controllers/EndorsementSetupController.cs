// SETS.Server/Controllers/EndorsementSetupController.cs
using Microsoft.AspNetCore.Mvc;
using Model.SETSDB;
using Reposi;
using Service;

namespace SETS.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EndorsementSetupController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        private string? Branch => HttpContext.Session.GetString("BranchCode");
        private string? UserID => HttpContext.Session.GetString("UserID");
        private bool IsAdmin => HttpContext.Session.GetString("IsAdmin") == "True";

        public EndorsementSetupController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private IActionResult RequireAdmin()
            => Unauthorized(new { message = "Administrator access required." });

        private string LocalBranchCode
            => _configuration["Default:Branch"]
               ?? throw new Exception("Default branch not configured.");

        // ── GET api/endorsement-setup/partners ────────────────────────────
        // Returns all external partner branches + their registered sections
        [HttpGet("partners")]
        public IActionResult GetPartners()
        {
            try
            {
                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                if (!IsAdmin) return RequireAdmin();

                using var master = new MasterService(branch);

                var partners = master.Branch.GetExternal();

                var result = partners.Select(p =>
                {
                    var sections = master.Section
                        .GetByBranchIncludeInactive(p.Code)
                        .Select(s => new
                        {
                            code = s.Code,
                            name = s.Name,
                            category = s.Category,
                            active = s.Active
                        })
                        .ToList();

                    return new
                    {
                        code = p.Code,
                        name = p.Name,
                        active = p.Active,
                        sections
                    };
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ── GET api/endorsement-setup/eligible-branches ───────────────────
        // Returns branches eligible to become partners
        // (in Branch_Master, not local, not already external)
        [HttpGet("eligible-branches")]
        public IActionResult GetEligibleBranches()
        {
            try
            {
                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                if (!IsAdmin) return RequireAdmin();

                using var master = new MasterService(branch);

                var eligible = master.Branch
                    .GetEligibleForPartner(LocalBranchCode)
                    .Select(b => new
                    {
                        code = b.Code,
                        name = b.Name,
                        active = b.Active
                    })
                    .OrderBy(b => b.code)
                    .ToList();

                return Ok(eligible);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ── POST api/endorsement-setup/partners ───────────────────────────
        // Registers an existing Branch_Master row as an external partner
        [HttpPost("partners")]
        public IActionResult RegisterPartner([FromBody] RegisterPartnerRequest request)
        {
            try
            {
                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                if (!IsAdmin) return RequireAdmin();

                if (string.IsNullOrWhiteSpace(request.BranchCode))
                    return BadRequest(new { message = "Branch code is required." });

                var code = request.BranchCode.Trim().ToUpper();

                if (code == LocalBranchCode)
                    return BadRequest(new { message = "Cannot register the local branch as an external partner." });

                using var master = new MasterService(branch);
                master.Branch.RegisterAsExternal(code, UserID ?? "SYSTEM");

                return Ok(new { success = true });
            }
            catch (Exception ex) when (ex.Message.Contains("already registered"))
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex) when (ex.Message.Contains("not found"))
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ── PATCH api/endorsement-setup/partners/{code}/toggle ────────────
        // Toggles active/inactive on a partner branch
        [HttpPatch("partners/{code}/toggle")]
        public IActionResult TogglePartner(string code)
        {
            try
            {
                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                if (!IsAdmin) return RequireAdmin();

                using var master = new MasterService(branch);
                master.Branch.ToggleExternal(code.ToUpper(), UserID ?? "SYSTEM");

                return Ok(new { success = true });
            }
            catch (Exception ex) when (ex.Message.Contains("not found"))
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex) when (ex.Message.Contains("not an external"))
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ── DELETE api/endorsement-setup/partners/{code} ──────────────────
        // Unregisters a partner branch (sets IsExternal = false)
        // Also soft-deletes all its registered sections
        [HttpDelete("partners/{code}")]
        public IActionResult UnregisterPartner(string code)
        {
            try
            {
                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                if (!IsAdmin) return RequireAdmin();

                var upperCode = code.ToUpper();

                using var master = new MasterService(branch);

                var sections = master.Section
                    .GetByBranchIncludeInactive(upperCode)
                    .Where(s => s.Active)
                    .ToList();

                foreach (var s in sections)
                    master.Section.RemoveForeignSection(upperCode, s.Code, UserID ?? "SYSTEM");

                master.Branch.UnregisterExternal(upperCode, UserID ?? "SYSTEM");

                return Ok(new { success = true });
            }
            catch (Exception ex) when (ex.Message.Contains("not found"))
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ── GET api/endorsement-setup/partners/{branchCode}/lookup-section ─
        // Opens the partner branch's remote DB and resolves the section
        [HttpGet("partners/{branchCode}/lookup-section")]
        public IActionResult LookupSection(string branchCode, [FromQuery] string code)
        {
            try
            {
                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                if (!IsAdmin) return RequireAdmin();

                if (string.IsNullOrWhiteSpace(code))
                    return BadRequest(new { message = "Section code is required." });

                using var master = new MasterService(branch);

                var partnerBranch = master.Branch.GetByCode(branchCode.ToUpper());
                if (partnerBranch == null || !partnerBranch.IsExternal)
                    return NotFound(new { message = $"'{branchCode}' is not a registered partner branch." });

                var section = master.Section.LookupFromRemote(
                    branchCode.ToUpper(),
                    code.Trim().ToUpper()
                );

                if (section == null)
                    return NotFound(new { message = $"Section '{code.ToUpper()}' not found in {branchCode} database." });

                return Ok(new
                {
                    code = section.Code,
                    name = section.Name,
                    category = section.Category
                });
            }
            catch (Exception ex) when (ex.Message.Contains("not found") ||
                                       ex.Message.Contains("Connection"))
            {
                return StatusCode(503, new { message = $"Could not connect to {branchCode} database. Ensure the branch is reachable." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ── POST api/endorsement-setup/partners/{branchCode}/sections ─────
        // Saves a looked-up section to local Section_Master
        [HttpPost("partners/{branchCode}/sections")]
        public IActionResult AddSection(string branchCode, [FromBody] AddPartnerSectionRequest request)
        {
            try
            {
                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                if (!IsAdmin) return RequireAdmin();

                if (string.IsNullOrWhiteSpace(request.Code))
                    return BadRequest(new { message = "Section code is required." });

                if (string.IsNullOrWhiteSpace(request.Name))
                    return BadRequest(new { message = "Section name is required." });

                if (string.IsNullOrWhiteSpace(request.Category))
                    return BadRequest(new { message = "Section category is required." });

                var upperBranch = branchCode.ToUpper();

                using var master = new MasterService(branch);

                var partnerBranch = master.Branch.GetByCode(upperBranch);
                if (partnerBranch == null || !partnerBranch.IsExternal)
                    return NotFound(new { message = $"'{branchCode}' is not a registered partner branch." });

                master.Section.AddForeignSection(
                    upperBranch,
                    request.Code.Trim().ToUpper(),
                    request.Name.Trim(),
                    request.Category,
                    UserID ?? "SYSTEM"
                );

                return Ok(new { success = true });
            }
            catch (Exception ex) when (ex.Message.Contains("already registered"))
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ── DELETE api/endorsement-setup/partners/{branchCode}/sections/{sectionCode}
        // Soft-deletes a registered section (IsActive = false)
        [HttpDelete("partners/{branchCode}/sections/{sectionCode}")]
        public IActionResult RemoveSection(string branchCode, string sectionCode)
        {
            try
            {
                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                if (!IsAdmin) return RequireAdmin();

                using var master = new MasterService(branch);
                master.Section.RemoveForeignSection(
                    branchCode.ToUpper(),
                    sectionCode.ToUpper(),
                    UserID ?? "SYSTEM"
                );

                return Ok(new { success = true });
            }
            catch (Exception ex) when (ex.Message.Contains("not found"))
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // GET api/EndorsementSetup/partners/{branchCode}/check?sectionCode=WS
        [HttpGet("partners/{branchCode}/check")]
        public async Task<IActionResult> CheckPartner(string branchCode, [FromQuery] string sectionCode)
        {
            try
            {
                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                if (!IsAdmin && string.IsNullOrEmpty(sectionCode))
                    return BadRequest(new { message = "Section code is required." });

                var upperBranch = branchCode.ToUpper();
                var upperSection = sectionCode?.Trim().ToUpper() ?? string.Empty;

                using var master = new MasterService(branch);

                // Verify it's a registered active partner
                var partnerBranch = master.Branch.GetByCode(upperBranch);
                if (partnerBranch == null || !partnerBranch.IsExternal || !partnerBranch.Active)
                    return NotFound(new { message = $"'{upperBranch}' is not an active partner branch." });

                var errors = new List<string>();
                bool reachable = false;
                long latencyMs = 0;
                bool localSectionExistsOnRemote = false;
                bool remoteProcessingSectionExistsLocally = false;
                string? remoteProcessingSectionCode = null;

                // ── Check 1: Connectivity ping ────────────────────────────────────
                var sw = System.Diagnostics.Stopwatch.StartNew();
                try
                {
                    var remoteConn = SetsConnection.ConnectionString(upperBranch);
                    using var remoteContext = new Reposi.Context.AppDbContextFactory()
                        .CreateContext(remoteConn);

                    // Lightweight query — just touch the DB
                    _ = remoteContext.Branch_Master.Any();
                    sw.Stop();
                    latencyMs = sw.ElapsedMilliseconds;
                    reachable = true;
                }
                catch
                {
                    sw.Stop();
                    latencyMs = sw.ElapsedMilliseconds;
                    reachable = false;
                    errors.Add($"Cannot connect to {upperBranch} database.");
                    return Ok(new
                    {
                        reachable,
                        latencyMs,
                        localSectionExistsOnRemote,
                        remoteProcessingSectionExistsLocally,
                        remoteProcessingSectionCode,
                        errors
                    });
                }

                // ── Check 2: Local section exists on remote ───────────────────────
                try
                {
                    var remoteConn = SetsConnection.ConnectionString(upperBranch);
                    using var remoteContext = new Reposi.Context.AppDbContextFactory()
                        .CreateContext(remoteConn);

                    localSectionExistsOnRemote = remoteContext.Section_Master
                        .Any(s => s.Code == upperSection && s.Active);

                    if (!localSectionExistsOnRemote)
                        errors.Add($"Section '{upperSection}' is not registered in {upperBranch}. Ask the {upperBranch} admin to register it.");
                }
                catch
                {
                    errors.Add($"Could not verify section registration on {upperBranch}.");
                }

                // ── Check 3: Remote processing section exists locally ─────────────
                var remoteProcessing = master.Section
                    .GetByBranchIncludeInactive(upperBranch)
                    .FirstOrDefault(s => s.Category == "2" && s.Active);

                if (remoteProcessing != null)
                {
                    remoteProcessingSectionExistsLocally = true;
                    remoteProcessingSectionCode = remoteProcessing.Code;
                }
                else
                {
                    errors.Add($"No active processing section for {upperBranch} found locally. Register it in Endorsement Setup.");
                }

                return Ok(new
                {
                    reachable,
                    latencyMs,
                    localSectionExistsOnRemote,
                    remoteProcessingSectionExistsLocally,
                    remoteProcessingSectionCode,
                    errors
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // GET api/EndorsementSetup/active-partners
        // Public to all authenticated users — used by NewEndorsement.vue dropdown
        [HttpGet("active-partners")]
        public IActionResult GetActivePartners()
        {
            try
            {
                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);

                var partners = master.Branch.GetActiveExternal();

                var result = partners.Select(p =>
                {
                    var sections = master.Section
                        .GetByBranchIncludeInactive(p.Code)
                        .Where(s => s.Active)
                        .Select(s => new
                        {
                            code = s.Code,
                            name = s.Name,
                            category = s.Category,
                            active = s.Active
                        })
                        .ToList();

                    return new
                    {
                        code = p.Code,
                        name = p.Name,
                        active = p.Active,
                        sections
                    };
                })
                .Where(p => p.sections.Any(s => s.category == "2"))
                .ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // GET api/EndorsementSetup/settings
        [HttpGet("settings")]
        public IActionResult GetSettings()
        {
            try
            {
                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);
                var settings = master.BranchSettings.Get();

                return Ok(new
                {
                    isOutboundEnabled = settings.IsOutboundEnabled,
                    updatedAt = settings.UpdatedAt,
                    updatedBy = settings.UpdatedBy
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // PATCH api/EndorsementSetup/settings/outbound
        [HttpPatch("settings/outbound")]
        public IActionResult SetOutbound([FromBody] SetOutboundRequest request)
        {
            try
            {
                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                if (!IsAdmin) return RequireAdmin();

                using var master = new MasterService(branch);
                master.BranchSettings.SetOutbound(request.Enabled, UserID ?? "SYSTEM");

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        public record SetOutboundRequest(bool Enabled);
    }


    // ── Request models ─────────────────────────────────────────────────────
    public record RegisterPartnerRequest(string BranchCode);

    public record AddPartnerSectionRequest(string Code, string Name, string Category);
}