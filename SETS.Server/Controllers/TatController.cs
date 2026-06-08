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

        // ── GET api/tat/processing ────────────────────────────────────────────────
        // Get the processing TAT setting for the current branch
        [HttpGet("processing")]
        public IActionResult GetProcessingTat()
        {
            try
            {
                if (!IsAdmin) return RequireAdmin();

                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);
                var item = master.Tat.GetProcessingTat();

                return Ok(new
                {
                    hours = item?.Hours ?? 0,
                    minutes = item?.Minutes ?? 30,
                    updatedBy = item?.UpdatedBy,
                    updated = item?.Updated
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ── PUT api/tat/processing ────────────────────────────────────────────────
        // Upsert the processing TAT setting for the current branch
        [HttpPut("processing")]
        public IActionResult UpsertProcessingTat([FromBody] ProcessingTatUpsertRequest request)
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

                if (request.Hours < 0 || request.Minutes < 0 || request.Minutes > 59)
                    return BadRequest(new { message = "Invalid time values." });

                using var master = new MasterService(branch);
                master.Tat.UpsertProcessingTat(request.Hours, request.Minutes, userID);

                return Ok(new { message = "Processing TAT saved." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ── GET api/tat/cycle-logs ────────────────────────────────────────────────
        // Endorser: returns cycle logs for their own section.
        // Admin: returns all sections, optionally filtered by sectionCode query param.
        [HttpGet("cycle-logs")]
        public IActionResult GetCycleLogs([FromQuery] string? sectionCode,
                                           [FromQuery] string? dateFrom,
                                           [FromQuery] string? dateTo)
        {
            try
            {
                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                var from = DateTime.TryParse(dateFrom, out var df) ? df : DateTime.Today;
                var to = DateTime.TryParse(dateTo, out var dt) ? dt : DateTime.Today;

                using var master = new MasterService(branch);

                // Load all active endorsing sections for name resolution
                var sections = master.Section.GetActive()
                    .ToDictionary(s => s.Code, s => s.Name);

                List<Model.SETSDB.Tat_Cycle_Log> logs;

                if (IsAdmin)
                {
                    logs = string.IsNullOrWhiteSpace(sectionCode)
                        ? master.Tat.GetAllCycleLogsByDateRange(from, to)
                        : master.Tat.GetCycleLogsByDateRange(sectionCode, from, to);
                }
                else
                {
                    // Endorser — use their own session section
                    var sessionSection = HttpContext.Session.GetString("SectionCode");
                    if (string.IsNullOrEmpty(sessionSection))
                        return Unauthorized(new { message = "Session expired." });

                    logs = master.Tat.GetCycleLogsByDateRange(sessionSection, from, to);
                }

                var result = logs.Select(c => new
                {
                    id = c.Id,
                    sectionCode = c.SectionCode,
                    sectionName = sections.TryGetValue(c.SectionCode, out var sn) ? sn : c.SectionCode,
                    cycleStart = c.CycleStart,
                    cycleEnd = c.CycleEnd,
                    elapsedMinutes = c.ElapsedMinutes,
                    result = c.Result,
                    batchNo = c.BatchNo,
                    appealedBy = c.AppealedBy,
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ════════════════════════════════════════════════════════════════════════════
        // OUTBOUND TAT
        // ════════════════════════════════════════════════════════════════════════════

        // ── GET api/tat/outbound/settings ─────────────────────────────────────────
        // Returns OutboundTatEnabled + OutboundTatAppealEnabled for this branch
        [HttpGet("outbound/settings")]
        public IActionResult GetOutboundTatSettings()
        {
            try
            {
                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);
                return Ok(new
                {
                    outboundTatEnabled = master.TatOutbound.IsOutboundTatEnabled(),
                    outboundTatAppealEnabled = master.TatOutbound.IsAppealEnabled()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ── PUT api/tat/outbound/settings ─────────────────────────────────────────
        // Toggle OutboundTatEnabled and/or OutboundTatAppealEnabled (admin only)
        [HttpPut("outbound/settings")]
        public IActionResult UpdateOutboundTatSettings([FromBody] OutboundTatSettingsRequest request)
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

                using var master = new MasterService(branch);
                master.TatOutbound.SetOutboundTatEnabled(request.OutboundTatEnabled, userID);
                master.TatOutbound.SetAppealEnabled(request.OutboundTatAppealEnabled, userID);

                return Ok(new { message = "Outbound TAT settings saved." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ── GET api/tat/outbound/windows ──────────────────────────────────────────
        // All configured outbound TAT windows (admin only)
        [HttpGet("outbound/windows")]
        public IActionResult GetOutboundWindows()
        {
            try
            {
                if (!IsAdmin) return RequireAdmin();

                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);
                var windows = master.TatOutbound.GetWindows();

                return Ok(windows.Select(w => new
                {
                    id = w.Id,
                    windowStart = w.WindowStart.ToString(@"hh\:mm"),
                    windowEnd = w.WindowEnd.ToString(@"hh\:mm"),
                    scheduleType = w.ScheduleType,
                    isActive = w.IsActive,
                    createdBy = w.CreatedBy,
                    created = w.Created,
                    updatedBy = w.UpdatedBy,
                    updated = w.Updated
                }).ToList());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ── PUT api/tat/outbound/windows ──────────────────────────────────────────
        // Upsert the full list of outbound windows (admin only)
        // Send Id=0 for new windows, existing Id for updates
        [HttpPut("outbound/windows")]
        public IActionResult UpsertOutboundWindows([FromBody] List<OutboundWindowUpsertRequest> requests)
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
                    return BadRequest(new { message = "No windows provided." });

                // Validate each window
                foreach (var r in requests)
                {
                    if (!TimeSpan.TryParse(r.WindowStart, out var start) ||
                        !TimeSpan.TryParse(r.WindowEnd, out var end))
                        return BadRequest(new { message = $"Invalid time format for window {r.Id}." });

                    if (end <= start)
                        return BadRequest(new { message = $"Window end must be after window start for window {r.Id}." });

                    if (r.ScheduleType != "Weekday" && r.ScheduleType != "Sunday")
                        return BadRequest(new { message = $"ScheduleType must be 'Weekday' or 'Sunday' for window {r.Id}." });
                }

                using var master = new MasterService(branch);
                master.TatOutbound.UpsertWindows(requests.Select(r => new Model.SETSDB.Tat_Outbound_Window
                {
                    Id = r.Id,
                    WindowStart = TimeSpan.Parse(r.WindowStart),
                    WindowEnd = TimeSpan.Parse(r.WindowEnd),
                    ScheduleType = r.ScheduleType,
                    IsActive = r.IsActive
                }).ToList(), userID);

                return Ok(new { message = "Outbound TAT windows saved." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ── DELETE api/tat/outbound/windows/{id} ──────────────────────────────────
        // Delete a single outbound window (admin only)
        [HttpDelete("outbound/windows/{id}")]
        public IActionResult DeleteOutboundWindow(int id)
        {
            try
            {
                if (!IsAdmin) return RequireAdmin();

                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);
                master.TatOutbound.DeleteWindow(id);

                return Ok(new { message = "Window deleted." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ── GET api/tat/outbound/current-window ───────────────────────────────────
        // Returns the current window status for the endorser's live display
        // Accessible by all authenticated users (not admin-only)
        [HttpGet("outbound/current-window")]
        public IActionResult GetCurrentWindowStatus()
        {
            try
            {
                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                var now = DateTime.Now;

                using var master = new MasterService(branch);

                if (!master.TatOutbound.IsOutboundTatEnabled())
                    return Ok(new { enabled = false });

                var todayWindows = master.TatOutbound.GetTodayWindows(now);
                var currentWindow = master.TatOutbound.GetCurrentWindow(now);
                var nextWindow = master.TatOutbound.GetNextWindow(now);
                var hasEndorsedThisWindow = currentWindow != null &&
                                master.TatOutbound.HasEndorsedInWindow(currentWindow, now);

                return Ok(new
                {
                    enabled = true,
                    appealEnabled = master.TatOutbound.IsAppealEnabled(),
                    currentWindow = currentWindow == null ? null : new
                    {
                        id = currentWindow.Id,
                        windowStart = currentWindow.WindowStart.ToString(@"hh\:mm"),
                        windowEnd = currentWindow.WindowEnd.ToString(@"hh\:mm"),
                        windowEndFull = now.Date.Add(currentWindow.WindowEnd)
                    },
                    nextWindow = nextWindow == null ? null : new
                    {
                        id = nextWindow.Id,
                        windowStart = nextWindow.WindowStart.ToString(@"hh\:mm"),
                        windowEnd = nextWindow.WindowEnd.ToString(@"hh\:mm"),
                        windowStartFull = now.Date.Add(nextWindow.WindowStart)
                    },
                    hasWindowsToday = todayWindows.Count > 0,
                    hasEndorsedThisWindow, 
                    serverTime = now
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ── GET api/tat/outbound/logs ─────────────────────────────────────────────
        // Returns outbound TAT window logs for audit trail
        // Admin: all logs. Endorser: same (outbound TAT is branch-level, not section-level)
        [HttpGet("outbound/logs")]
        public IActionResult GetOutboundLogs([FromQuery] string? dateFrom,
                                              [FromQuery] string? dateTo)
        {
            try
            {
                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                var from = DateTime.TryParse(dateFrom, out var df) ? df : DateTime.Today;
                var to = DateTime.TryParse(dateTo, out var dt) ? dt : DateTime.Today;

                using var master = new MasterService(branch);

                var windows = master.TatOutbound.GetWindows()
                    .ToDictionary(w => w.Id);

                var logs = master.TatOutbound.GetLogs(from, to);

                var result = logs.Select(l => new
                {
                    id = l.Id,
                    windowId = l.WindowId,
                    scheduleType = windows.TryGetValue(l.WindowId, out var w) ? w.ScheduleType : "—",
                    windowDate = l.WindowDate,
                    windowStart = l.WindowStart,
                    windowEnd = l.WindowEnd,
                    batchNos = l.BatchNos,
                    result = l.Result,
                    appealedBy = l.AppealedBy,
                    appealedAt = l.AppealedAt,
                    created = l.Created
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ── POST api/tat/outbound/logs/{logId}/appeal ─────────────────────────────
        // Appeal a missed outbound TAT window
        [HttpPost("outbound/logs/{logId}/appeal")]
        public IActionResult AppealOutboundLog(int logId)
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

                // Check appeal is enabled for this branch
                if (!master.TatOutbound.IsAppealEnabled())
                    return BadRequest(new { message = "Appeals are not enabled for this branch." });

                var log = master.TatOutbound.GetLogById(logId);
                if (log == null)
                    return NotFound(new { message = "Log entry not found." });

                if (log.Result != "Missed")
                    return BadRequest(new { message = "Only missed windows can be appealed." });

                master.TatOutbound.Appeal(logId, userID, DateTime.Now);
                return Ok(new { message = "Appeal recorded." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }

}