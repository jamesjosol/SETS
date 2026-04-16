using Microsoft.AspNetCore.Mvc;
using Model.Main;
using Service;

namespace SETS.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;

        public AuthController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            try
            {
                using var master = new MasterService(request.Branch);

                // Step 1: Verify credentials in HCLAB (Oracle)
                bool isAuthenticated = master.User.Login(
                    request.UserID,
                    request.Password,
                    request.Branch
                );

                if (!isAuthenticated)
                    return Unauthorized(new LoginResponse
                    {
                        Success = false,
                        Message = "Invalid username or password."
                    });

                // Step 2: Get user data from SETSDB (MSSQL)
                var user = master.User.GetUser(request.UserID, request.Branch);

                if (user == null)
                    return Unauthorized(new LoginResponse
                    {
                        Success = false,
                        Message = "User is not registered."
                    });

                // Step 3: Check if user has access to the selected section
                var userSection = master.UserSection.GetByUserAndSection(
                    request.UserID,
                    request.SectionCode
                );

                if (userSection == null)
                    return Unauthorized(new LoginResponse
                    {
                        Success = false,
                        Message = $"You do not have access to the {request.SectionCode} section."
                    });

                HttpContext.Session.SetString("UserID", user.UserID);
                HttpContext.Session.SetString("UserName", user.UserName);
                HttpContext.Session.SetString("BranchCode", request.Branch);
                HttpContext.Session.SetString("SectionCode", request.SectionCode);
                HttpContext.Session.SetInt32("RoleID", userSection.RoleID);
                HttpContext.Session.SetString("IsAdmin", user.IsAdmin.ToString());

                Console.WriteLine("logged in: " + HttpContext.Session.GetString("UserID"));

                var section = master.Section.GetByCode(request.SectionCode);

                return Ok(new LoginResponse
                {
                    Success = true,
                    Message = "Login successful.",
                    Data = new UserSessionData
                    {
                        UserID = user.UserID,
                        UserName = user.UserName,
                        IsAdmin = user.IsAdmin,
                        BranchCode = request.Branch,
                        SectionCode = request.SectionCode,
                        SectionName = section?.Name ?? request.SectionCode,
                        RoleID = userSection.RoleID,
                        Theme = user.Theme,
                        SectionCategory = section?.Category ?? "1"
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new LoginResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpGet("pc-info")]
        public IActionResult GetPCInfo()
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

                var _branch = _config["Default:Branch"];

                // handle IPv6 loopback
                if (ipAddress == "::1") ipAddress = "127.0.0.1";

                using var master = new MasterService(_branch);

                var pc = master.PC.GetByIP(ipAddress);

                if (pc == null)
                    return Ok(new
                    {
                        isRegistered = false,
                        ipAddress,
                        sections = new List<object>()
                    });

                var pcSections = master.PC.GetSectionsByIP(ipAddress);

                // get section names
                var sections = pcSections.Select(ps =>
                {
                    var section = master.Section.GetByCode(ps.SectionCode);
                    return new
                    {
                        code = ps.SectionCode,
                        name = section?.Name ?? ps.SectionCode,
                        branchCode = section?.BranchCode ?? _branch
                    };
                }).ToList();

                return Ok(new
                {
                    isRegistered = true,
                    ipAddress,
                    sections
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            try
            {
                Console.WriteLine("logged out: " + HttpContext.Session.GetString("UserID"));
                HttpContext.Session.Clear();
                return Ok(new { success = true, message = "Logged out successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}