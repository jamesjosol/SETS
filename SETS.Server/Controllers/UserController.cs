using Microsoft.AspNetCore.Mvc;
using Model.Main;
using Service;

namespace SETS.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        [HttpPut("theme")]
        public IActionResult UpdateTheme([FromBody] UpdateThemeRequest request)
        {
            try
            {
                var userID = HttpContext.Session.GetString("UserID");
                var branch = HttpContext.Session.GetString("BranchCode");

                if (string.IsNullOrEmpty(userID))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);
                var user = master.User.GetUser(userID, branch);

                if (user == null)
                    return NotFound(new { message = "User not found." });

                user.Theme = request.Theme;
                master.User.Update(user);

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
