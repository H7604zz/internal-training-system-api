using InternalTrainingSystem.Core.Configuration;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InternalTrainingSystem.Core.Constants;

namespace InternalTrainingSystem.Core.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseController : ControllerBase
    {
        private readonly ICourseService _courseService;

        public CourseController(ICourseService courseService)
        {
            _courseService = courseService;
        }

        // POST: /api/courses
        [HttpPost]
        public ActionResult<Course> Create([FromBody] CreateCourseDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var userId = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                         ?? "system";

            var now = DateTime.UtcNow;

            var entity = new Course
            {
                CourseName = dto.CourseName.Trim(),
                Description = dto.Description,
                CourseCategoryId = dto.CourseCategoryId,
                Duration = dto.Duration,
                Level = dto.Level,
                Status = CourseConstants.Status.Pending,
                CreatedDate = now,
                UpdatedDate = null,
                CreatedById = userId
            };

            var created = _courseService.CreateCourses(entity);
            if (created == null) return BadRequest(new { message = "Create course failed" });

            return CreatedAtAction(nameof(GetCourseDetail), new { id = created.CourseId }, created);
        }


        // PUT: /api/courses/5
        [HttpPut("{id:int}")]
        public IActionResult Update(int id, [FromBody] Course course)
        {
            if (id != course.CourseId)
                return BadRequest(new { message = "Id in route and body must match" });

            var ok = _courseService.UpdateCourses(course);
            if (!ok) return NotFound(new { message = $"Course {id} not found or update failed" });

            return NoContent();
        }

        // DELETE: /api/courses/5
        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            var ok = _courseService.DeleteCoursesByCourseId(id);
            if (!ok) return NotFound(new { message = $"Course {id} not found or delete failed" });

            return NoContent();
        }

        [HttpPatch("{id:int}/status")]
        public IActionResult ToggleStatus(int id, [FromBody] ToggleStatusDto dto)
        {
            var ok = _courseService.ToggleStatus(id, dto.Status);
            if (!ok) return NotFound(new { message = $"Course {id} not found" });

            return Ok(new { courseId = id, isActive = dto.Status });
        }

        [HttpGet("search")]
        public async Task<ActionResult<PagedResult<CourseListItemDto>>> Search([FromQuery] CourseSearchRequest req,
            CancellationToken ct)
        {
            var result = await _courseService.SearchAsync(req, ct);
            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CourseListDto>>> GetAllCourses()
        {
            try
            {
                var courses = await _courseService.GetAllCoursesAsync();
                return Ok(courses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpPost("by-identifiers")]
        public async Task<ActionResult<IEnumerable<CourseListDto>>> GetCoursesByIdentifiers(
            [FromBody] GetCoursesByIdentifiersRequest request)
        {
            try
            {
                if (request?.Identifiers == null || !request.Identifiers.Any())
                {
                    return BadRequest(new { message = "Identifiers list cannot be empty" });
                }

                var courses = await _courseService.GetCoursesByIdentifiersAsync(request.Identifiers);
                return Ok(courses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpGet("{id:int}/detail")]
        public async Task<ActionResult<CourseDetailDto>> GetCourseDetail(int id)
        {
            try
            {
                var course = await _courseService.GetCourseDetailAsync(id);
                if (course == null)
                {
                    return NotFound(new { message = "Course not found" });
                }

                return Ok(course);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

    }
}
