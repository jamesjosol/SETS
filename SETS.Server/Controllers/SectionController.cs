using Microsoft.AspNetCore.Mvc;
using Model.SETSDB;
using Model.Main;
using Service;

namespace SETS.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SectionController : ControllerBase
    {
        private string? Branch => HttpContext.Session.GetString("BranchCode");
        private string? CurrentUserID => HttpContext.Session.GetString("UserID");
        private bool IsAdmin => HttpContext.Session.GetString("IsAdmin") == "True";

        private IActionResult RequireAdmin()
            => Unauthorized(new { message = "Administrator access required." });

        // ── GET api/section ───────────────────────────────────────────────────
        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);

                var allTestGroups = master.SectionTestGroup.GetAll();

                var sections = master.Section.GetAll()
                    .OrderBy(s => s.BranchCode)
                    .ThenBy(s => s.Category)
                    .ThenBy(s => s.Name)
                    .Select(s =>
                    {
                        var testGroups = allTestGroups
                            .Where(tg => tg.SectionCode == s.Code)
                            .Select(tg => new { tg.Id, tg.TestGroupCode, tg.Active })
                            .ToList();

                        return new
                        {
                            code = s.Code,
                            name = s.Name,
                            branchCode = s.BranchCode,
                            category = s.Category,
                            autoNo = s.AutoNo,
                            active = s.Active,
                            created = s.Created,
                            createdBy = s.CreatedBy,
                            updated = s.Updated,
                            updatedBy = s.UpdatedBy,
                            cutOffTime = s.CutOffTime.HasValue
                                ? $"{(int)s.CutOffTime.Value.TotalHours:D2}:{s.CutOffTime.Value.Minutes:D2}"
                                : null,
                            testGroups
                        };
                    })
                    .ToList();

                return Ok(sections);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ── GET api/section/test-groups ───────────────────────────────────────
        // Returns all test groups with their current active section assignment.
        [HttpGet("test-groups")]
        public IActionResult GetTestGroups()
        {
            try
            {
                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);

                var allGroups = master.TestGroup.GetAll();
                var allAssigned = master.SectionTestGroup.GetAll();

                var result = allGroups.Select(g =>
                {
                    var assignment = allAssigned
                        .FirstOrDefault(a => a.TestGroupCode == g.Code && a.Active);

                    return new
                    {
                        code = g.Code,
                        name = g.Name,
                        assignedSectionCode = assignment?.SectionCode  // null = free to assign
                    };
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ── POST api/section ──────────────────────────────────────────────────
        [HttpPost]
        public IActionResult Add([FromBody] AddSectionRequest request)
        {
            try
            {
                if (!IsAdmin) return RequireAdmin();

                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                if (string.IsNullOrWhiteSpace(request.Code))
                    return BadRequest(new { message = "Section code is required." });

                if (string.IsNullOrWhiteSpace(request.Name))
                    return BadRequest(new { message = "Section name is required." });

                if (string.IsNullOrWhiteSpace(request.BranchCode))
                    return BadRequest(new { message = "Branch is required." });

                if (string.IsNullOrWhiteSpace(request.Category) ||
                    !new[] { "1", "2", "3" }.Contains(request.Category))
                    return BadRequest(new { message = "Invalid category." });

                if (request.Category == "3" &&
                    (request.TestGroupCodes == null || !request.TestGroupCodes.Any()))
                    return BadRequest(new { message = "Laboratory sections must have at least one test group." });

                var createdBy = CurrentUserID ?? "SYSTEM";
                var now = DateTime.Now;

                using var master = new MasterService(branch);

                // Duplicate code check
                if (master.Section.GetByCode(request.Code.Trim().ToUpper()) != null)
                    return BadRequest(new { message = $"Section code '{request.Code.ToUpper()}' already exists." });

                // One receiver per branch
                if (request.Category == "2")
                {
                    var receiverExists = master.Section.GetAll()
                        .Any(s => s.Category == "2" && s.BranchCode == request.BranchCode);
                    if (receiverExists)
                        return BadRequest(new { message = $"A Receiver section already exists for branch '{request.BranchCode}'." });
                }

                // Test group conflict check
                if (request.Category == "3" && request.TestGroupCodes?.Any() == true)
                {
                    var allAssigned = master.SectionTestGroup.GetAll()
                        .Where(a => a.Active).ToList();

                    var conflicts = request.TestGroupCodes
                        .Where(code => allAssigned.Any(a => a.TestGroupCode == code))
                        .ToList();

                    if (conflicts.Any())
                        return BadRequest(new
                        {
                            message = $"Test group(s) already assigned to another section: {string.Join(", ", conflicts)}."
                        });
                }

                var section = new Section_Master
                {
                    Code = request.Code.Trim().ToUpper(),
                    Name = request.Name.Trim(),
                    BranchCode = request.BranchCode.Trim(),
                    Category = request.Category,
                    AutoNo = request.Category == "1" ? request.AutoNo : 0,
                    Active = true,
                    Created = now,
                    CreatedBy = createdBy
                };

                master.Section.Add(section);

                if (request.Category == "3")
                {
                    foreach (var code in request.TestGroupCodes!)
                    {
                        master.SectionTestGroup.Add(new Section_TestGroup
                        {
                            SectionCode = section.Code,
                            TestGroupCode = code,
                            Active = true,
                            Created = now,
                            CreatedBy = createdBy
                        });
                    }
                }

                return Ok(new { success = true, message = "Section created successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ── PUT api/section/{code} ────────────────────────────────────────────
        [HttpPut("{code}")]
        public IActionResult Update(string code, [FromBody] UpdateSectionRequest request)
        {
            try
            {
                if (!IsAdmin) return RequireAdmin();

                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                if (string.IsNullOrWhiteSpace(request.Name))
                    return BadRequest(new { message = "Section name is required." });

                var updatedBy = CurrentUserID ?? "SYSTEM";
                var now = DateTime.Now;

                using var master = new MasterService(branch);

                var section = master.Section.GetByCode(code);
                if (section == null)
                    return NotFound(new { message = "Section not found." });

                if (section.Category == "3" &&
                    (request.TestGroupCodes == null || !request.TestGroupCodes.Any()))
                    return BadRequest(new { message = "Laboratory sections must have at least one test group." });

                // Conflict check — exclude self
                if (section.Category == "3" && request.TestGroupCodes?.Any() == true)
                {
                    var allAssigned = master.SectionTestGroup.GetAll()
                        .Where(a => a.Active && a.SectionCode != code).ToList();

                    var conflicts = request.TestGroupCodes
                        .Where(tgCode => allAssigned.Any(a => a.TestGroupCode == tgCode))
                        .ToList();

                    if (conflicts.Any())
                        return BadRequest(new
                        {
                            message = $"Test group(s) already assigned to another section: {string.Join(", ", conflicts)}."
                        });
                }

                section.Name = request.Name.Trim();
                // auto no should not be updated
                //section.AutoNo = section.Category == "1" ? request.AutoNo : 0;
                // ── Cut-off time (Lab sections only) ──────────────────────
                if (section.Category == "3")
                {
                    if (!string.IsNullOrEmpty(request.CutOffTime) &&
                        TimeSpan.TryParseExact(request.CutOffTime, @"hh\:mm", null, out var ts))
                        section.CutOffTime = ts;
                    else
                        section.CutOffTime = null;
                }
                section.UpdatedBy = updatedBy;
                section.Updated = now;

                master.Section.Update(section);

                // Sync test groups
                if (section.Category == "3")
                {
                    var existing = master.SectionTestGroup.GetBySectionCode(code);

                    foreach (var item in existing.Where(e =>
                        !request.TestGroupCodes!.Contains(e.TestGroupCode)))
                    {
                        item.Active = false;
                        master.SectionTestGroup.Update(item);
                    }

                    foreach (var tgCode in request.TestGroupCodes!)
                    {
                        var match = existing.FirstOrDefault(e => e.TestGroupCode == tgCode);
                        if (match == null)
                        {
                            master.SectionTestGroup.Add(new Section_TestGroup
                            {
                                SectionCode = code,
                                TestGroupCode = tgCode,
                                Active = true,
                                Created = now,
                                CreatedBy = updatedBy
                            });
                        }
                        else if (!match.Active)
                        {
                            match.Active = true;
                            master.SectionTestGroup.Update(match);
                        }
                    }
                }

                return Ok(new { success = true, message = "Section updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ── PATCH api/section/{code}/toggle ───────────────────────────────────
        [HttpPatch("{code}/toggle")]
        public IActionResult Toggle(string code)
        {
            try
            {
                if (!IsAdmin) return RequireAdmin();

                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);

                var section = master.Section.GetByCode(code);
                if (section == null)
                    return NotFound(new { message = "Section not found." });

                section.Active = !section.Active;
                section.UpdatedBy = CurrentUserID ?? "SYSTEM";
                section.Updated = DateTime.Now;

                master.Section.Update(section);

                return Ok(new { success = true, active = section.Active });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ── GET api/section/check-code/{code} ─────────────────────────────────
        [HttpGet("check-code/{code}")]
        public IActionResult CheckCode(string code)
        {
            try
            {
                if (!IsAdmin) return RequireAdmin();

                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);
                var exists = master.Section.GetByCode(code.Trim().ToUpper()) != null;

                return Ok(new { exists });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}