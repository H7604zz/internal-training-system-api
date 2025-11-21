using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using InternalTrainingSystem.Core.Utils;

namespace InternalTrainingSystem.Core.Controllers
{
    [ApiController]
    [Route("api/assignments/{assignmentId:int}/submissions")]
    public class AssignmentSubmissionsController : ControllerBase
    {
        private readonly IAssignmentService _assignmentService;
        private readonly IFileStorage _fileStorage;

        public AssignmentSubmissionsController(
            IAssignmentService assignmentService,
            IFileStorage fileStorage)
        {
            _assignmentService = assignmentService;
            _fileStorage = fileStorage;
        }

        private string GetUserId()
            => User.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? throw new InvalidOperationException("UserId not found in claims.");

        /// <summary>
        /// Mentor: xem toàn bộ bài nộp cho 1 assignment
        /// </summary>
        [HttpGet]
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
        /// Staff: xem các lần nộp của chính mình
        /// </summary>
        [HttpGet("my")]
        [Authorize(Roles = "Staff")]
        public async Task<ActionResult<List<AssignmentSubmissionSummaryDto>>> GetMySubmissions(
            int assignmentId,
            CancellationToken ct)
        {
            var userId = GetUserId();
            var list = await _assignmentService.GetMySubmissionsAsync(assignmentId, userId, ct);
            return Ok(list);
        }

        /// <summary>
        /// Mentor + Staff: xem chi tiết một submission
        /// </summary>
        [HttpGet("{submissionId:int}")]
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
        /// Staff: nộp bài, MỖI LẦN CHỈ 1 FILE
        /// </summary>
        [HttpPost]
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

            // Meta cho S3: chuẩn hóa ContentType + ContentDisposition UTF-8
            var meta = StorageObjectMetadata.ForUpload(form.File.FileName, form.File.ContentType);

            var subFolder = $"assignments/{assignmentId}";
            var (url, relativePath) = await _fileStorage.SaveAsync(form.File, subFolder, meta, ct);

            var uploadedFile = (
                fileName: form.File.FileName,
                relativePath: relativePath,
                url: url,
                mimeType: form.File.ContentType,
                sizeBytes: (long?)form.File.Length
            );

            var request = new CreateSubmissionRequest
            {
                Note = form.Note
            };

            var result = await _assignmentService.CreateSubmissionAsync(
                assignmentId,
                userId,
                request,
                uploadedFile,
                ct);

            return CreatedAtAction(
                nameof(GetSubmissionDetail),
                new { assignmentId = assignmentId, submissionId = result.SubmissionId },
                result);
        }

        /// <summary>
        /// Mentor: chấm điểm một submission
        /// </summary>
        [HttpPut("{submissionId:int}/grade")]
        [Authorize(Roles = "Mentor")]
        public async Task<ActionResult<AssignmentSubmissionDetailDto>> GradeSubmission(
            int assignmentId,
            int submissionId,
            [FromBody] GradeSubmissionDto dto,
            CancellationToken ct)
        {
            var mentorId = GetUserId();
            var result = await _assignmentService.GradeSubmissionAsync(submissionId, mentorId, dto, ct);

            if (result.AssignmentId != assignmentId)
                return BadRequest("Submission không thuộc assignment này.");

            return Ok(result);
        }
    }
}
