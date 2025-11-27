using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using InternalTrainingSystem.Core.Common.Constants;
using Microsoft.AspNetCore.Mvc;
using InternalTrainingSystem.Core.Services.Implement;
using InternalTrainingSystem.Core.Utils;

namespace InternalTrainingSystem.Core.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AssignmentController : ControllerBase
    {
        private readonly IAssignmentService _assignmentService;
        private readonly IFileStorage _fileStorage;

        public AssignmentController(IAssignmentService assignmentService, IFileStorage fileStorage)
        {
            _assignmentService = assignmentService;
            _fileStorage = fileStorage;
        }
        private string GetUserId()
            => User.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? throw new InvalidOperationException("UserId not found in claims.");

        /// <summary>
        /// Mentor tạo assignment cho CLASS offline
        /// </summary>
        [HttpPost("{classId:int}")]
        [Authorize(Roles = UserRoles.Mentor)]
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
        [HttpPut("{classId:int}/{assignmentId:int}")]
        [Authorize(Roles = UserRoles.Mentor)]
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
        [HttpDelete("{classId:int}/{assignmentId:int}")]
        [Authorize(Roles = UserRoles.Mentor)]
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
        /// lấy bài cuối kì của class.
        /// Mentor: xem tất cả.
        /// Staff: chỉ xem nếu thuộc class (Class.Employees).
        /// </summary>
        [HttpGet("{classId:int}")]
        [Authorize]
        public async Task<ActionResult<AssignmentDto?>> GetAssignment(
    int classId,
    CancellationToken ct)
        {
            var userId = GetUserId();

            AssignmentDto? assignment;

            if (User.IsInRole(UserRoles.Mentor))
            {
                assignment = await _assignmentService.GetAssignmentForClassAsync(classId, ct);
            }
            else
            {
                assignment = await _assignmentService.GetAssignmentForStaffInClassAsync(classId, userId, ct);
            }

            if (assignment == null)
                return NotFound("Lớp này chưa có assignment.");

            return Ok(assignment);
        }

        /// <summary>
        /// Xem chi tiết 1 assignment thuộc class
        /// </summary>
        [HttpGet("{classId:int}/{assignmentId:int}")]
        [Authorize]
        public async Task<ActionResult<AssignmentDto>> GetAssignmentById(
            int classId,
            int assignmentId,
            CancellationToken ct)
        {
            var userId = GetUserId();
            AssignmentDto? dto;

            if (User.IsInRole(UserRoles.Mentor))
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


        /// <summary>
        /// Mentor: xem toàn bộ bài nộp cho 1 assignment
        /// </summary>
        [HttpGet("{assignmentId:int}/submissions")]
        [Authorize(Roles = "Mentor")]
        public async Task<ActionResult<List<AssignmentSubmissionSummaryDto>>> GetSubmissions(
            int assignmentId,
            CancellationToken ct)
        {
            var mentorId = GetUserId();
            var list = await _assignmentService.GetSubmissionsForAssignmentAsync(assignmentId, mentorId, ct);
            return Ok(list);
        }

        /// <summary>
        /// Mentor + Staff: xem chi tiết một submission
        /// </summary>
        [HttpGet("{assignmentId:int}/submissions/{submissionId:int}")]
        [Authorize]
        public async Task<ActionResult<AssignmentSubmissionDetailDto>> GetSubmissionDetail(
            int assignmentId,
            int submissionId,
            CancellationToken ct)
        {
            var userId = GetUserId();
            var isMentor = User.IsInRole("Mentor");

            var dto = await _assignmentService.GetSubmissionDetailAsync(
                submissionId, userId, isMentor, ct);

            if (dto == null || dto.AssignmentId != assignmentId)
                return NotFound();

            return Ok(dto);
        }

        /// <summary>
        /// Staff: nộp bài, MỖI LẦN CHỉ 1 FILE
        /// </summary>
        [HttpPost("{assignmentId:int}/submissions")]
        [Authorize(Roles = "Staff")]
        [RequestSizeLimit(100_000_000)]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<AssignmentSubmissionDetailDto>> SubmitAssignment(
            int assignmentId,
            [FromForm] SubmitAssignmentForm form,
            CancellationToken ct)
        {
            var userId = GetUserId();

            if (form.File == null || form.File.Length <= 0)
                return BadRequest("Vui lòng chọn 1 file để nộp.");

            var meta = StorageObjectMetadata.ForUpload(form.File.FileName, form.File.ContentType);
            var subFolder = $"assignments/{assignmentId}";

            var uploaded = await _fileStorage.SaveAsync(form.File, subFolder, meta, ct);

            var uploadedFile = (
                fileName: form.File.FileName,
                relativePath: uploaded.relativePath,
                url: uploaded.url,
                mimeType: form.File.ContentType,
                sizeBytes: (long?)form.File.Length
            );

            var result = await _assignmentService.CreateSubmissionAsync(
                assignmentId,
                userId,
                uploadedFile,
                ct);

            return CreatedAtAction(
                nameof(GetSubmissionDetail),
                new {assignmentId = assignmentId, submissionId = result.SubmissionId },
                result);
        }
    }
}
