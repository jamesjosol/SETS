using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Model.Main;
using Model.SETSDB;
using Service;
using System.Threading.Tasks;

namespace SETS.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private string? Branch => HttpContext.Session.GetString("BranchCode");
        private string? CurrentUserID => HttpContext.Session.GetString("UserID");
        private bool IsAdmin => HttpContext.Session.GetString("IsAdmin") == "True";

        private IActionResult RequireAdmin()
            => Unauthorized(new { message = "Administrator access required." });

        // ── PUT api/user/theme ────────────────────────────────────────────────
        [HttpPut("theme")]
        public IActionResult UpdateTheme([FromBody] UpdateThemeRequest request)
        {
            try
            {
                var userID = CurrentUserID;
                var branch = Branch;
                if (string.IsNullOrEmpty(userID))
                    return Unauthorized(new { message = "Session expired." });
                using var master = new MasterService(branch);
                var user = master.User.GetUser(userID, branch);
                if (user == null)
                    return NotFound(new { message = "User not found." });
                user.Theme = request.Theme;
                user.AccentColor = request.AccentColor;
                master.User.Update(user);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ── GET api/user ──────────────────────────────────────────────────────
        // Returns all users with their section assignments.
        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                if (!IsAdmin) return RequireAdmin();

                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);

                var users = master.User.GetAll();
                var allSections = master.Section.GetAll();

                var result = users.Select(u =>
                {
                    var userSections = master.UserSection.GetByUserID(u.UserID);
                    var sections = userSections.Select(us =>
                    {
                        var section = allSections.FirstOrDefault(s => s.Code == us.SectionCode);
                        return new
                        {
                            id = us.Id,
                            sectionCode = us.SectionCode,
                            sectionName = section?.Name ?? us.SectionCode,
                            roleID = us.RoleID,
                            active = us.Active
                        };
                    }).ToList();

                    return new
                    {
                        userID = u.UserID,
                        userName = u.UserName,
                        isAdmin = u.IsAdmin,
                        active = u.Active,
                        theme = u.Theme,
                        created = u.Created,
                        sections
                    };
                })
                .OrderBy(u => u.userID)
                .ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ── GET HCLAB api/hclab-user ──────────────────────────────────────────────────────
        // Returns all users from hclab
        [HttpGet("hclab-user")]
        public async Task<IActionResult> GetHclabUsers(string param)
        {
            try
            {
                if (!IsAdmin) return RequireAdmin();

                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                if (string.IsNullOrWhiteSpace(param))
                    return BadRequest(new { message = "User name is required." });

                using var master = new MasterService(branch);

                var users = await master.User.GetHCLABUsers(param.ToUpper());


                var result = users.Select(u =>
                {
                    return new
                    {
                        userID = u.UserID,
                        userName = u.UserName,
                    };
                })
                .OrderBy(u => u.userID)
                .ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ── POST api/user ─────────────────────────────────────────────────────
        // Register a new user with at least one section.
        [HttpPost]
        public IActionResult Add([FromBody] AddUserRequest request)
        {
            try
            {
                if (!IsAdmin) return RequireAdmin();

                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                if (string.IsNullOrWhiteSpace(request.UserID))
                    return BadRequest(new { message = "User ID is required." });

                if (string.IsNullOrWhiteSpace(request.UserName))
                    return BadRequest(new { message = "User name is required." });

                if (request.Sections == null || !request.Sections.Any())
                    return BadRequest(new { message = "At least one section must be assigned." });

                var createdBy = CurrentUserID ?? "SYSTEM";

                using var master = new MasterService(branch);

                // Check duplicate
                var existing = master.User.GetUser(request.UserID.Trim().ToUpper(), branch);
                if (existing != null)
                    return BadRequest(new { message = $"User ID '{request.UserID}' is already registered." });

                var user = new User_Master
                {
                    UserID = request.UserID.Trim().ToUpper(),
                    UserName = request.UserName.Trim(),
                    IsAdmin = request.IsAdmin,
                    Active = true,
                    Theme = 0,
                    Created = DateTime.Now,
                    CreatedBy = createdBy
                };

                master.User.Add(user);

                // Add section assignments
                foreach (var sec in request.Sections)
                {
                    master.UserSection.Add(new User_Section
                    {
                        UserID = user.UserID,
                        SectionCode = sec.SectionCode,
                        RoleID = sec.RoleID,
                        Active = true,
                        Created = DateTime.Now,
                        CreatedBy = createdBy
                    });
                }

                return Ok(new { success = true, message = "User registered successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ── PUT api/user/{userID} ─────────────────────────────────────────────
        // Update user info (name, isAdmin, active).
        [HttpPut("{userID}")]
        public IActionResult Update(string userID, [FromBody] UpdateUserRequest request)
        {
            try
            {
                if (!IsAdmin) return RequireAdmin();

                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);

                var user = master.User.GetUser(userID, branch);
                if (user == null)
                    return NotFound(new { message = "User not found." });

                user.UserName = request.UserName?.Trim() ?? user.UserName;
                user.IsAdmin = request.IsAdmin;
                user.Active = request.Active;
                user.UpdatedBy = CurrentUserID ?? "SYSTEM";
                user.Updated = DateTime.Now;

                master.User.Update(user);

                return Ok(new { success = true, message = "User updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ── PATCH api/user/{userID}/toggle ────────────────────────────────────
        // Toggle active/inactive status.
        [HttpPatch("{userID}/toggle")]
        public IActionResult Toggle(string userID)
        {
            try
            {
                if (!IsAdmin) return RequireAdmin();

                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                // Prevent self-deactivation
                if (userID.Equals(CurrentUserID, StringComparison.OrdinalIgnoreCase))
                    return BadRequest(new { message = "You cannot deactivate your own account." });

                using var master = new MasterService(branch);

                var user = master.User.GetUser(userID, branch);
                if (user == null)
                    return NotFound(new { message = "User not found." });

                user.Active = !user.Active;
                user.UpdatedBy = CurrentUserID ?? "SYSTEM";
                user.Updated = DateTime.Now;

                master.User.Update(user);

                return Ok(new { success = true, active = user.Active });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ── PUT api/user/{userID}/sections ────────────────────────────────────
        // Replace the full set of section assignments for a user.
        [HttpPut("{userID}/sections")]
        public IActionResult UpdateSections(string userID, [FromBody] UpdateUserSectionsRequest request)
        {
            try
            {
                if (!IsAdmin) return RequireAdmin();

                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                if (request.Sections == null || !request.Sections.Any())
                    return BadRequest(new { message = "At least one section must be assigned." });

                var updatedBy = CurrentUserID ?? "SYSTEM";

                using var master = new MasterService(branch);

                var user = master.User.GetUser(userID, branch);
                if (user == null)
                    return NotFound(new { message = "User not found." });

                // Deactivate existing section assignments (all statuses)
                var existing = master.UserSection.GetAllByUserID(userID);
                foreach (var us in existing)
                {
                    us.Active = false;
                    us.UpdatedBy = updatedBy;
                    us.Updated = DateTime.Now;
                    master.UserSection.Update(us);
                }

                // Add new assignments (or reactivate if same section exists)
                foreach (var sec in request.Sections)
                {
                    // Check if this user-section combo already existed (even if inactive)
                    var match = existing.FirstOrDefault(e => e.SectionCode == sec.SectionCode);
                    if (match != null)
                    {
                        match.Active = true;
                        match.RoleID = sec.RoleID;
                        match.UpdatedBy = updatedBy;
                        match.Updated = DateTime.Now;
                        master.UserSection.Update(match);
                    }
                    else
                    {
                        master.UserSection.Add(new User_Section
                        {
                            UserID = userID,
                            SectionCode = sec.SectionCode,
                            RoleID = sec.RoleID,
                            Active = true,
                            Created = DateTime.Now,
                            CreatedBy = updatedBy
                        });
                    }
                }

                return Ok(new { success = true, message = "Section assignments updated." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
