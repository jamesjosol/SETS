using Microsoft.AspNetCore.Mvc;
using Model.Main;
using Service;

namespace SETS.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RunnerController : ControllerBase
    {
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
        public IActionResult SaveAssignments([FromBody] SaveAssignmentsRequest request)
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
                master.Runner.SaveAssignments(request);

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

        // GET api/runner/saved?sectionCode=HEMA
        [HttpGet("saved")]
        public IActionResult GetSavedSpecimens([FromQuery] string sectionCode)
        {
            try
            {
                var branch = HttpContext.Session.GetString("BranchCode");
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                if (string.IsNullOrEmpty(sectionCode))
                    return BadRequest(new { message = "Section code is required." });

                using var master = new MasterService(branch);
                return Ok(master.Runner.GetSavedTests(sectionCode));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // GET api/runner/tests/{headerId}
        [HttpGet("tests/{headerId}")]
        public IActionResult GetTestsByHeader(int headerId)
        {
            try
            {
                var branch = HttpContext.Session.GetString("BranchCode");
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);
                return Ok(master.Runner.GetTestsByHeader(headerId));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}