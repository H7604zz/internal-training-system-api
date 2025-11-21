using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace InternalTrainingSystem.Core.Controllers
{
    [ApiController]
    [Route("api/classes/{classId:int}/assignments")]
    public class AssignmentController : ControllerBase
    {
        private readonly IAssignmentService _assignmentService;

        public AssignmentController(IAssignmentService assignmentService)
        {
            _assignmentService = assignmentService;
        }
        private string GetUserId()
            => User.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? throw new InvalidOperationException("UserId not found in claims.");

        /// <summary>
        /// Mentor tạo assignment cho CLASS offline
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Mentor")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<AssignmentDto>> CreateAssignment(int classId, [FromForm] CreateAssignmentForm form, CancellationToken ct)
        {
            if (classId != form.ClassId)
                return BadRequest("ClassId không khớp.");

            var mentorId = GetUserId();
            var result = await _assignmentService.CreateAssignmentAsync(form, mentorId, ct);

            return CreatedAtAction(nameof(GetAssignmentById),
                new { classId, assignmentId = result.AssignmentId },
                result);
        }

        /// <summary>
        /// Mentor cập nhật assignment
        /// </summary>
        [HttpPut("{assignmentId:int}")]
        [Authorize(Roles = "Mentor")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<AssignmentDto>> UpdateAssignment(int classId, int assignmentId, [FromForm] UpdateAssignmentForm form, CancellationToken ct)
        {
            var mentorId = GetUserId();
            var result = await _assignmentService.UpdateAssignmentAsync(assignmentId, form, mentorId, ct);

            if (result.ClassId != classId)
                return BadRequest("Assignment không thuộc class này.");

            return Ok(result);
        }

        /// <summary>
        /// Mentor xoá assignment
        /// </summary>
        [HttpDelete("{assignmentId:int}")]
        [Authorize(Roles = "Mentor")]
        public async Task<IActionResult> DeleteAssignment(
            int classId,
            int assignmentId,
            CancellationToken ct)
        {
            var mentorId = GetUserId();
            await _assignmentService.DeleteAssignmentAsync(assignmentId, mentorId, ct);
            return NoContent();
        }

        /// <summary>
        /// Lấy danh sách assignment của class.
        /// Mentor: xem tất cả.
        /// Staff: chỉ xem nếu thuộc class (Class.Employees).
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<List<AssignmentDto>>> GetAssignments(
            int classId,
            CancellationToken ct)
        {
            var userId = GetUserId();

            if (User.IsInRole("Mentor"))
            {
                var list = await _assignmentService.GetAssignmentsForClassAsync(classId, ct);
                return Ok(list);
            }
            else
            {
                var list = await _assignmentService.GetAssignmentsForStaffInClassAsync(classId, userId, ct);
                return Ok(list);
            }
        }

        /// <summary>
        /// Xem chi tiết 1 assignment thuộc class
        /// </summary>
        [HttpGet("{assignmentId:int}")]
        [Authorize]
        public async Task<ActionResult<AssignmentDto>> GetAssignmentById(
            int classId,
            int assignmentId,
            CancellationToken ct)
        {
            var userId = GetUserId();
            AssignmentDto? dto;

            if (User.IsInRole("Mentor"))
            {
                dto = await _assignmentService.GetAssignmentByIdAsync(assignmentId, ct);
            }
            else
            {
                dto = await _assignmentService.GetAssignmentForStaffAsync(assignmentId, userId, ct);
            }

            if (dto == null)
                return NotFound();

            if (dto.ClassId != classId)
                return BadRequest("Assignment không thuộc class này.");

            return Ok(dto);
        }
    }
}
