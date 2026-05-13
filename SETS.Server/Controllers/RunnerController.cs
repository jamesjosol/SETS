using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Model.Main;
using Service;
using SETS.Server.Hubs;

namespace SETS.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RunnerController : ControllerBase
    {
        private readonly IHubContext<NotificationHub> _hub;

        private string? Branch => HttpContext.Session.GetString("BranchCode");

        public RunnerController(IHubContext<NotificationHub> hub)
        {
            _hub = hub;
        }

        // POST api/runner/scan
        // Scans a specimen barcode on the Assign RMT page.
        // Stamps section receipt on first scan, returns header + tests on all scans.
        [HttpPost("scan")]
        public IActionResult ScanSpecimen([FromBody] ScanSpecimenRequest request)
        {
            try
            {
                var branch = HttpContext.Session.GetString("BranchCode");
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                if (string.IsNullOrEmpty(request.SpecimenNo))
                    return BadRequest(new { message = "Specimen number is required." });

                if (string.IsNullOrEmpty(request.SectionCode))
                    return BadRequest(new { message = "Section code is required." });

                if (string.IsNullOrEmpty(request.UserID))
                    return BadRequest(new { message = "User ID is required." });

                using var master = new MasterService(branch);
                var result = master.Runner.ScanSpecimen(request);

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(400, new { message = ex.Message });
            }
        }

        // POST api/runner/assign
        // Saves RMT assignments and schedule tags for a list of tests.
        [HttpPost("assign")]
        public async Task<IActionResult> SaveAssignments([FromBody] SaveAssignmentsRequest request)
        {
            try
            {
                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                if (string.IsNullOrEmpty(request.UserID))
                    return BadRequest(new { message = "User ID is required." });

                if (request.Assignments == null || !request.Assignments.Any())
                    return BadRequest(new { message = "No assignments provided." });

                using var master = new MasterService(branch);
                var result = master.Runner.SaveAssignments(request);

                // ── Notification: SPECIMEN_COMPLETED → section runners ────────
                foreach (var completed in result.CompletedHeaders)
                {
                    try
                    {
                        var notif = master.Notification.Create(new CreateNotificationRequest
                        {
                            NotifType = NotifType.SpecimenCompleted,
                            Title = "Specimen Completed",
                            Message = $"Specimen {completed.SpecimenNo} has been completed in your section.",
                            TargetCategory = "3",
                            TargetSection = completed.SectionCode,
                            ReferenceID = completed.SpecimenNo
                        });

                        await _hub.Clients
                            .Group($"notif-{branch}-sec-{completed.SectionCode}")
                            .SendAsync("NewNotification", new
                            {
                                notif.NotifID,
                                notif.NotifType,
                                notif.Title,
                                notif.Message,
                                notif.IsRead,
                                notif.ReferenceID,
                                notif.CreatedAt
                            });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[NOTIF] SPECIMEN_COMPLETED failed for {completed.SpecimenNo}: {ex.Message}");
                    }
                }

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(400, new { message = ex.Message });
            }
        }
        // GET api/runner/pending?sectionCode=HEMA
        [HttpGet("pending")]
        public IActionResult GetPendingSpecimens([FromQuery] string sectionCode)
        {
            try
            {
                var branch = HttpContext.Session.GetString("BranchCode");
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                if (string.IsNullOrEmpty(sectionCode))
                    return BadRequest(new { message = "Section code is required." });

                using var master = new MasterService(branch);
                return Ok(master.Runner.GetPendingSpecimens(sectionCode));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // GET api/runner/scheduled?sectionCode=HEMA
        [HttpGet("scheduled")]
        public IActionResult GetScheduledSpecimens([FromQuery] string sectionCode)
        {
            try
            {
                var branch = HttpContext.Session.GetString("BranchCode");
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                if (string.IsNullOrEmpty(sectionCode))
                    return BadRequest(new { message = "Section code is required." });

                using var master = new MasterService(branch);
                return Ok(master.Runner.GetScheduledSpecimens(sectionCode));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // GET api/runner/tests/{headerId}?isOnSite=false
        [HttpGet("tests/{headerId}")]
        public IActionResult GetTestsByHeader(int headerId, [FromQuery] bool isOnSite = false)
        {
            try
            {
                var branch = HttpContext.Session.GetString("BranchCode");
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);

                if (isOnSite)
                    return Ok(master.OnSite.GetTestsByHeaderId(headerId));

                return Ok(master.Runner.GetTestsByHeader(headerId));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // GET api/runner/running?sectionCode=HEMA
        [HttpGet("running")]
        public IActionResult GetRunningSpecimens([FromQuery] string sectionCode, [FromQuery] bool allUsers = false)
        {
            try
            {
                var branch = HttpContext.Session.GetString("BranchCode");
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                var userID = allUsers ? null : HttpContext.Session.GetString("UserID");

                if (string.IsNullOrEmpty(sectionCode))
                    return BadRequest(new { message = "Section code is required." });

                using var master = new MasterService(branch);
                return Ok(master.Runner.GetRunningSpecimens(sectionCode, userID));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // GET api/runner/dashboard-summary?sectionCode=HEMA
        [HttpGet("dashboard-summary")]
        public IActionResult GetDashboardSummary([FromQuery] string sectionCode)
        {
            try
            {
                var branch = HttpContext.Session.GetString("BranchCode");
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                var userID = HttpContext.Session.GetString("UserID");
                if (string.IsNullOrEmpty(userID))
                    return Unauthorized(new { message = "Session expired." });

                if (string.IsNullOrEmpty(sectionCode))
                    return BadRequest(new { message = "Section code is required." });

                using var master = new MasterService(branch);
                return Ok(master.Runner.GetDashboardSummary(sectionCode, userID));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // GET api/runner/dashboard-summary/all  — admin only
        [HttpGet("dashboard-summary/all")]
        public IActionResult GetAllSectionsSummary()
        {
            try
            {
                var branch = HttpContext.Session.GetString("BranchCode");
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);
                return Ok(master.Runner.GetAllSectionsSummary());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // GET api/runner/completed-today?sectionCode=
        [HttpGet("completed-today")]
        public IActionResult GetCompletedToday([FromQuery] string sectionCode)
        {
            try
            {
                var branch = HttpContext.Session.GetString("BranchCode");
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);
                return Ok(master.Runner.GetCompletedToday(sectionCode));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("admin/running")]
        public IActionResult GetAllSectionsRunning()
        {
            try
            {
                var branch = HttpContext.Session.GetString("BranchCode");
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);
                return Ok(master.Runner.GetAllSectionsRunning());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("admin/recently-routed")]
        public IActionResult GetAllSectionsRecentlyRouted()
        {
            try
            {
                var branch = HttpContext.Session.GetString("BranchCode");
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);
                return Ok(master.Runner.GetAllSectionsRecentlyRouted());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("admin/due-today")]
        public IActionResult GetAllSectionsDueToday()
        {
            try
            {
                var branch = HttpContext.Session.GetString("BranchCode");
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);
                return Ok(master.Runner.GetAllSectionsDueToday());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("admin/completed-today")]
        public IActionResult GetAllSectionsCompletedToday()
        {
            try
            {
                var branch = HttpContext.Session.GetString("BranchCode");
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);
                return Ok(master.Runner.GetAllSectionsCompletedToday());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // GET api/runner/admin/pending
        [HttpGet("admin/pending")]
        public IActionResult GetAllSectionsPending()
        {
            try
            {
                var branch = HttpContext.Session.GetString("BranchCode");
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);
                return Ok(master.Runner.GetAllSectionsPending());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("admin/scheduled")]
        public IActionResult GetAllSectionsScheduled()
        {
            try
            {
                var branch = HttpContext.Session.GetString("BranchCode");
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);
                return Ok(master.Runner.GetAllSectionsScheduled());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // GET api/runner/completed?from=2026-04-01&to=2026-04-28
        [HttpGet("completed")]
        public IActionResult GetCompletedSpecimens([FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            try
            {
                var branch = HttpContext.Session.GetString("BranchCode");
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                var sectionCode = HttpContext.Session.GetString("SectionCode");
                if (string.IsNullOrEmpty(sectionCode))
                    return BadRequest(new { message = "Section not found in session." });

                using var master = new MasterService(branch);
                var result = master.Runner.GetCompletedSpecimens(sectionCode, from, to);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // GET api/runner/admin/completed?from=2026-04-01&to=2026-04-28
        [HttpGet("admin/completed")]
        public IActionResult GetAllSectionsCompleted([FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            try
            {
                var branch = HttpContext.Session.GetString("BranchCode");
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);
                var result = master.Runner.GetAllSectionsCompleted(from, to);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────────────────────────────────────────────────── ON-SITE ENDPOINTS ─────────────────────────────────────────────────────────────────────────────────────────

        // POST api/runner/onsite/scan
        [HttpPost("onsite/scan")]
        public async Task<IActionResult> ScanOnSiteSpecimen([FromBody] ScanOnSiteSpecimenRequest request)
        {
            try
            {
                var branch = HttpContext.Session.GetString("BranchCode");
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                if (string.IsNullOrEmpty(request.SpecimenNo))
                    return BadRequest(new { message = "Specimen number is required." });

                if (string.IsNullOrEmpty(request.SectionCode))
                    return BadRequest(new { message = "Section code is required." });

                if (string.IsNullOrEmpty(request.UserID))
                    return BadRequest(new { message = "User ID is required." });

                using var master = new MasterService(branch);
                var result = await master.OnSite.ScanSpecimen(request);

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(400, new { message = ex.Message });
            }
        }

        // POST api/runner/onsite/assign
        [HttpPost("onsite/assign")]
        public IActionResult SaveOnSiteAssignments([FromBody] SaveAssignmentsRequest request)
        {
            try
            {
                var branch = HttpContext.Session.GetString("BranchCode");
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                if (string.IsNullOrEmpty(request.UserID))
                    return BadRequest(new { message = "User ID is required." });

                if (request.Assignments == null || !request.Assignments.Any())
                    return BadRequest(new { message = "No assignments provided." });

                using var master = new MasterService(branch);
                master.OnSite.SaveAssignments(request);

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(400, new { message = ex.Message });
            }
        }

        // GET api/runner/onsite/settings
        [HttpGet("onsite/settings")]
        public IActionResult GetOnSiteSettings()
        {
            try
            {
                var branch = HttpContext.Session.GetString("BranchCode");
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);
                var settings = master.OnSiteSettings.GetSettings();
                var labNos = master.OnSiteSettings.GetAllAllowedLabNos();

                return Ok(new
                {
                    isEnabled = settings?.IsEnabled ?? false,
                    allowedLabNos = labNos.Select(l => new
                    {
                        id = l.Id,
                        prefix = l.Prefix,
                        description = l.Description,
                        active = l.Active,
                        created = l.Created,
                        createdBy = l.CreatedBy,
                    })
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // PATCH api/runner/onsite/settings/toggle
        [HttpPatch("onsite/settings/toggle")]
        public IActionResult ToggleOnSite([FromBody] ToggleOnSiteRequest request)
        {
            try
            {
                var branch = HttpContext.Session.GetString("BranchCode");
                var userID = HttpContext.Session.GetString("UserID");
                var isAdmin = HttpContext.Session.GetString("IsAdmin") == "True";

                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                if (!isAdmin)
                    return Unauthorized(new { message = "Administrator access required." });

                using var master = new MasterService(branch);
                master.OnSiteSettings.SetEnabled(request.IsEnabled, userID);

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // POST api/runner/onsite/allowed-labnos
        [HttpPost("onsite/allowed-labnos")]
        public IActionResult AddAllowedLabNo([FromBody] AddAllowedLabNoRequest request)
        {
            try
            {
                var branch = HttpContext.Session.GetString("BranchCode");
                var userID = HttpContext.Session.GetString("UserID");
                var isAdmin = HttpContext.Session.GetString("IsAdmin") == "True";

                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                if (!isAdmin)
                    return Unauthorized(new { message = "Administrator access required." });

                if (string.IsNullOrEmpty(request.Prefix))
                    return BadRequest(new { message = "Prefix is required." });

                using var master = new MasterService(branch);
                master.OnSiteSettings.AddAllowedLabNo(request.Prefix, request.Description, userID);

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(400, new { message = ex.Message });
            }
        }

        // PATCH api/runner/onsite/allowed-labnos/{id}/toggle
        [HttpPatch("onsite/allowed-labnos/{id}/toggle")]
        public IActionResult ToggleAllowedLabNo(int id)
        {
            try
            {
                var branch = HttpContext.Session.GetString("BranchCode");
                var userID = HttpContext.Session.GetString("UserID");
                var isAdmin = HttpContext.Session.GetString("IsAdmin") == "True";

                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                if (!isAdmin)
                    return Unauthorized(new { message = "Administrator access required." });

                using var master = new MasterService(branch);
                master.OnSiteSettings.ToggleAllowedLabNo(id, userID);

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(400, new { message = ex.Message });
            }
        }

        // DELETE api/runner/onsite/allowed-labnos/{id}
        [HttpDelete("onsite/allowed-labnos/{id}")]
        public IActionResult DeleteAllowedLabNo(int id)
        {
            try
            {
                var branch = HttpContext.Session.GetString("BranchCode");
                var isAdmin = HttpContext.Session.GetString("IsAdmin") == "True";

                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                if (!isAdmin)
                    return Unauthorized(new { message = "Administrator access required." });

                using var master = new MasterService(branch);
                master.OnSiteSettings.DeleteAllowedLabNo(id);

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(400, new { message = ex.Message });
            }
        }

        // POST api/runner/cancel-specimen
        // Cancels an entire specimen (header + all non-released tests) from the runner/section side.
        // Restricted to Team Lead (RoleID == 2) and Admin.
        [HttpPost("cancel-specimen")]
        public IActionResult CancelSpecimen([FromBody] CancelSectionSpecimenRequest request)
        {
            try
            {
                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                var roleID = HttpContext.Session.GetInt32("RoleID") ?? 0;
                var isAdmin = HttpContext.Session.GetString("IsAdmin") == "True";
                if (roleID < 2 && !isAdmin)
                    return Unauthorized(new { message = "Team Lead or Administrator access required." });

                if (string.IsNullOrEmpty(request.SpecimenNo))
                    return BadRequest(new { message = "Specimen number is required." });

                if (string.IsNullOrEmpty(request.SectionCode))
                    return BadRequest(new { message = "Section code is required." });

                if (string.IsNullOrWhiteSpace(request.Reason))
                    return BadRequest(new { message = "A cancellation reason is required." });

                if (string.IsNullOrEmpty(request.UserID))
                    return BadRequest(new { message = "User ID is required." });

                using var master = new MasterService(branch);

                if (request.IsOnSite)
                    master.OnSite.CancelSpecimen(request);
                else
                    master.Runner.CancelSpecimen(request);

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(400, new { message = ex.Message });
            }
        }

        // POST api/runner/abort-test
        // Aborts a single running test back to pending.
        // Available to all users (no role restriction).
        [HttpPost("abort-test")]
        public IActionResult AbortRunningTest([FromBody] AbortRunningTestRequest request)
        {
            try
            {
                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                if (request.TestId <= 0)
                    return BadRequest(new { message = "Test ID is required." });

                if (request.HeaderId <= 0)
                    return BadRequest(new { message = "Header ID is required." });

                if (string.IsNullOrEmpty(request.SpecimenNo))
                    return BadRequest(new { message = "Specimen number is required." });

                if (string.IsNullOrEmpty(request.SectionCode))
                    return BadRequest(new { message = "Section code is required." });

                if (string.IsNullOrWhiteSpace(request.Reason))
                    return BadRequest(new { message = "An abort reason is required." });

                if (string.IsNullOrEmpty(request.UserID))
                    return BadRequest(new { message = "User ID is required." });

                using var master = new MasterService(branch);

                if (request.IsOnSite)
                    master.OnSite.AbortRunningTest(request);
                else
                    master.Runner.AbortRunningTest(request);

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(400, new { message = ex.Message });
            }
        }
    }
}