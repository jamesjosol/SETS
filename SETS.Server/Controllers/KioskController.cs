using Microsoft.AspNetCore.Mvc;
using Service;

namespace SETS.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class KioskController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public KioskController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Resolves the branch from appsettings.json Default:Branch
        // No session required — this is a public kiosk endpoint
        private string LocalBranch
            => _configuration["Default:Branch"]
               ?? throw new Exception("Default branch not configured in appsettings.json.");

        // ── GET api/kiosk/summary ─────────────────────────────────────────────
        // KPI counts for the kiosk header row
        [HttpGet("summary")]
        public IActionResult GetSummary()
        {
            try
            {
                var branch = LocalBranch;
                using var master = new MasterService(branch);

                // Resolve the processing section for this branch
                var procSection = master.Section
                    .GetByBranch(branch)
                    .FirstOrDefault(s => s.Category == "2" && s.Active);

                if (procSection == null)
                    return StatusCode(500, new { message = "No active Processing section found for this branch." });

                var summary = master.Receiving.GetReceiverDashboardSummary(procSection.Code, DateTime.Today);

                return Ok(new
                {
                    branchCode = branch,
                    branchName = master.Branch.GetAll().FirstOrDefault(b => b.Code == branch)?.Name ?? branch,
                    totalEndorsed = summary.TotalEndorsed,
                    pending = summary.Pending,
                    partial = summary.OutsideTAT,   // OutsideTAT is reused as Partial count here
                    completed = summary.Received
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ── GET api/kiosk/monitoring ──────────────────────────────────────────
        // Full monitoring grid data — all endorser sections + their today-batches
        [HttpGet("monitoring")]
        public IActionResult GetMonitoring()
        {
            try
            {
                var branch = LocalBranch;
                using var master = new MasterService(branch);

                // Resolve the processing section for this branch
                var procSection = master.Section
                    .GetByBranch(branch)
                    .FirstOrDefault(s => s.Category == "2" && s.Active);

                if (procSection == null)
                    return StatusCode(500, new { message = "No active Processing section found for this branch." });

                var data = master.Receiving.GetMonitoringDashboard(procSection.Code, DateTime.Today);
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
    }
}