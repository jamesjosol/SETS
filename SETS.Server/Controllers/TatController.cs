using Microsoft.AspNetCore.Mvc;
using Model.SETSDB;
using Model.Main;
using Service;

namespace SETS.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TatController : ControllerBase
    {
        private string? Branch => HttpContext.Session.GetString("BranchCode");
        private string? CurrentUserID => HttpContext.Session.GetString("UserID");
        private bool IsAdmin => HttpContext.Session.GetString("IsAdmin") == "True";

        private IActionResult RequireAdmin()
            => Unauthorized(new { message = "Administrator access required." });

        // ── GET api/tat ───────────────────────────────────────────────────────
        // All TAT settings for all endorsing sections
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
                var items = master.Tat.GetAll()
                    .Select(t => new
                    {
                        sectionCode = t.SectionCode,
                        hours = t.Hours,
                        minutes = t.Minutes,
                        appealWindow = t.AppealWindow,
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

        // ── GET api/tat/{sectionCode} ─────────────────────────────────────────
        // TAT setting for a specific section
        [HttpGet("{sectionCode}")]
        public IActionResult GetBySection(string sectionCode)
        {
            try
            {
                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);
                var item = master.Tat.GetBySection(sectionCode);
                if (item == null) return NotFound(new { message = "No TAT setting found for this section." });

                return Ok(new
                {
                    sectionCode = item.SectionCode,
                    hours = item.Hours,
                    minutes = item.Minutes,
                    appealWindow = item.AppealWindow
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ── PUT api/tat ───────────────────────────────────────────────────────
        // Upsert TAT settings (single or bulk)
        [HttpPut]
        public IActionResult Upsert([FromBody] List<TatUpsertRequest> requests)
        {
            try
            {
                if (!IsAdmin) return RequireAdmin();

                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                var userID = CurrentUserID;
                if (string.IsNullOrEmpty(userID))
                    return Unauthorized(new { message = "Session expired." });

                if (requests == null || requests.Count == 0)
                    return BadRequest(new { message = "No data provided." });

                using var master = new MasterService(branch);
                foreach (var r in requests)
                {
                    if (string.IsNullOrWhiteSpace(r.SectionCode))
                        continue;

                    if (r.Hours < 0 || r.Minutes < 0 || r.Minutes > 59)
                        return BadRequest(new { message = $"Invalid time values for section {r.SectionCode}." });

                    if (r.AppealWindow != "Before" && r.AppealWindow != "After")
                        return BadRequest(new { message = $"AppealWindow must be 'Before' or 'After' for section {r.SectionCode}." });

                    master.Tat.Upsert(new Tat_Section
                    {
                        SectionCode = r.SectionCode,
                        Hours = r.Hours,
                        Minutes = r.Minutes,
                        AppealWindow = r.AppealWindow
                    }, userID);
                }

                return Ok(new { message = "TAT settings saved." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ── GET api/tat/{sectionCode}/cycle ───────────────────────────────────
        // Get the current open cycle for a section (for the endorser's live timer)
        [HttpGet("{sectionCode}/cycle")]
        public IActionResult GetOpenCycle(string sectionCode)
        {
            try
            {
                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);
                var cycle = master.Tat.GetOpenCycle(sectionCode);
                var setting = master.Tat.GetBySection(sectionCode);

                return Ok(new
                {
                    hasOpenCycle = cycle != null,
                    cycleStart = cycle?.CycleStart,
                    thresholdMins = setting != null ? setting.Hours * 60 + setting.Minutes : (int?)null,
                    canAppeal = cycle != null && master.Tat.CanAppeal(sectionCode)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ── POST api/tat/{sectionCode}/appeal ─────────────────────────────────
        // Endorser files a "nothing to endorse" appeal
        [HttpPost("{sectionCode}/appeal")]
        public IActionResult Appeal(string sectionCode)
        {
            try
            {
                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                var userID = CurrentUserID;
                if (string.IsNullOrEmpty(userID))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);

                if (!master.Tat.CanAppeal(sectionCode))
                    return BadRequest(new { message = "Appeal not allowed at this time." });

                master.Tat.Appeal(sectionCode, userID, DateTime.Now);
                return Ok(new { message = "Appeal recorded. Timer has been reset." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }

}