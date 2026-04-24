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
        private bool IsAdmin => HttpContext.Session.GetString("IsAdmin") == "True";

        private IActionResult RequireAdmin()
            => Unauthorized(new { message = "Administrator access required." });

        // ── GET api/test-running-day ──────────────────────────────────────────
        // All configured running day setups.
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
                var items = master.TestRunningDay.GetAll()
                    .Select(t => new
                    {
                        id = t.Id,
                        testCode = t.TestCode,
                        testName = t.TestName,
                        runningDays = t.RunningDays,
                        dayList = t.GetDayList(),
                        createdBy = t.CreatedBy,
                        created = t.Created,
                        updatedBy = t.UpdatedBy,
                        updated = t.Updated
                    })
                    .ToList();

                return Ok(items);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ── GET api/test-running-day/search?param=FBS ─────────────────────────
        // Searches HCLAB for tests matching the param (3+ chars).
        [HttpGet("search")]
        public async Task<IActionResult> SearchHCLAB([FromQuery] string param)
        {
            try
            {
                if (!IsAdmin) return RequireAdmin();

                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                if (string.IsNullOrWhiteSpace(param) || param.Trim().Length < 3)
                    return BadRequest(new { message = "Minimum 3 characters required." });

                using var master = new MasterService(branch);
                var results = await master.TestRunningDay.GetHCLABTests(param.Trim().ToUpper());

                // Mark which ones already have a running day configured
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
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ── POST api/test-running-day ─────────────────────────────────────────
        // Create a new running day setup.
        [HttpPost]
        public IActionResult Add([FromBody] AddTestRunningDayRequest request)
        {
            try
            {
                if (!IsAdmin) return RequireAdmin();

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

                // Duplicate check
                if (master.TestRunningDay.GetByTestCode(request.TestCode.Trim().ToUpper()) != null)
                    return BadRequest(new { message = $"Running day setup for '{request.TestCode}' already exists." });

                master.TestRunningDay.Add(new Test_RunningDay
                {
                    TestCode = request.TestCode.Trim().ToUpper(),
                    TestName = request.TestName.Trim(),
                    RunningDays = string.Join(";", request.Days),
                    CreatedBy = CurrentUserID ?? "SYSTEM",
                    Created = DateTime.Now
                });

                return Ok(new { success = true, message = "Running day setup saved." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ── PUT api/test-running-day/{id} ─────────────────────────────────────
        // Update running days for an existing setup.
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] UpdateTestRunningDayRequest request)
        {
            try
            {
                if (!IsAdmin) return RequireAdmin();

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

                item.RunningDays = string.Join(";", request.Days);
                item.UpdatedBy = CurrentUserID ?? "SYSTEM";
                item.Updated = DateTime.Now;

                master.TestRunningDay.Update(item);

                return Ok(new { success = true, message = "Running days updated." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ── DELETE api/test-running-day/{id} ──────────────────────────────────
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

                var item = master.TestRunningDay.GetAll().FirstOrDefault(t => t.Id == id);
                if (item == null)
                    return NotFound(new { message = "Setup not found." });

                master.TestRunningDay.Delete(item.Id);

                return Ok(new { success = true, message = "Running day setup removed." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}