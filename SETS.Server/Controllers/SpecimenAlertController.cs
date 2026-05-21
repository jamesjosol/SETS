using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Model.Main;
using Model.SETSDB;
using Service;
using SETS.Server.Hubs;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SpecimenAlertController : ControllerBase
    {
        private readonly IHubContext<NotificationHub> _hub;

        private string? Branch => HttpContext.Session.GetString("BranchCode");
        private string? UserID => HttpContext.Session.GetString("UserID");

        public SpecimenAlertController(IHubContext<NotificationHub> hub)
        {
            _hub = hub;
        }

        // POST api/specimensalert/set
        [HttpPost("set")]
        public async Task<IActionResult> SetAlert([FromBody] SetSpecimenAlertRequest request)
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

                if (string.IsNullOrWhiteSpace(request.Alert))
                    return BadRequest(new { message = "Alert message is required." });

                if (string.IsNullOrEmpty(request.UserID))
                    return BadRequest(new { message = "User ID is required." });

                using var master = new MasterService(branch);
                master.Receiving.SetSpecimenAlert(request);

                // ── Notification → lab sections ───────────────────────────────────
                try
                {
                    // Get the routed section code for this specimen
                    var sectionCode = master.SpecimenSection
                        .GetBySpecimenNo(request.SpecimenNo)
                        .FirstOrDefault()?.SectionCode;


                    if (!string.IsNullOrEmpty(sectionCode))
                    {
                        var notif = master.Notification.Create(new CreateNotificationRequest
                        {
                            NotifType = NotifType.SpecimenAlert,
                            Title = "Specimen Alert",
                            Message = $"Specimen {request.SpecimenNo} has an alert from processing: {request.Alert}",
                            TargetSection = sectionCode,
                            ReferenceID = request.SpecimenNo
                        });

                        await _hub.Clients
                            .Group($"notif-{branch}-sec-{sectionCode}")
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
                }
                catch (Exception notifEx)
                {
                    Console.WriteLine($"[NOTIF] SpecimenAlert failed for {request.SpecimenNo}: {notifEx.Message}");
                }

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // POST api/specimensalert/clear
        [HttpPost("clear")]
        public IActionResult ClearAlert([FromBody] ClearSpecimenAlertRequest request)
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

                if (string.IsNullOrEmpty(request.UserID))
                    return BadRequest(new { message = "User ID is required." });

                using var master = new MasterService(branch);
                master.Receiving.ClearSpecimenAlert(request);

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}