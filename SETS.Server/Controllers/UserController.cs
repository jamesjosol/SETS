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
        // ── Session helpers ───────────────────────────────────────────────────

        private string? Branch => HttpContext.Session.GetString("BranchCode");
        private string? CurrentUserID => HttpContext.Session.GetString("UserID");
        private bool IsAdmin => HttpContext.Session.GetString("IsAdmin") == "True";
        private int RoleID => HttpContext.Session.GetInt32("RoleID") ?? 0;
        private string? SectionCode => HttpContext.Session.GetString("SectionCode");

        private bool IsTL => RoleID == 2;

        private IActionResult RequireAdmin()
            => Unauthorized(new { message = "Administrator access required." });

        private IActionResult RequireTL()
            => Unauthorized(new { message = "Team Lead access required." });

        // ══════════════════════════════════════════════════════════════════════
        // SHARED / PERSONAL ENDPOINTS
        // ══════════════════════════════════════════════════════════════════════

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
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // ── GET api/user/hclab-user ───────────────────────────────────────────
        // Accessible to Admin AND TL (both need it to search users)
        [HttpGet("hclab-user")]
        public async Task<IActionResult> GetHclabUsers(string param)
        {
            try
            {
                if (!IsAdmin && !IsTL) return RequireAdmin();

                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                if (string.IsNullOrWhiteSpace(param))
                    return BadRequest(new { message = "User name is required." });

                using var master = new MasterService(branch);

                var users = await master.User.GetHCLABUsers(param.ToUpper());

                var result = users.Select(u => new
                {
                    userID = u.UserID,
                    userName = u.UserName,
                })
                .OrderBy(u => u.userID)
                .ToList();

                return Ok(result);
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // ══════════════════════════════════════════════════════════════════════
        // ADMIN ENDPOINTS (your original — unchanged)
        // ══════════════════════════════════════════════════════════════════════

        // ── GET api/user ──────────────────────────────────────────────────────
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
                        profilePicture = u.ProfilePicture,
                        sections
                    };
                })
                .OrderBy(u => u.userID)
                .ToList();

                return Ok(result);
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // ── POST api/user ─────────────────────────────────────────────────────
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
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // ── PUT api/user/{userID} ─────────────────────────────────────────────
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
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // ── PATCH api/user/{userID}/toggle ────────────────────────────────────
        [HttpPatch("{userID}/toggle")]
        public IActionResult Toggle(string userID)
        {
            try
            {
                if (!IsAdmin) return RequireAdmin();

                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

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
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // ── PUT api/user/{userID}/sections ────────────────────────────────────
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

                var existing = master.UserSection.GetAllByUserID(userID);
                foreach (var us in existing)
                {
                    us.Active = false;
                    us.UpdatedBy = updatedBy;
                    us.Updated = DateTime.Now;
                    master.UserSection.Update(us);
                }

                foreach (var sec in request.Sections)
                {
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
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // ── DELETE api/user/{userID} ──────────────────────────────────────────
        // Soft deletes a user (sets Active = false). Preserves audit trail history.
        // Add this method after the existing Toggle endpoint in UserController.cs
        [HttpDelete("{userID}")]
        public IActionResult Delete(string userID)
        {
            try
            {
                if (!IsAdmin) return RequireAdmin();

                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                // Prevent self-deletion
                if (userID.Equals(CurrentUserID, StringComparison.OrdinalIgnoreCase))
                    return BadRequest(new { message = "You cannot delete your own account." });

                using var master = new MasterService(branch);

                var user = master.User.GetUser(userID, branch);
                if (user == null)
                    return NotFound(new { message = "User not found." });

                user.Active = false;
                user.UpdatedBy = CurrentUserID ?? "SYSTEM";
                user.Updated = DateTime.Now;

                master.User.Update(user);

                return Ok(new { success = true, message = "User deactivated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // TL ENDPOINTS (scoped to session SectionCode)
        // ══════════════════════════════════════════════════════════════════════

        // ── GET api/user/tl ───────────────────────────────────────────────────
        [HttpGet("tl")]
        public IActionResult TLGetAll()
        {
            try
            {
                if (!IsTL && !IsAdmin) return RequireTL();

                var branch = Branch;
                var sectionCode = SectionCode;

                if (string.IsNullOrEmpty(branch) || string.IsNullOrEmpty(sectionCode))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);

                var allUsers = master.User.GetAll();
                var allSections = master.Section.GetAll();

                var result = allUsers
                    .Select(u =>
                    {
                        var userSections = master.UserSection.GetByUserID(u.UserID);
                        return new { u, userSections };
                    })
                    .Where(x => x.userSections.Any(us => us.SectionCode == sectionCode))
                    .Select(x =>
                    {
                        var sections = x.userSections.Select(us =>
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
                            userID = x.u.UserID,
                            userName = x.u.UserName,
                            isAdmin = x.u.IsAdmin,
                            active = x.u.Active,
                            theme = x.u.Theme,
                            created = x.u.Created,
                            sections
                        };
                    })
                    .OrderBy(u => u.userID)
                    .ToList();

                return Ok(result);
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // ── POST api/user/tl ──────────────────────────────────────────────────
        // Add a user and assign them to the TL's section.
        [HttpPost("tl")]
        public IActionResult TLAdd([FromBody] TLAddUserRequest request)
        {
            try
            {
                if (!IsTL && !IsAdmin) return RequireTL();

                var branch = Branch;
                var sectionCode = SectionCode;
                var createdBy = CurrentUserID ?? "SYSTEM";

                if (string.IsNullOrEmpty(branch) || string.IsNullOrEmpty(sectionCode))
                    return Unauthorized(new { message = "Session expired." });

                if (string.IsNullOrWhiteSpace(request.UserID))
                    return BadRequest(new { message = "User ID is required." });

                if (string.IsNullOrWhiteSpace(request.UserName))
                    return BadRequest(new { message = "User name is required." });

                using var master = new MasterService(branch);

                var existing = master.User.GetUser(request.UserID.Trim().ToUpper(), branch);

                if (existing != null)
                {
                    // User already exists — grant section access if not already assigned
                    var existingSections = master.UserSection.GetAllByUserID(existing.UserID);
                    var match = existingSections.FirstOrDefault(us => us.SectionCode == sectionCode);

                    if (match != null && match.Active)
                        return BadRequest(new { message = $"User '{request.UserID}' already has access to this section." });

                    if (match != null)
                    {
                        match.Active = true;
                        match.RoleID = request.RoleID;
                        match.UpdatedBy = createdBy;
                        match.Updated = DateTime.Now;
                        master.UserSection.Update(match);
                    }
                    else
                    {
                        master.UserSection.Add(new User_Section
                        {
                            UserID = existing.UserID,
                            SectionCode = sectionCode,
                            RoleID = request.RoleID,
                            Active = true,
                            Created = DateTime.Now,
                            CreatedBy = createdBy
                        });
                    }

                    return Ok(new { success = true, message = "User access granted to this section." });
                }

                // New user — TL cannot create admins
                var user = new User_Master
                {
                    UserID = request.UserID.Trim().ToUpper(),
                    UserName = request.UserName.Trim(),
                    IsAdmin = false,
                    Active = true,
                    Theme = 0,
                    Created = DateTime.Now,
                    CreatedBy = createdBy
                };

                master.User.Add(user);

                // Always use session SectionCode — never trust client
                master.UserSection.Add(new User_Section
                {
                    UserID = user.UserID,
                    SectionCode = sectionCode,
                    RoleID = request.RoleID,
                    Active = true,
                    Created = DateTime.Now,
                    CreatedBy = createdBy
                });

                return Ok(new { success = true, message = "User registered successfully." });
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // ── PUT api/user/tl/{userID}/role ─────────────────────────────────────
        // Update a user's role within the TL's section only.
        [HttpPut("tl/{userID}/role")]
        public IActionResult TLUpdateRole(string userID, [FromBody] TLUpdateRoleRequest request)
        {
            try
            {
                if (!IsTL && !IsAdmin) return RequireTL();

                var branch = Branch;
                var sectionCode = SectionCode;

                if (string.IsNullOrEmpty(branch) || string.IsNullOrEmpty(sectionCode))
                    return Unauthorized(new { message = "Session expired." });

                // TL cannot elevate to Admin (roleID 3+)
                if (request.RoleID > 2)
                    return BadRequest(new { message = "Invalid role." });

                using var master = new MasterService(branch);

                var assignment = master.UserSection.GetByUserAndSection(userID, sectionCode);
                if (assignment == null)
                    return NotFound(new { message = "User does not have access to this section." });

                assignment.RoleID = request.RoleID;
                assignment.UpdatedBy = CurrentUserID ?? "SYSTEM";
                assignment.Updated = DateTime.Now;

                master.UserSection.Update(assignment);
                return Ok(new { success = true, message = "Role updated successfully." });
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // ── PATCH api/user/tl/{userID}/toggle ────────────────────────────────
        [HttpPatch("tl/{userID}/toggle")]
        public IActionResult TLToggle(string userID)
        {
            try
            {
                if (!IsTL && !IsAdmin) return RequireTL();

                var branch = Branch;
                var sectionCode = SectionCode;

                if (string.IsNullOrEmpty(branch) || string.IsNullOrEmpty(sectionCode))
                    return Unauthorized(new { message = "Session expired." });

                if (userID.Equals(CurrentUserID, StringComparison.OrdinalIgnoreCase))
                    return BadRequest(new { message = "You cannot deactivate your own account." });

                using var master = new MasterService(branch);

                var userSections = master.UserSection.GetByUserID(userID);
                if (!userSections.Any(us => us.SectionCode == sectionCode))
                    return Unauthorized(new { message = "You do not have access to this user." });

                var user = master.User.GetUser(userID, branch);
                if (user == null)
                    return NotFound(new { message = "User not found." });

                user.Active = !user.Active;
                user.UpdatedBy = CurrentUserID ?? "SYSTEM";
                user.Updated = DateTime.Now;

                master.User.Update(user);
                return Ok(new { success = true, active = user.Active });
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // ── DELETE api/user/tl/{userID}/section ───────────────────────────────
        // Remove a user's access from TL's section (soft deactivate UserSection).
        [HttpDelete("tl/{userID}/section")]
        public IActionResult TLRemoveFromSection(string userID)
        {
            try
            {
                if (!IsTL && !IsAdmin) return RequireTL();

                var branch = Branch;
                var sectionCode = SectionCode;

                if (string.IsNullOrEmpty(branch) || string.IsNullOrEmpty(sectionCode))
                    return Unauthorized(new { message = "Session expired." });

                if (userID.Equals(CurrentUserID, StringComparison.OrdinalIgnoreCase))
                    return BadRequest(new { message = "You cannot remove your own section access." });

                using var master = new MasterService(branch);

                var assignment = master.UserSection.GetByUserAndSection(userID, sectionCode);
                if (assignment == null)
                    return NotFound(new { message = "User does not have access to this section." });

                assignment.Active = false;
                assignment.UpdatedBy = CurrentUserID ?? "SYSTEM";
                assignment.Updated = DateTime.Now;

                master.UserSection.Update(assignment);
                return Ok(new { success = true, message = "User removed from section." });
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // ── GET api/user/profile ──────────────────────────────────────────────────
        [HttpGet("profile")]
        public IActionResult GetProfile()
        {
            try
            {
                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                var userID = CurrentUserID;
                if (string.IsNullOrEmpty(userID))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);
                var result = master.User.GetProfile(userID);
                return Ok(result);
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // ── PUT api/user/profile/picture ─────────────────────────────────────────
        [HttpPut("profile/picture")]
        public IActionResult UpdateProfilePicture([FromBody] UpdateProfilePictureRequest request)
        {
            try
            {
                var branch = Branch;
                if (string.IsNullOrEmpty(branch))
                    return Unauthorized(new { message = "Session expired." });

                var userID = CurrentUserID;
                if (string.IsNullOrEmpty(userID))
                    return Unauthorized(new { message = "Session expired." });

                using var master = new MasterService(branch);
                master.User.UpdateProfilePicture(userID, request.ProfilePicture);
                return Ok(new { success = true });
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }
    }
}