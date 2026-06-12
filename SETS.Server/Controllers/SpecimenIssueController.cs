// SETS.Server/Controllers/SpecimenIssueController.cs
using Microsoft.AspNetCore.Mvc;
using Model.Main;
using Service;

namespace SETS.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SpecimenIssueController : ControllerBase
    {
        private string? Branch => HttpContext.Session.GetString("BranchCode");
        private string? CurrentUserID => HttpContext.Session.GetString("UserID");
        private bool IsTLOrAdmin =>
            HttpContext.Session.GetString("IsAdmin") == "True" ||
            HttpContext.Session.GetInt32("RoleID") == 2;

        private IActionResult SessionExpired()
            => Unauthorized(new { message = "Session expired." });

        private IActionResult RequireTL()
            => Unauthorized(new { message = "Team Lead or Administrator access required." });

        // ── GET api/specimenissue/incident-types ──────────────────────────────
        [HttpGet("incident-types")]
        public IActionResult GetIncidentTypes()
        {
            try
            {
                var branch = Branch;
                if (string.IsNullOrEmpty(branch)) return SessionExpired();

                using var master = new MasterService(branch);
                return Ok(master.SpecimenIssue.GetIncidentTypes());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ── POST api/specimenissue/incident-types ─────────────────────────────
        [HttpPost("incident-types")]
        public IActionResult CreateIncidentType([FromBody] CreateIncidentTypeRequest request)
        {
            try
            {
                var branch = Branch;
                if (string.IsNullOrEmpty(branch)) return SessionExpired();

                request.UserID = CurrentUserID;

                using var master = new MasterService(branch);
                master.SpecimenIssue.CreateIncidentType(request);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(400, new { message = ex.Message });
            }
        }

        // ── PATCH api/specimenissue/incident-types/{id}/toggle ────────────────
        [HttpPatch("incident-types/{id}/toggle")]
        public IActionResult ToggleIncidentType(int id)
        {
            try
            {
                if (!IsTLOrAdmin) return RequireTL();

                var branch = Branch;
                if (string.IsNullOrEmpty(branch)) return SessionExpired();

                var request = new ToggleActiveRequest { UserID = CurrentUserID };

                using var master = new MasterService(branch);
                master.SpecimenIssue.ToggleIncidentType(id, request);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(400, new { message = ex.Message });
            }
        }

        // ── GET api/specimenissue/incident-types/{id}/subcategories ───────────
        [HttpGet("incident-types/{id}/subcategories")]
        public IActionResult GetSubCategories(int id)
        {
            try
            {
                var branch = Branch;
                if (string.IsNullOrEmpty(branch)) return SessionExpired();

                using var master = new MasterService(branch);
                return Ok(master.SpecimenIssue.GetSubCategories(id));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ── POST api/specimenissue/subcategories ──────────────────────────────
        [HttpPost("subcategories")]
        public IActionResult CreateSubCategory([FromBody] CreateSubCategoryRequest request)
        {
            try
            {
                var branch = Branch;
                if (string.IsNullOrEmpty(branch)) return SessionExpired();

                request.UserID = CurrentUserID;

                using var master = new MasterService(branch);
                master.SpecimenIssue.CreateSubCategory(request);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(400, new { message = ex.Message });
            }
        }

        // ── PATCH api/specimenissue/subcategories/{id}/toggle ─────────────────
        [HttpPatch("subcategories/{id}/toggle")]
        public IActionResult ToggleSubCategory(int id)
        {
            try
            {
                if (!IsTLOrAdmin) return RequireTL();

                var branch = Branch;
                if (string.IsNullOrEmpty(branch)) return SessionExpired();

                var request = new ToggleActiveRequest { UserID = CurrentUserID };

                using var master = new MasterService(branch);
                master.SpecimenIssue.ToggleSubCategory(id, request);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(400, new { message = ex.Message });
            }
        }

        // ── POST api/specimenissue/tags ───────────────────────────────────────
        [HttpPost("tags")]
        public IActionResult AddTag([FromBody] AddTagRequest request)
        {
            try
            {
                var branch = Branch;
                if (string.IsNullOrEmpty(branch)) return SessionExpired();

                request.UserID = CurrentUserID;

                using var master = new MasterService(branch);
                master.SpecimenIssue.AddTag(request);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(400, new { message = ex.Message });
            }
        }

        // ── DELETE api/specimenissue/tags/{id} ────────────────────────────────
        [HttpDelete("tags/{id}")]
        public IActionResult DeleteTag(int id)
        {
            try
            {
                var branch = Branch;
                if (string.IsNullOrEmpty(branch)) return SessionExpired();

                using var master = new MasterService(branch);
                master.SpecimenIssue.DeleteTag(id);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(400, new { message = ex.Message });
            }
        }

        // ── GET api/specimenissue/tags/suggestions ────────────────────────────
        [HttpGet("tags/suggestions")]
        public IActionResult GetTagSuggestions()
        {
            try
            {
                var branch = Branch;
                if (string.IsNullOrEmpty(branch)) return SessionExpired();

                using var master = new MasterService(branch);
                return Ok(master.SpecimenIssue.GetAllTagNames());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ── GET api/specimenissue/subcategories/{id}/entries ──────────────────
        [HttpGet("subcategories/{id}/entries")]
        public IActionResult GetLabEntries(int id)
        {
            try
            {
                var branch = Branch;
                if (string.IsNullOrEmpty(branch)) return SessionExpired();

                using var master = new MasterService(branch);
                return Ok(master.SpecimenIssue.GetLabEntries(id));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ── POST api/specimenissue/entries ────────────────────────────────────
        [HttpPost("entries")]
        public async Task<IActionResult> AddLabEntry([FromBody] AddLabEntryRequest request)
        {
            try
            {
                var branch = Branch;
                if (string.IsNullOrEmpty(branch)) return SessionExpired();

                if (string.IsNullOrEmpty(request.SpecimenNo))
                    return BadRequest(new { message = "Specimen number is required." });

                request.UserID = CurrentUserID;

                using var master = new MasterService(branch);
                await master.SpecimenIssue.AddLabEntry(request);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(400, new { message = ex.Message });
            }
        }

        // DELETE api/specimenissue/entries/{id}
        [HttpDelete("entries/{id}")]
        public IActionResult DeleteLabEntry(int id)
        {
            try
            {
                if (!IsTLOrAdmin) return RequireTL();

                var branch = Branch;
                if (string.IsNullOrEmpty(branch)) return SessionExpired();

                using var master = new MasterService(branch);
                master.SpecimenIssue.DeleteLabEntry(id);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(400, new { message = ex.Message });
            }
        }

        // PATCH api/specimenissue/entries/{id}/remark
        [HttpPatch("entries/{id}/remark")]
        public IActionResult UpdateLabEntryRemark(int id, [FromBody] UpdateLabEntryRemarkRequest request)
        {
            try
            {
                var branch = Branch;
                if (string.IsNullOrEmpty(branch)) return SessionExpired();

                request.UserID = CurrentUserID;

                using var master = new MasterService(branch);
                master.SpecimenIssue.UpdateLabEntryRemark(id, request);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(400, new { message = ex.Message });
            }
        }

        // ── GET api/specimenissue/incident-types/{id}/comments ────────────────
        [HttpGet("incident-types/{id}/comments")]
        public IActionResult GetComments(int id)
        {
            try
            {
                var branch = Branch;
                if (string.IsNullOrEmpty(branch)) return SessionExpired();

                using var master = new MasterService(branch);
                return Ok(master.SpecimenIssue.GetComments(id));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ── POST api/specimenissue/comments ───────────────────────────────────
        [HttpPost("comments")]
        public IActionResult AddComment([FromBody] AddCommentRequest request)
        {
            try
            {
                var branch = Branch;
                if (string.IsNullOrEmpty(branch)) return SessionExpired();

                if (string.IsNullOrEmpty(request.CommentText))
                    return BadRequest(new { message = "Comment text is required." });

                request.UserID = CurrentUserID;

                using var master = new MasterService(branch);
                master.SpecimenIssue.AddComment(request);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(400, new { message = ex.Message });
            }
        }

        // ── PATCH api/specimenissue/comments/{id} ─────────────────────────────
        [HttpPatch("comments/{id}")]
        public IActionResult EditComment(int id, [FromBody] EditCommentRequest request)
        {
            try
            {
                var branch = Branch;
                if (string.IsNullOrEmpty(branch)) return SessionExpired();

                if (string.IsNullOrEmpty(request.CommentText))
                    return BadRequest(new { message = "Comment text is required." });

                request.UserID = CurrentUserID;

                using var master = new MasterService(branch);
                master.SpecimenIssue.EditComment(id, request);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(400, new { message = ex.Message });
            }
        }

        // ── GET api/specimenissue/incident-types/{id}/export ─────────────────
        [HttpGet("incident-types/{id}/export")]
        public IActionResult ExportIncidentType(int id)
        {
            try
            {
                var branch = Branch;
                if (string.IsNullOrEmpty(branch)) return SessionExpired();

                using var master = new MasterService(branch);
                var bytes = master.SpecimenIssue.ExportIncidentTypeExcel(id);

                var fileName = $"IssuesLog_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                return File(bytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}