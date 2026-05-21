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
    public class FlagController : ControllerBase
    {
        private readonly IHubContext<NotificationHub> _hub;

        private string? Branch => HttpContext.Session.GetString("BranchCode");
        private string? UserID => HttpContext.Session.GetString("UserID");

        public FlagController(IHubContext<NotificationHub> hub)
        {
            _hub = hub;
        }

        // PATCH api/flag/specimen
        [HttpPatch("specimen")]
        public async Task<IActionResult> FlagSpecimen([FromBody] FlagSpecimenRequest request)
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

                if (string.IsNullOrWhiteSpace(request.FlagReason))
                    return BadRequest(new { message = "Flag reason is required." });

                if (string.IsNullOrEmpty(request.UserID))
                    return BadRequest(new { message = "User ID is required." });

                using var master = new MasterService(branch);
                var result = master.Flag.FlagSpecimen(request);

                // ── Audit ─────────────────────────────────────────────────────────
                try
                {
                    using var auditMaster = new MasterService(branch);
                    auditMaster.Audit.Log(Audit_Log.SpecimenFlagged(
                        request.SpecimenNo,
                        request.BatchNo,
                        result.PatientName,
                        result.PID,
                        result.LocationName,
                        request.FlagReason,
                        request.UserID));
                }
                catch (Exception auditEx)
                {
                    Console.WriteLine($"[AUDIT] SpecimenFlagged failed for {request.SpecimenNo}: {auditEx.Message}");
                }

                // ── Notification → receiver category ─────────────────────────────
                try
                {
                    using var notifMaster = new MasterService(branch);
                    var notif = notifMaster.Notification.Create(new CreateNotificationRequest
                    {
                        NotifType = NotifType.SpecimenFlagged,
                        Title = "Action Required Specimen",
                        Message = $"Specimen {request.SpecimenNo} in batch {request.BatchNo} has been flagged. Reason: {request.FlagReason}",
                        TargetCategory = "2",
                        ReferenceID = request.BatchNo
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
                catch (Exception notifEx)
                {
                    Console.WriteLine($"[NOTIF] SpecimenFlagged failed for {request.SpecimenNo}: {notifEx.Message}");
                }

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // PATCH api/flag/specimen/unflag
        [HttpPatch("specimen/unflag")]
        public IActionResult UnflagSpecimen([FromBody] UnflagSpecimenRequest request)
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
                var result = master.Flag.UnflagSpecimen(request);

                // ── Audit ─────────────────────────────────────────────────────────
                try
                {
                    using var auditMaster = new MasterService(branch);
                    auditMaster.Audit.Log(Audit_Log.SpecimenUnflagged(
                        request.SpecimenNo,
                        request.BatchNo,
                        result.PatientName,
                        result.PID,
                        result.LocationName,
                        request.UserID));
                }
                catch (Exception auditEx)
                {
                    Console.WriteLine($"[AUDIT] SpecimenUnflagged failed for {request.SpecimenNo}: {auditEx.Message}");
                }

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}