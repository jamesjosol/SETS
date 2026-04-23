using Microsoft.AspNetCore.Mvc;
using Model.SETSDB;
using Service;

namespace SETS.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PCController : ControllerBase
    {
        // ── Helpers ──────────────────────────────────────────────────────────

        private string? Branch => HttpContext.Session.GetString("BranchCode");
        private bool IsAdmin => HttpContext.Session.GetString("IsAdmin") == "True";

        private IActionResult RequireAdmin()
            => Unauthorized(new { message = "Administrator access required." });

        // ── GET api/pc ────────────────────────────────────────────────────────
        // Returns all PCs with their assigned sections.
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

                var pcs = master.PC.GetAll();
                var allSections = master.Section.GetAll();

                var result = pcs.Select(pc =>
                {
                    var pcSections = master.PC.GetSectionsByPCId(pc.Id);
                    var sections = pcSections.Select(ps =>
                    {
                        var section = allSections.FirstOrDefault(s => s.Code == ps.SectionCode);
                        return new
                        {
                            id = ps.Id,
                            sectionCode = ps.SectionCode,
                            sectionName = section?.Name ?? ps.SectionCode
                        };
                    }).ToList();

                    return new
                    {
                        id = pc.Id,
                        ipAddress = pc.IpAddress,
                        description = pc.Description,
                        active = pc.Active,
                        createdBy = pc.CreatedBy,
                        created = pc.Created,
                        updatedBy = pc.UpdatedBy,
                        updated = pc.Updated,
                        sections
                    };
                }).OrderBy(p => p.ipAddress).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ── POST api/pc ───────────────────────────────────────────────────────
        // Register a new PC.
        [HttpPost]
        public IActionResult Add([FromBody] AddPCRequest request)
        {
            try
            {
                if (!IsAdmin) return RequireAdmin();

                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                if (string.IsNullOrWhiteSpace(request.IpAddress))
                    return BadRequest(new { message = "IP Address is required." });

                var userID = HttpContext.Session.GetString("UserID") ?? "SYSTEM";

                using var master = new MasterService(branch);

                // Check for duplicate
                var existing = master.PC.GetAll().FirstOrDefault(p => p.IpAddress == request.IpAddress);
                if (existing != null)
                    return BadRequest(new { message = $"IP address {request.IpAddress} is already registered." });

                var pc = new PC_Master
                {
                    IpAddress = request.IpAddress.Trim(),
                    Description = request.Description?.Trim() ?? string.Empty,
                    Active = true,
                    CreatedBy = userID,
                    Created = DateTime.Now
                };

                master.PC.Add(pc);

                // Assign sections if provided
                if (request.SectionCodes?.Any() == true)
                {
                    // Re-fetch to get the generated Id
                    var saved = master.PC.GetAll().First(p => p.IpAddress == pc.IpAddress);
                    foreach (var code in request.SectionCodes)
                    {
                        master.PC.AddSection(new PC_Section { PCId = saved.Id, SectionCode = code });
                    }
                }

                return Ok(new { success = true, message = "PC registered successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ── PUT api/pc/{id} ───────────────────────────────────────────────────
        // Update a PC's description and active status.
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] UpdatePCRequest request)
        {
            try
            {
                if (!IsAdmin) return RequireAdmin();

                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                var userID = HttpContext.Session.GetString("UserID") ?? "SYSTEM";

                using var master = new MasterService(branch);

                var pc = master.PC.GetAll().FirstOrDefault(p => p.Id == id);
                if (pc == null)
                    return NotFound(new { message = "PC not found." });

                // Check for IP duplicate if IP is being changed
                if (!string.IsNullOrWhiteSpace(request.IpAddress) && request.IpAddress != pc.IpAddress)
                {
                    var dup = master.PC.GetAll().FirstOrDefault(p => p.IpAddress == request.IpAddress);
                    if (dup != null)
                        return BadRequest(new { message = $"IP address {request.IpAddress} is already registered." });
                    pc.IpAddress = request.IpAddress.Trim();
                }

                pc.Description = request.Description?.Trim() ?? pc.Description;
                pc.Active = request.Active;
                pc.UpdatedBy = userID;
                pc.Updated = DateTime.Now;

                master.PC.Update(pc);

                return Ok(new { success = true, message = "PC updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ── PATCH api/pc/{id}/toggle ──────────────────────────────────────────
        // Toggle active/inactive status.
        [HttpPatch("{id}/toggle")]
        public IActionResult Toggle(int id)
        {
            try
            {
                if (!IsAdmin) return RequireAdmin();

                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                var userID = HttpContext.Session.GetString("UserID") ?? "SYSTEM";

                using var master = new MasterService(branch);

                var pc = master.PC.GetAll().FirstOrDefault(p => p.Id == id);
                if (pc == null)
                    return NotFound(new { message = "PC not found." });

                pc.Active = !pc.Active;
                pc.UpdatedBy = userID;
                pc.Updated = DateTime.Now;

                master.PC.Update(pc);

                return Ok(new { success = true, active = pc.Active });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ── PUT api/pc/{id}/sections ──────────────────────────────────────────
        // Replace the full set of section assignments for a PC.
        [HttpPut("{id}/sections")]
        public IActionResult UpdateSections(int id, [FromBody] UpdatePCSectionsRequest request)
        {
            try
            {
                if (!IsAdmin) return RequireAdmin();

                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);

                var pc = master.PC.GetAll().FirstOrDefault(p => p.Id == id);
                if (pc == null)
                    return NotFound(new { message = "PC not found." });

                // Remove all existing section assignments
                master.PC.DeleteSectionsByPCId(id);

                // Re-add new assignments
                foreach (var code in request.SectionCodes ?? new List<string>())
                {
                    master.PC.AddSection(new PC_Section { PCId = id, SectionCode = code });
                }

                return Ok(new { success = true, message = "Section assignments updated." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }

    // ── Request DTOs ──────────────────────────────────────────────────────────

    public class AddPCRequest
    {
        public string IpAddress { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<string>? SectionCodes { get; set; }
    }

    public class UpdatePCRequest
    {
        public string? IpAddress { get; set; }
        public string? Description { get; set; }
        public bool Active { get; set; }
    }

    public class UpdatePCSectionsRequest
    {
        public List<string>? SectionCodes { get; set; }
    }
}