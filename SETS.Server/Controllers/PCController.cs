using Microsoft.AspNetCore.Mvc;
using Model.SETSDB;
using Service;

namespace SETS.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PCController : ControllerBase
    {
        // ── Session helpers ───────────────────────────────────────────────────

        private string? Branch => HttpContext.Session.GetString("BranchCode");
        private string? SessionUserID => HttpContext.Session.GetString("UserID");
        private bool IsAdmin => HttpContext.Session.GetString("IsAdmin") == "True";
        private int RoleID => HttpContext.Session.GetInt32("RoleID") ?? 0;
        private string? SectionCode => HttpContext.Session.GetString("SectionCode");

        private bool IsTL => RoleID == 2;

        private IActionResult RequireAdmin()
            => Unauthorized(new { message = "Administrator access required." });

        private IActionResult RequireTL()
            => Unauthorized(new { message = "Team Lead access required." });

        // ══════════════════════════════════════════════════════════════════════
        // ADMIN ENDPOINTS
        // ══════════════════════════════════════════════════════════════════════

        // ── GET api/pc ────────────────────────────────────────────────────────
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
                        return new { id = ps.Id, sectionCode = ps.SectionCode, sectionName = section?.Name ?? ps.SectionCode };
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
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // ── POST api/pc ───────────────────────────────────────────────────────
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

                var userID = SessionUserID ?? "SYSTEM";

                using var master = new MasterService(branch);

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

                if (request.SectionCodes?.Any() == true)
                {
                    var saved = master.PC.GetAll().First(p => p.IpAddress == pc.IpAddress);
                    foreach (var code in request.SectionCodes)
                        master.PC.AddSection(new PC_Section { PCId = saved.Id, SectionCode = code });
                }

                return Ok(new { success = true, message = "PC registered successfully." });
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // ── PUT api/pc/{id} ───────────────────────────────────────────────────
        // Updates description and active status.
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] UpdatePCRequest request)
        {
            try
            {
                if (!IsAdmin) return RequireAdmin();

                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                var userID = SessionUserID ?? "SYSTEM";

                using var master = new MasterService(branch);

                var pc = master.PC.GetAll().FirstOrDefault(p => p.Id == id);
                if (pc == null) return NotFound(new { message = "PC not found." });

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
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // ── PATCH api/pc/{id}/toggle ──────────────────────────────────────────
        [HttpPatch("{id}/toggle")]
        public IActionResult Toggle(int id)
        {
            try
            {
                if (!IsAdmin) return RequireAdmin();

                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                var userID = SessionUserID ?? "SYSTEM";

                using var master = new MasterService(branch);

                var pc = master.PC.GetAll().FirstOrDefault(p => p.Id == id);
                if (pc == null) return NotFound(new { message = "PC not found." });

                pc.Active = !pc.Active;
                pc.UpdatedBy = userID;
                pc.Updated = DateTime.Now;

                master.PC.Update(pc);
                return Ok(new { success = true, active = pc.Active });
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // ── PUT api/pc/{id}/sections ──────────────────────────────────────────
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
                if (pc == null) return NotFound(new { message = "PC not found." });

                master.PC.DeleteSectionsByPCId(id);

                foreach (var code in request.SectionCodes ?? new List<string>())
                    master.PC.AddSection(new PC_Section { PCId = id, SectionCode = code });

                return Ok(new { success = true, message = "Section assignments updated." });
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // ── DELETE api/pc/{id} ────────────────────────────────────────────────
        // Hard deletes the PC and all its section assignments.
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                if (!IsAdmin) return RequireAdmin();

                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);

                var pc = master.PC.GetAll().FirstOrDefault(p => p.Id == id);
                if (pc == null) return NotFound(new { message = "PC not found." });

                master.PC.Delete(id);
                return Ok(new { success = true, message = "PC deleted successfully." });
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // ══════════════════════════════════════════════════════════════════════
        // TL ENDPOINTS (scoped to session SectionCode)
        // ══════════════════════════════════════════════════════════════════════

        // ── GET api/pc/tl ─────────────────────────────────────────────────────
        [HttpGet("tl")]
        public IActionResult TLGetAll()
        {
            try
            {
                if (!IsTL && !IsAdmin) return RequireTL();

                var branch = Branch;
                var sectionCode = SectionCode;

                if (string.IsNullOrEmpty(branch) || string.IsNullOrEmpty(sectionCode))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);

                var allPCs = master.PC.GetAll();
                var allSections = master.Section.GetAll();

                var result = allPCs
                    .Select(pc =>
                    {
                        var pcSections = master.PC.GetSectionsByPCId(pc.Id);
                        return new { pc, pcSections };
                    })
                    .Where(x => x.pcSections.Any(ps => ps.SectionCode == sectionCode))
                    .Select(x =>
                    {
                        var sections = x.pcSections.Select(ps =>
                        {
                            var section = allSections.FirstOrDefault(s => s.Code == ps.SectionCode);
                            return new { id = ps.Id, sectionCode = ps.SectionCode, sectionName = section?.Name ?? ps.SectionCode };
                        }).ToList();

                        return new
                        {
                            id = x.pc.Id,
                            ipAddress = x.pc.IpAddress,
                            description = x.pc.Description,
                            active = x.pc.Active,
                            createdBy = x.pc.CreatedBy,
                            created = x.pc.Created,
                            updatedBy = x.pc.UpdatedBy,
                            updated = x.pc.Updated,
                            sections
                        };
                    })
                    .OrderBy(p => p.ipAddress)
                    .ToList();

                return Ok(result);
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // ── POST api/pc/tl ────────────────────────────────────────────────────
        [HttpPost("tl")]
        public IActionResult TLAdd([FromBody] TLAddPCRequest request)
        {
            try
            {
                if (!IsTL && !IsAdmin) return RequireTL();

                var branch = Branch;
                var sectionCode = SectionCode;
                var userID = SessionUserID ?? "SYSTEM";

                if (string.IsNullOrEmpty(branch) || string.IsNullOrEmpty(sectionCode))
                    return Unauthorized(new { message = "Session expired." });

                if (string.IsNullOrWhiteSpace(request.IpAddress))
                    return BadRequest(new { message = "IP Address is required." });

                using var master = new MasterService(branch);

                var existing = master.PC.GetAll().FirstOrDefault(p => p.IpAddress == request.IpAddress);
                if (existing != null)
                {
                    var existingSections = master.PC.GetSectionsByPCId(existing.Id);
                    if (existingSections.Any(ps => ps.SectionCode == sectionCode))
                        return BadRequest(new { message = $"IP address {request.IpAddress} is already registered for your section." });

                    master.PC.AddSection(new PC_Section { PCId = existing.Id, SectionCode = sectionCode });
                    return Ok(new { success = true, message = "Section added to existing PC." });
                }

                var pc = new PC_Master
                {
                    IpAddress = request.IpAddress.Trim(),
                    Description = request.Description?.Trim() ?? string.Empty,
                    Active = true,
                    CreatedBy = userID,
                    Created = DateTime.Now
                };

                master.PC.Add(pc);

                var saved = master.PC.GetAll().First(p => p.IpAddress == pc.IpAddress);
                master.PC.AddSection(new PC_Section { PCId = saved.Id, SectionCode = sectionCode });

                return Ok(new { success = true, message = "PC registered successfully." });
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // ── PUT api/pc/tl/{id} ────────────────────────────────────────────────
        [HttpPut("tl/{id}")]
        public IActionResult TLUpdate(int id, [FromBody] UpdatePCRequest request)
        {
            try
            {
                if (!IsTL && !IsAdmin) return RequireTL();

                var branch = Branch;
                var sectionCode = SectionCode;
                var userID = SessionUserID ?? "SYSTEM";

                if (string.IsNullOrEmpty(branch) || string.IsNullOrEmpty(sectionCode))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);

                var pc = master.PC.GetAll().FirstOrDefault(p => p.Id == id);
                if (pc == null) return NotFound(new { message = "PC not found." });

                var pcSections = master.PC.GetSectionsByPCId(id);
                if (!pcSections.Any(ps => ps.SectionCode == sectionCode))
                    return Unauthorized(new { message = "You do not have access to this PC." });

                pc.Description = request.Description?.Trim() ?? pc.Description;
                pc.Active = request.Active;
                pc.UpdatedBy = userID;
                pc.Updated = DateTime.Now;

                master.PC.Update(pc);
                return Ok(new { success = true, message = "PC updated successfully." });
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // ── PATCH api/pc/tl/{id}/toggle ───────────────────────────────────────
        [HttpPatch("tl/{id}/toggle")]
        public IActionResult TLToggle(int id)
        {
            try
            {
                if (!IsTL && !IsAdmin) return RequireTL();

                var branch = Branch;
                var sectionCode = SectionCode;
                var userID = SessionUserID ?? "SYSTEM";

                if (string.IsNullOrEmpty(branch) || string.IsNullOrEmpty(sectionCode))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);

                var pc = master.PC.GetAll().FirstOrDefault(p => p.Id == id);
                if (pc == null) return NotFound(new { message = "PC not found." });

                var pcSections = master.PC.GetSectionsByPCId(id);
                if (!pcSections.Any(ps => ps.SectionCode == sectionCode))
                    return Unauthorized(new { message = "You do not have access to this PC." });

                pc.Active = !pc.Active;
                pc.UpdatedBy = userID;
                pc.Updated = DateTime.Now;

                master.PC.Update(pc);
                return Ok(new { success = true, active = pc.Active });
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // ── DELETE api/pc/tl/{id}/section ─────────────────────────────────────
        [HttpDelete("tl/{id}/section")]
        public IActionResult TLRemoveSection(int id)
        {
            try
            {
                if (!IsTL && !IsAdmin) return RequireTL();

                var branch = Branch;
                var sectionCode = SectionCode;

                if (string.IsNullOrEmpty(branch) || string.IsNullOrEmpty(sectionCode))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);

                var pc = master.PC.GetAll().FirstOrDefault(p => p.Id == id);
                if (pc == null) return NotFound(new { message = "PC not found." });

                var pcSections = master.PC.GetSectionsByPCId(id);
                var assignment = pcSections.FirstOrDefault(ps => ps.SectionCode == sectionCode);

                if (assignment == null)
                    return NotFound(new { message = "Your section is not assigned to this PC." });

                master.PC.DeleteSection(assignment.Id);
                return Ok(new { success = true, message = "Section removed from PC." });
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }
    }

    // ── Request DTOs ──────────────────────────────────────────────────────────

    public class AddPCRequest
    {
        public string IpAddress { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<string>? SectionCodes { get; set; }
    }

    public class TLAddPCRequest
    {
        public string IpAddress { get; set; } = string.Empty;
        public string? Description { get; set; }
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