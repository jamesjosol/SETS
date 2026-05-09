using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Model.HCLAB;
using Model.Main;
using Service;
using SETS.Server.Hubs;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BatchController : ControllerBase
    {
        private readonly IHubContext<NotificationHub> _hub;

        private string? Branch => HttpContext.Session.GetString("BranchCode");
        private string? UserID => HttpContext.Session.GetString("UserID");
        private string? SectionCode => HttpContext.Session.GetString("SectionCode");

        public BatchController(IHubContext<NotificationHub> hub)
        {
            _hub = hub;
        }

        [HttpPost("endorse")]
        public async Task<IActionResult> Endorse([FromBody] EndorseRequest request)
        {
            try
            {
                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                if (string.IsNullOrEmpty(request.SectionCode))
                    return BadRequest(new { message = "Section code is required." });

                if (string.IsNullOrEmpty(request.UserID))
                    return BadRequest(new { message = "User ID is required." });

                if (request.Specimens.Count == 0 && request.NonBarcoded.Count == 0)
                    return BadRequest(new { message = "No items to endorse." });

                using var master = new MasterService(branch);

                var processingSection = master.Section
                    .GetByBranch(branch)
                    .FirstOrDefault(s => s.Category == "2" && s.Active);

                if (processingSection == null)
                    return StatusCode(500, new { message = "No active Processing section found for this branch." });

                request.ProcDestination = processingSection.Code;

                var result = master.Batch.Endorse(request);

                // ── Notification: BATCH_ENDORSED → all receivers ──────────────
                try
                {
                    var notif = master.Notification.Create(new CreateNotificationRequest
                    {
                        NotifType = NotifType.BatchEndorsed,
                        Title = "New Batch Endorsed",
                        Message = $"Batch {result.BatchNo} endorsed from {result.LocationName}.",
                        TargetCategory = "2",
                        ReferenceID = result.BatchNo
                    });

                    await _hub.Clients
                        .Group($"notif-{branch}-cat-2")
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
                    Console.WriteLine($"[NOTIF] BATCH_ENDORSED failed for {result.BatchNo}: {ex.Message}");
                }

                return Ok(new { success = true, batchNo = result.BatchNo });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("checkspecimen/{specimenNo}")]
        public IActionResult CheckSpecimen(string specimenNo)
        {
            try
            {
                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);
                var existing = master.Batch.CheckSpecimen(specimenNo);

                return Ok(new
                {
                    previouslyEndorsed = existing != null,
                    batchNo = existing?.BatchNo,
                    endorsedOn = existing?.Endorsed,
                    endorsedBy = existing?.EndorsedBy
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // GET api/batch/dashboard-summary?sectionCode=WP
        [HttpGet("dashboard-summary")]
        public IActionResult GetDashboardSummary([FromQuery] string sectionCode)
        {
            try
            {
                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });
                if (string.IsNullOrEmpty(sectionCode))
                    return BadRequest(new { message = "Section code is required." });

                using var master = new MasterService(branch);
                var summary = master.Batch.GetDashboardSummary(sectionCode, DateTime.Today);

                return Ok(summary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // GET api/batch/dashboard-summary/all
        // Admin only — summary for all sections in the current branch
        [HttpGet("dashboard-summary/all")]
        public IActionResult GetAllSectionsSummary()
        {
            try
            {
                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);
                var summary = master.Batch.GetAllSectionsSummary(DateTime.Today);

                return Ok(summary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // GET api/batch/recent?sectionCode=WP
        // Regular / Team Lead — top 5 recent batches for their section
        [HttpGet("recent")]
        public IActionResult GetRecentBatches([FromQuery] string sectionCode)
        {
            try
            {
                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });
                if (string.IsNullOrEmpty(sectionCode))
                    return BadRequest(new { message = "Section code is required." });

                using var master = new MasterService(branch);
                var items = master.Batch.GetRecentBatches(sectionCode);

                return Ok(items);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // GET api/batch/recent/all
        // Admin — top 5 consolidated recent batches across all endorser sections
        [HttpGet("recent/all")]
        public IActionResult GetAllSectionsRecentBatches()
        {
            try
            {
                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);
                var items = master.Batch.GetAllSectionsRecentBatches();

                return Ok(items);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // GET api/batch/{batchNo}/detail
        // Batch detail for drawer — header + specimens + non-barcoded
        [HttpGet("{batchNo}/detail")]
        public IActionResult GetBatchDetail(string batchNo)
        {
            try
            {
                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);
                var detail = master.Batch.GetBatchDetail(batchNo);

                if (detail == null)
                    return NotFound(new { message = $"Batch '{batchNo}' not found." });

                return Ok(detail);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // GET api/batch/weekly-flow?sectionCode=WP
        // Regular / Team Lead — weekly batch flow for their section
        [HttpGet("weekly-flow")]
        public IActionResult GetWeeklyFlow([FromQuery] string sectionCode)
        {
            try
            {
                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });
                if (string.IsNullOrEmpty(sectionCode))
                    return BadRequest(new { message = "Section code is required." });

                using var master = new MasterService(branch);
                var flow = master.Batch.GetWeeklyFlow(sectionCode);

                return Ok(flow);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // GET api/batch/weekly-flow/all
        // Admin — consolidated weekly flow across all endorser sections
        [HttpGet("weekly-flow/all")]
        public IActionResult GetAllSectionsWeeklyFlow()
        {
            try
            {
                var branch = HttpContext.Session.GetString("BranchCode");
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);
                var flow = master.Batch.GetAllSectionsWeeklyFlow();

                return Ok(flow);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // GET api/batch/endorsements?sectionCode=WP
        // Regular / TL — all endorsements for their section (archive)
        [HttpGet("endorsements")]
        public IActionResult GetEndorsements([FromQuery] string sectionCode,
                                      [FromQuery] string dateFrom,
                                      [FromQuery] string dateTo)
        {
            try
            {
                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });
                if (string.IsNullOrEmpty(sectionCode))
                    return BadRequest(new { message = "Section code is required." });

                var from = DateTime.TryParse(dateFrom, out var df) ? df : DateTime.Today.AddDays(-30);
                var to = DateTime.TryParse(dateTo, out var dt) ? dt : DateTime.Today;

                using var master = new MasterService(branch);
                return Ok(master.Batch.GetEndorsements(sectionCode, from, to));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("endorsements/all")]
        public IActionResult GetAllEndorsements([FromQuery] string dateFrom,
                                                [FromQuery] string dateTo)
        {
            try
            {
                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                var from = DateTime.TryParse(dateFrom, out var df) ? df : DateTime.Today.AddDays(-30);
                var to = DateTime.TryParse(dateTo, out var dt) ? dt : DateTime.Today;

                using var master = new MasterService(branch);
                return Ok(master.Batch.GetAllEndorsements(from, to));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // GET api/batch/search?q=2654070
        [HttpGet("search")]
        public IActionResult GlobalSearch([FromQuery] string q)
        {
            try
            {
                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 3)
                    return BadRequest(new { message = "Minimum 3 characters required." });

                var userID = UserID ?? "";
                var category = HttpContext.Session.GetString("SectionCategory") ?? "";
                var sectionCode = SectionCode ?? "";
                var isAdmin = HttpContext.Session.GetString("IsAdmin") == "True";

                // Admin bypasses all role scoping
                if (isAdmin) category = "admin";

                using var master = new MasterService(branch);
                var results = master.Batch.GlobalSearch(q.Trim(), userID, category, sectionCode);

                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}

