using Microsoft.AspNetCore.Mvc;
using Model.Main;
using Service;

namespace SETS.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestGroupOverrideController : ControllerBase
    {
        private string? Branch => HttpContext.Session.GetString("BranchCode");
        private string? UserID => HttpContext.Session.GetString("UserID");

        // GET api/testgroupoverride
        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);
                var result = master.TestGroupOverride.GetAll();
                return Ok(result);
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // POST api/testgroupoverride
        [HttpPost]
        public IActionResult Add([FromBody] AddTestGroupOverrideRequest request)
        {
            try
            {
                var branch = Branch;
                var userID = UserID;
                if (string.IsNullOrEmpty(branch) || string.IsNullOrEmpty(userID))
                    return Unauthorized(new { message = "Session expired." });

                request.UserID = userID;
                using var master = new MasterService(branch);
                master.TestGroupOverride.Add(request);
                return Ok(new { success = true });
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // PUT api/testgroupoverride/{id}
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] UpdateTestGroupOverrideRequest request)
        {
            try
            {
                var branch = Branch;
                var userID = UserID;
                if (string.IsNullOrEmpty(branch) || string.IsNullOrEmpty(userID))
                    return Unauthorized(new { message = "Session expired." });

                request.UserID = userID;
                using var master = new MasterService(branch);
                master.TestGroupOverride.Update(id, request);
                return Ok(new { success = true });
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // PATCH api/testgroupoverride/{id}/toggle
        [HttpPatch("{id}/toggle")]
        public IActionResult Toggle(int id)
        {
            try
            {
                var branch = Branch;
                var userID = UserID;
                if (string.IsNullOrEmpty(branch) || string.IsNullOrEmpty(userID))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);
                master.TestGroupOverride.Toggle(id, userID);
                return Ok(new { success = true });
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // DELETE api/testgroupoverride/{id}
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);
                master.TestGroupOverride.Delete(id);
                return Ok(new { success = true });
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }
    }
}