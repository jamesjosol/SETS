using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Model.Main;
using Service;
using SETS.Server.Hubs;

namespace SETS.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContingencyController : ControllerBase
    {
        private readonly IHubContext<ContingencyHub> _hub;

        private string? Branch => HttpContext.Session.GetString("BranchCode");
        private string? UserID => HttpContext.Session.GetString("UserID");
        private string? SectionCode => HttpContext.Session.GetString("SectionCode");
        private bool IsAdmin => HttpContext.Session.GetString("IsAdmin") == "True";
        private bool IsContingency => HttpContext.Session.GetString("IsContingency") == "True";

        public ContingencyController(IHubContext<ContingencyHub> hub)
        {
            _hub = hub;
        }

        private bool HasAccess()
        {
            // Normal authenticated users OR contingency-mode users
            return !string.IsNullOrEmpty(UserID);
        }

        // ── GET api/contingency/config ─────────────────────────────────────
        [HttpGet("config")]
        public IActionResult GetConfig()
        {
            try
            {
                if (!IsAdmin) return Unauthorized(new { message = "Admin access required." });
                var branch = Branch;
                if (string.IsNullOrEmpty(branch)) return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);
                return Ok(master.Contingency.GetConfig());
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // ── PUT api/contingency/config ─────────────────────────────────────
        [HttpPut("config")]
        public IActionResult UpsertConfig([FromBody] UpsertContingencyConfigRequest request)
        {
            try
            {
                if (!IsAdmin) return Unauthorized(new { message = "Admin access required." });
                var branch = Branch;
                var userID = UserID;
                if (string.IsNullOrEmpty(branch) || string.IsNullOrEmpty(userID))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);
                master.Contingency.UpsertConfig(request, userID);
                return Ok(new { message = "Contingency settings saved." });
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // ── GET api/contingency/sample-types ──────────────────────────────
        [HttpGet("sample-types")]
        public IActionResult GetSampleTypes()
        {
            try
            {
                var branch = Branch;
                if (string.IsNullOrEmpty(branch)) return Unauthorized(new { message = "Session expired." });
                using var master = new MasterService(branch);
                return Ok(master.Contingency.GetSampleTypes());
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // ── POST api/contingency/endorse ───────────────────────────────────
        [HttpPost("endorse")]
        public async Task<IActionResult> Endorse([FromBody] ContingencyEndorseRequest request)
        {
            try
            {
                var branch = Branch;
                var userID = UserID;
                var sectionCode = SectionCode;
                if (string.IsNullOrEmpty(branch) || string.IsNullOrEmpty(userID))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);
                var batchNo = master.Contingency.Endorse(request, userID, branch, sectionCode);

                // Notify receivers on destination branch via SignalR
                await _hub.Clients.Group($"branch-{request.EndorsedTo}")
                    .SendAsync("NewContingencyBatch", new { batchNo, endorsingBranch = branch, endorsedBy = userID });

                return Ok(new { batchNo, message = "Contingency batch endorsed successfully." });
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // GET api/contingency/endorsed-batches
        [HttpGet("endorsed-batches")]
        public IActionResult GetEndorsedBatches()
        {
            try
            {
                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);
                var batches = master.Contingency.GetBatchesByEndorsingBranch(branch);
                return Ok(batches);
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // ── GET api/contingency/export/{batchNo} ───────────────────────────
        [HttpGet("export/{batchNo}")]
        public IActionResult ExportExcel(string batchNo)
        {
            try
            {
                var branch = Branch;
                if (string.IsNullOrEmpty(branch)) return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);
                var bytes = master.Contingency.ExportBatchExcel(batchNo);
                var fileName = $"{batchNo}.xlsx";
                return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // ── GET api/contingency/batches ────────────────────────────────────
        [HttpGet("batches")]
        public IActionResult GetBatches()
        {
            try
            {
                var branch = Branch;
                if (string.IsNullOrEmpty(branch)) return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);
                var batches = master.Contingency.GetBatchesByBranch(branch);
                return Ok(batches);
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // ── GET api/contingency/batches/{id} ──────────────────────────────
        [HttpGet("batches/{id:int}")]
        public IActionResult GetBatchDetail(int id)
        {
            try
            {
                var branch = Branch;
                if (string.IsNullOrEmpty(branch)) return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);
                var detail = master.Contingency.GetBatchDetail(id);
                return Ok(detail);
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // ── POST api/contingency/scan ──────────────────────────────────────
        [HttpPost("scan")]
        public async Task<IActionResult> Scan([FromBody] ContingencyScanRequest request)
        {
            try
            {
                var branch = Branch;
                var userID = UserID;
                if (string.IsNullOrEmpty(branch) || string.IsNullOrEmpty(userID))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);
                var result = master.Contingency.ScanSpecimen(request, userID);

                // Notify all receivers on same branch of progress update
                if (result.Found && !result.AlreadyReceived)
                {
                    await _hub.Clients.Group($"branch-{branch}")
                        .SendAsync("ContingencySpecimenReceived", new
                        {
                            batchId = request.BatchId,
                            specimenNo = request.SpecimenNo,
                            batchCompleted = result.BatchCompleted
                        });
                }

                return Ok(result);
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // ── POST api/contingency/import ────────────────────────────────────────
        [HttpPost("import")]
        [Consumes("multipart/form-data")]
        public IActionResult ImportExcel(IFormFile file)
        {
            try
            {
                var branch = Branch;
                var userID = UserID;

                if (string.IsNullOrEmpty(branch) || string.IsNullOrEmpty(userID))
                    return Unauthorized(new { message = "Session expired." });

                if (file == null || file.Length == 0)
                    return BadRequest(new { message = "No file provided." });

                using var ms = new MemoryStream();
                file.CopyTo(ms);

                var bytes = ms.ToArray();

                using var master = new MasterService(branch);

                var result = master.Contingency.ImportFromExcel(bytes, branch, userID);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}