using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace InternalTrainingSystem.Core.Controllers
{
    [ApiController]
    [Route("api/modules/{moduleId:int}/lessons")]
    public class LessonController : ControllerBase
    {
        private readonly ICourseMaterialService _courseMaterialService;

        public LessonController(ICourseMaterialService cms)
        {
            _courseMaterialService = cms;
        }

        // GET: /api/modules/{moduleId}/lessons
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LessonListItemDto>>> GetLessons(int moduleId, CancellationToken ct)
        {
            var items = await _courseMaterialService.GetLessonsByModuleAsync(moduleId, ct);
            return Ok(items);
        }

        // GET: /api/modules/{moduleId}/lessons/{lessonId}
        [HttpGet("{lessonId:int}")]
        public async Task<ActionResult<LessonListItemDto>> GetLesson(int moduleId, int lessonId, CancellationToken ct)
        {
            var l = await _courseMaterialService.GetLessonAsync(lessonId, ct);
            if (l == null || l.ModuleId != moduleId) return NotFound(new { message = "Lesson not found" });
            return Ok(l);
        }

        // POST: /api/modules/{moduleId}/lessons
        [HttpPost]
        public async Task<ActionResult> CreateLesson(int moduleId, [FromBody] CreateLessonDto dto, CancellationToken ct)
        {
            if (dto.ModuleId != moduleId)
                return BadRequest(new { message = "moduleId in route and body must match" });

            var created = await _courseMaterialService.CreateLessonAsync(dto, ct);
            return CreatedAtAction(nameof(GetLesson),
                new { moduleId, lessonId = created.Id },
                new { created.Id, created.ModuleId, created.Title, created.Type });
        }

        // PUT: /api/modules/{moduleId}/lessons/{lessonId}
        [HttpPut("{lessonId:int}")]
        public async Task<IActionResult> UpdateLesson(int moduleId, int lessonId, [FromBody] UpdateLessonDto dto, CancellationToken ct)
        {
            var exists = await _courseMaterialService.GetLessonAsync(lessonId, ct);
            if (exists == null || exists.ModuleId != moduleId)
                return NotFound(new { message = "Lesson not found" });

            var ok = await _courseMaterialService.UpdateLessonAsync(lessonId, dto, ct);
            return ok ? NoContent() : NotFound(new { message = "Update failed" });
        }

        // DELETE: /api/modules/{moduleId}/lessons/{lessonId}
        [HttpDelete("{lessonId:int}")]
        public async Task<IActionResult> DeleteLesson(int moduleId, int lessonId, CancellationToken ct)
        {
            var exists = await _courseMaterialService.GetLessonAsync(lessonId, ct);
            if (exists == null || exists.ModuleId != moduleId)
                return NotFound(new { message = "Lesson not found" });

            var ok = await _courseMaterialService.DeleteLessonAsync(lessonId, ct);
            return ok ? NoContent() : NotFound(new { message = "Delete failed" });
        }
        [HttpPost("{lessonId:int}/file")]
        [RequestSizeLimit(20 * 1024 * 1024)] // 20 MB
        public async Task<IActionResult> UploadLessonFile(int moduleId, int lessonId, IFormFile file, CancellationToken ct)
        {
            // exist check
            var l = await _courseMaterialService.GetLessonAsync(lessonId, ct);
            if (l == null || l.ModuleId != moduleId)
                return NotFound(new { message = "Lesson not found" });

            try
            {
                var (url, _) = await _courseMaterialService.UploadLessonFileAsync(lessonId, file, ct);
                return Ok(new { lessonId, fileUrl = url });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{lessonId:int}/file")]
        public async Task<IActionResult> RemoveLessonFile(int moduleId, int lessonId, CancellationToken ct)
        {
            var l = await _courseMaterialService.GetLessonAsync(lessonId, ct);
            if (l == null || l.ModuleId != moduleId)
                return NotFound(new { message = "Lesson not found" });

            var ok = await _courseMaterialService.ClearLessonFileAsync(lessonId, ct);
            return ok ? NoContent() : NotFound(new { message = "Clear failed" });
        }
    }
}
