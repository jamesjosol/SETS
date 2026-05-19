using Microsoft.AspNetCore.Mvc;
using Model.SETSDB;
using Model.Main;
using Service;

namespace SETS.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestRunningDayController : ControllerBase
    {
        private string? Branch => HttpContext.Session.GetString("BranchCode");
        private string? CurrentUserID => HttpContext.Session.GetString("UserID");
        private string? SectionCode => HttpContext.Session.GetString("SectionCode");
        private bool IsAdmin => HttpContext.Session.GetString("IsAdmin") == "True";
        private int RoleID => HttpContext.Session.GetInt32("RoleID") ?? 0;

        private bool IsAdminOrTL => IsAdmin || RoleID == 2;

        private IActionResult RequireAdminOrTL()
            => Unauthorized(new { message = "Administrator or Team Lead access required." });

        // ── GET api/test-running-day ──────────────────────────────────────────
        // Admin  → all entries
        // TL     → only entries whose TestGroupCode matches their section's test groups
        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                if (!IsAdminOrTL) return RequireAdminOrTL();

                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);

                List<Test_RunningDay> items;

                if (IsAdmin)
                {
                    items = master.TestRunningDay.GetAll();
                }
                else
                {
                    // Resolve this TL's section → get their test group codes
                    var sectionCode = SectionCode;
                    if (string.IsNullOrEmpty(sectionCode))
                        return Unauthorized(new { message = "Session expired." });

                    var testGroupCodes = master.SectionTestGroup
                        .GetBySectionCode(sectionCode)
                        .Where(tg => tg.Active)
                        .Select(tg => tg.TestGroupCode)
                        .ToList();

                    items = master.TestRunningDay.GetByTestGroupCodes(testGroupCodes);
                }

                var result = items.Select(t => new
                {
                    id = t.Id,
                    testCode = t.TestCode,
                    testName = t.TestName,
                    testGroupCode = t.TestGroupCode,
                    runningDays = t.RunningDays,
                    dayList = t.GetDayList(),
                    createdBy = t.CreatedBy,
                    created = t.Created,
                    updatedBy = t.UpdatedBy,
                    updated = t.Updated
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // ── GET api/test-running-day/search?param=FBS ─────────────────────────
        // Admin  → search all HCLAB tests
        // TL     → search restricted to their section's test groups
        [HttpGet("search")]
        public async Task<IActionResult> SearchHCLAB([FromQuery] string param)
        {
            try
            {
                if (!IsAdminOrTL) return RequireAdminOrTL();

                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                if (string.IsNullOrWhiteSpace(param) || param.Trim().Length < 3)
                    return BadRequest(new { message = "Minimum 3 characters required." });

                using var master = new MasterService(branch);

                // Resolve test group filter for non-admin TL
                List<string>? testGroupCodes = null;
                if (!IsAdmin)
                {
                    var sectionCode = SectionCode;
                    if (string.IsNullOrEmpty(sectionCode))
                        return Unauthorized(new { message = "Session expired." });

                    testGroupCodes = master.SectionTestGroup
                        .GetBySectionCode(sectionCode)
                        .Where(tg => tg.Active)
                        .Select(tg => tg.TestGroupCode)
                        .ToList();
                }

                var results = await master.TestRunningDay.GetHCLABTests(
                    param.Trim().ToUpper(), testGroupCodes);

                var configured = master.TestRunningDay.GetAll()
                    .Select(t => t.TestCode)
                    .ToHashSet();

                var mapped = results.Select(t => new
                {
                    testCode = t.TestCode,
                    testName = t.TestName,
                    testGroup = t.TestGroup,
                    hasSetup = configured.Contains(t.TestCode)
                }).ToList();

                return Ok(mapped);
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // ── POST api/test-running-day ─────────────────────────────────────────
        [HttpPost]
        public IActionResult Add([FromBody] AddTestRunningDayRequest request)
        {
            try
            {
                if (!IsAdminOrTL) return RequireAdminOrTL();

                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                if (string.IsNullOrWhiteSpace(request.TestCode))
                    return BadRequest(new { message = "Test code is required." });

                if (string.IsNullOrWhiteSpace(request.TestName))
                    return BadRequest(new { message = "Test name is required." });

                if (request.Days == null || !request.Days.Any())
                    return BadRequest(new { message = "At least one running day must be selected." });

                var validDays = new[] { "Sunday", "Monday", "Tuesday", "Wednesday",
                                        "Thursday", "Friday", "Saturday" };
                var invalid = request.Days.Except(validDays).ToList();
                if (invalid.Any())
                    return BadRequest(new { message = $"Invalid day(s): {string.Join(", ", invalid)}." });

                using var master = new MasterService(branch);

                if (master.TestRunningDay.GetByTestCode(request.TestCode.Trim().ToUpper()) != null)
                    return BadRequest(new { message = $"Running day setup for '{request.TestCode}' already exists." });

                master.TestRunningDay.Add(new Test_RunningDay
                {
                    TestCode = request.TestCode.Trim().ToUpper(),
                    TestName = request.TestName.Trim(),
                    TestGroupCode = request.TestGroupCode?.Trim().ToUpper(),
                    RunningDays = string.Join(";", request.Days),
                    CreatedBy = CurrentUserID ?? "SYSTEM",
                    Created = DateTime.Now
                });

                return Ok(new { success = true, message = "Running day setup saved." });
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // ── PUT api/test-running-day/{id} ─────────────────────────────────────
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] UpdateTestRunningDayRequest request)
        {
            try
            {
                if (!IsAdminOrTL) return RequireAdminOrTL();

                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                if (request.Days == null || !request.Days.Any())
                    return BadRequest(new { message = "At least one running day must be selected." });

                var validDays = new[] { "Sunday", "Monday", "Tuesday", "Wednesday",
                                        "Thursday", "Friday", "Saturday" };
                var invalid = request.Days.Except(validDays).ToList();
                if (invalid.Any())
                    return BadRequest(new { message = $"Invalid day(s): {string.Join(", ", invalid)}." });

                using var master = new MasterService(branch);

                var item = master.TestRunningDay.GetAll().FirstOrDefault(t => t.Id == id);
                if (item == null)
                    return NotFound(new { message = "Setup not found." });

                // TL can only edit entries belonging to their own section's test groups
                if (!IsAdmin)
                {
                    var sectionCode = SectionCode;
                    if (string.IsNullOrEmpty(sectionCode))
                        return Unauthorized(new { message = "Session expired." });

                    var testGroupCodes = master.SectionTestGroup
                        .GetBySectionCode(sectionCode)
                        .Where(tg => tg.Active)
                        .Select(tg => tg.TestGroupCode)
                        .ToHashSet();

                    if (item.TestGroupCode == null || !testGroupCodes.Contains(item.TestGroupCode))
                        return Unauthorized(new { message = "You do not have permission to edit this setup." });
                }

                item.RunningDays = string.Join(";", request.Days);
                item.UpdatedBy = CurrentUserID ?? "SYSTEM";
                item.Updated = DateTime.Now;

                master.TestRunningDay.Update(item);
                return Ok(new { success = true, message = "Running days updated." });
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // ── DELETE api/test-running-day/{id} ──────────────────────────────────
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                if (!IsAdminOrTL) return RequireAdminOrTL();

                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);

                var item = master.TestRunningDay.GetAll().FirstOrDefault(t => t.Id == id);
                if (item == null)
                    return NotFound(new { message = "Setup not found." });

                // TL can only delete entries belonging to their own section's test groups
                if (!IsAdmin)
                {
                    var sectionCode = SectionCode;
                    if (string.IsNullOrEmpty(sectionCode))
                        return Unauthorized(new { message = "Session expired." });

                    var testGroupCodes = master.SectionTestGroup
                        .GetBySectionCode(sectionCode)
                        .Where(tg => tg.Active)
                        .Select(tg => tg.TestGroupCode)
                        .ToHashSet();

                    if (item.TestGroupCode == null || !testGroupCodes.Contains(item.TestGroupCode))
                        return Unauthorized(new { message = "You do not have permission to delete this setup." });
                }

                master.TestRunningDay.Delete(id);
                return Ok(new { success = true, message = "Running day setup removed." });
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }
    }
}