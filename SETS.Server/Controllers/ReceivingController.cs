using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Model.Main;
using Service;
using SETS.Server.Hubs;

namespace SETS.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReceivingController : ControllerBase
    {
        private readonly IHubContext<NotificationHub> _hub;

        private string? Branch => HttpContext.Session.GetString("BranchCode");
        private string? UserID => HttpContext.Session.GetString("UserID");
        private string? SectionCode => HttpContext.Session.GetString("SectionCode");

        public ReceivingController(IHubContext<NotificationHub> hub)
        {
            _hub = hub;
        }

        // POST api/receiving/specimen
        [HttpPost("specimen")]
        public async Task<IActionResult> ReceiveSpecimen([FromBody] ReceiveSpecimenRequest request)
        {
            try
            {
                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                if (string.IsNullOrEmpty(request.SpecimenNo))
                    return BadRequest(new { message = "Specimen number is required." });

                if (string.IsNullOrEmpty(request.UserID))
                    return BadRequest(new { message = "User ID is required." });

                using var master = new MasterService(branch);
                var result = await master.Receiving.ReceiveSpecimen(request);

                // ── Notification: BATCH_RECEIVED → specific endorser ──────────
                if (result.BatchStatus == "C")
                {
                    try
                    {
                        var notif = master.Notification.Create(new CreateNotificationRequest
                        {
                            NotifType = NotifType.BatchReceived,
                            Title = "Batch Fully Received",
                            Message = $"Batch {result.BatchNo} from {result.LocationName} has been fully received.",
                            TargetUserID = result.EndorsedBy,
                            ReferenceID = result.BatchNo
                        });

                        await _hub.Clients
                            .Group($"notif-user-{result.EndorsedBy}")
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
                        Console.WriteLine($"[NOTIF] BATCH_RECEIVED failed for {result.BatchNo}: {ex.Message}");
                    }
                }

                // ── Notification: SPECIMEN_ARRIVED → section runners ──────────
                if (!string.IsNullOrEmpty(result.RoutedSectionCode))
                {
                    try
                    {
                        var notif = master.Notification.Create(new CreateNotificationRequest
                        {
                            NotifType = NotifType.SpecimenArrived,
                            Title = "New Specimen Arrived",
                            Message = $"Specimen {result.SpecimenNo} has arrived in your section.",
                            TargetCategory = "3",
                            TargetSection = result.RoutedSectionCode,
                            ReferenceID = result.SpecimenNo
                        });

                        await _hub.Clients
                            .Group($"notif-{branch}-sec-{result.RoutedSectionCode}")
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
                        Console.WriteLine($"[NOTIF] SPECIMEN_ARRIVED failed for {result.SpecimenNo}: {ex.Message}");
                    }
                }

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(400, new { message = ex.Message });
            }
        }

        // GET api/receiving/check/{specimenNo}?currentBatchNo=WES26-00001
        [HttpGet("check/{specimenNo}")]
        public IActionResult CheckSpecimen(string specimenNo, [FromQuery] string? currentBatchNo)
        {
            try
            {
                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);
                var result = master.Receiving.CheckSpecimen(specimenNo, currentBatchNo);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(400, new { message = ex.Message });
            }
        }

        // PATCH api/receiving/specimen-remarks
        [HttpPatch("specimen-remarks")]
        public IActionResult UpdateSpecimenRemarks([FromBody] UpdateSpecimenRemarksRequest request)
        {
            try
            {
                var branch = HttpContext.Session.GetString("BranchCode");
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);
                master.Receiving.UpdateSpecimenReceivingRemarks(
                    request.SpecimenNo,
                    request.BatchNo,
                    request.ReceivingRemarks
                );

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(400, new { message = ex.Message });
            }
        }

        // PATCH api/receiving/batch-temp
        [HttpPatch("batch-temp")]
        public IActionResult UpdateBatchTemp([FromBody] UpdateBatchTempRequest request)
        {
            try
            {
                var branch = HttpContext.Session.GetString("BranchCode");
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                if (string.IsNullOrEmpty(request.BatchNo))
                    return BadRequest(new { message = "Batch number is required." });

                using var master = new MasterService(branch);
                master.Receiving.UpdateBatchTemp(request);

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(400, new { message = ex.Message });
            }
        }

        // PATCH api/receiving/specimen-cancel
        [HttpPatch("specimen-cancel")]
        public async Task<IActionResult> CancelSpecimen([FromBody] CancelSpecimenRequest request)
        {
            try
            {
                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                if (string.IsNullOrEmpty(request.SpecimenNo))
                    return BadRequest(new { message = "Specimen number is required." });

                if (string.IsNullOrEmpty(request.BatchNo))
                    return BadRequest(new { message = "Batch number is required." });

                if (string.IsNullOrEmpty(request.CancelReason))
                    return BadRequest(new { message = "Cancel reason is required." });

                if (string.IsNullOrEmpty(request.UserID))
                    return BadRequest(new { message = "User ID is required." });

                using var master = new MasterService(branch);
                var cancelResult = master.Receiving.CancelSpecimen(request);

                // ── Notification: SPECIMEN_CANCELLED → specific endorser ───────
                try
                {
                    var notif = master.Notification.Create(new CreateNotificationRequest
                    {
                        NotifType = NotifType.SpecimenCancelled,
                        Title = "Specimen Cancelled",
                        Message = $"Specimen {request.SpecimenNo} from batch {request.BatchNo} was cancelled. Reason: {request.CancelReason}",
                        TargetUserID = cancelResult.EndorsedBy,
                        ReferenceID = request.SpecimenNo
                    });

                    await _hub.Clients
                        .Group($"notif-user-{cancelResult.EndorsedBy}")
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
                    Console.WriteLine($"[NOTIF] SPECIMEN_CANCELLED failed for {request.SpecimenNo}: {ex.Message}");
                }

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(400, new { message = ex.Message });
            }
        }

        // POST api/receiving/cancel-nonbarcoded
        [HttpPost("cancel-nonbarcoded")]
        public IActionResult CancelNonBarcodedItem([FromBody] CancelNonBarcodedRequest request)
        {
            try
            {
                var branch = HttpContext.Session.GetString("BranchCode");
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                if (request.ItemID <= 0)
                    return BadRequest(new { message = "Item ID is required." });

                if (string.IsNullOrEmpty(request.BatchNo))
                    return BadRequest(new { message = "Batch number is required." });

                if (string.IsNullOrEmpty(request.CancelReason))
                    return BadRequest(new { message = "Cancel reason is required." });

                if (string.IsNullOrEmpty(request.UserID))
                    return BadRequest(new { message = "User ID is required." });

                using var master = new MasterService(branch);
                master.Receiving.CancelNonBarcodedItem(request);

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(400, new { message = ex.Message });
            }
        }

        // POST api/receiving/nonbarcoded
        [HttpPost("nonbarcoded")]
        public IActionResult ReceiveNonBarcoded([FromBody] ReceiveNonBarcodedRequest request)
        {
            try
            {
                var branch = HttpContext.Session.GetString("BranchCode");
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                if (string.IsNullOrEmpty(request.UserID))
                    return BadRequest(new { message = "User ID is required." });

                if (request.ItemIDs == null || request.ItemIDs.Count == 0)
                    return BadRequest(new { message = "No items selected." });

                using var master = new MasterService(branch);
                var result = master.Receiving.ReceiveNonBarcoded(request);

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(400, new { message = ex.Message });
            }
        }


        // PATCH api/receiving/nonbarcoded-remarks
        [HttpPatch("nonbarcoded-remarks")]
        public IActionResult UpdateNonBarcodedRemarks([FromBody] UpdateNonBarcodedRemarksRequest request)
        {
            try
            {
                var branch = HttpContext.Session.GetString("BranchCode");
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);
                master.Receiving.UpdateNonBarcodedReceivingRemarks(
                    request.ItemID,
                    request.ReceivingRemarks
                );

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(400, new { message = ex.Message });
            }
        }

        // GET api/receiving/pending-nonbarcoded?sectionCode=WP
        // Loads all pending non-barcoded items for the processing section
        [HttpGet("pending-nonbarcoded")]
        public IActionResult GetPendingNonBarcoded([FromQuery] string sectionCode)
        {
            try
            {
                var branch = HttpContext.Session.GetString("BranchCode");
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);

                var resolvedCode = ResolveProcessingSection(master, branch, sectionCode);
                if (resolvedCode == null)
                    return StatusCode(500, new { message = "No active Processing section found for this branch." });

                var result = master.Receiving.GetPendingNonBarcoded(resolvedCode);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }


        // GET api/receiving/dashboard-summary?sectionCode=WP
        [HttpGet("dashboard-summary")]
        public IActionResult GetDashboardSummary([FromQuery] string sectionCode)
        {
            try
            {
                var branch = HttpContext.Session.GetString("BranchCode");
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);

                // Resolve processing section code
                var resolvedCode = ResolveProcessingSection(master, branch, sectionCode);
                if (resolvedCode == null)
                    return StatusCode(500, new { message = "No active Processing section found for this branch." });

                var summary = master.Receiving.GetReceiverDashboardSummary(resolvedCode, DateTime.Today);
                return Ok(summary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // GET api/receiving/monitoring?sectionCode=WP
        [HttpGet("monitoring")]
        public IActionResult GetMonitoringDashboard([FromQuery] string sectionCode)
        {
            try
            {
                var branch = HttpContext.Session.GetString("BranchCode");
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);

                // Resolve processing section code
                var resolvedCode = ResolveProcessingSection(master, branch, sectionCode);
                if (resolvedCode == null)
                    return StatusCode(500, new { message = "No active Processing section found for this branch." });

                var data = master.Receiving.GetMonitoringDashboard(resolvedCode, DateTime.Today);
                var procTat = master.Tat.GetProcessingTat();
                return Ok(new
                {
                    thresholdMins = procTat != null ? procTat.Hours * 60 + procTat.Minutes : (int?)null,
                    sections = data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ── Helper ─────────────────────────────────────────────────────────────────

        private static string? ResolveProcessingSection(MasterService master, string branch, string sectionCode)
        {
            // If the passed sectionCode belongs to a processing section (category 2), use it directly
            var section = master.Section.GetByCode(sectionCode);
            if (section != null && section.Category == "2")
                return sectionCode;

            // Otherwise (admin logged in under a non-processing section),
            // auto-resolve the active processing section for the branch
            return master.Section
                .GetByBranch(branch)
                .FirstOrDefault(s => s.Category == "2" && s.Active)
                ?.Code;
        }

        // GET api/receiving/weekly-flow?sectionCode=WP
        [HttpGet("weekly-flow")]
        public IActionResult GetWeeklyFlow([FromQuery] string sectionCode)
        {
            try
            {
                var branch = HttpContext.Session.GetString("BranchCode");
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);

                var resolvedCode = ResolveProcessingSection(master, branch, sectionCode);
                if (resolvedCode == null)
                    return StatusCode(500, new { message = "No active Processing section found for this branch." });

                var flow = master.Receiving.GetWeeklyReceivedFlow(resolvedCode);
                return Ok(flow);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // GET api/receiving/hourly-flow?sectionCode=WP
        [HttpGet("hourly-flow")]
        public IActionResult GetHourlyFlow([FromQuery] string sectionCode)
        {
            try
            {
                var branch = HttpContext.Session.GetString("BranchCode");
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);

                var resolvedCode = ResolveProcessingSection(master, branch, sectionCode);
                if (resolvedCode == null)
                    return StatusCode(500, new { message = "No active Processing section found for this branch." });

                var flow = master.Receiving.GetHourlyReceivedFlow(resolvedCode);
                return Ok(flow);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // GET api/receiving/incoming-batches?sectionCode=WP
        [HttpGet("incoming-batches")]
        public IActionResult GetIncomingBatches([FromQuery] string sectionCode)
        {
            try
            {
                var branch = HttpContext.Session.GetString("BranchCode");
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);

                var resolvedCode = ResolveProcessingSection(master, branch, sectionCode);
                if (resolvedCode == null)
                    return StatusCode(500, new { message = "No active Processing section found for this branch." });

                var data = master.Receiving.GetIncomingBatches(resolvedCode);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // GET api/receiving/incoming-specimens?sectionCode=WP
        [HttpGet("incoming-specimens")]
        public IActionResult GetIncomingSpecimens([FromQuery] string sectionCode)
        {
            try
            {
                var branch = HttpContext.Session.GetString("BranchCode");
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);

                var resolvedCode = ResolveProcessingSection(master, branch, sectionCode);
                if (resolvedCode == null)
                    return StatusCode(500, new { message = "No active Processing section found for this branch." });

                var data = master.Receiving.GetIncomingSpecimens(resolvedCode);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // GET api/receiving/received-batches?sectionCode=WP&dateFrom=2025-01-01&dateTo=2025-01-31
        [HttpGet("received-batches")]
        public IActionResult GetReceivedBatches([FromQuery] string? sectionCode,
                                                [FromQuery] string dateFrom,
                                                [FromQuery] string dateTo)
        {
            try
            {
                var branch = HttpContext.Session.GetString("BranchCode");
                var isAdminString = HttpContext.Session.GetString("IsAdmin");
                bool isAdmin = bool.TryParse(isAdminString, out var result) && result;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });
                if (string.IsNullOrEmpty(sectionCode) && !isAdmin)
                    return BadRequest(new { message = "Section code is required." });

                var from = DateTime.TryParse(dateFrom, out var df) ? df : DateTime.Today.AddDays(-30);
                var to = DateTime.TryParse(dateTo, out var dt) ? dt : DateTime.Today;

                using var master = new MasterService(branch);
                return Ok(master.Receiving.GetReceivedBatches(sectionCode, from, to));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}