using InternalTrainingSystem.Core.Configuration;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InternalTrainingSystem.Core.Constants;
using System.Security.Claims;

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

        // GET: /api/courses
        //[HttpGet]
        //public ActionResult<IEnumerable<Course>> GetAll()
        //{
        //    var items = _courseService.GetAllCoursesAsync();
        //    return Ok(items);
        //}

        // GET: /api/courses/5
        [HttpGet("{id:int}")]
        public ActionResult<Course> GetById(int courseId)
        {
            var item = _courseService.GetCourseByCourseID(courseId);
            if (item == null) return NotFound(new { message = $"Course {courseId} not found" });
            return Ok(item);
        }

        // POST: /api/courses
        [HttpPost]
        public async Task<ActionResult<Course>> Create([FromBody] CreateCourseDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
            var now = DateTime.UtcNow;

            var entity = new Course
            {
                CourseName = dto.CourseName.Trim(),
                Description = dto.Description,
                CourseCategoryId = dto.CourseCategoryId,
                Duration = dto.Duration,
                Level = dto.Level, // đã có validation mặc định ở DTO
                Status = CourseConstants.Status.Pending,
                CreatedDate = now,
                UpdatedDate = null,
                CreatedById = userId
            };

            var created = await _courseService.CreateCourseAsync(entity, dto.Departments);
            if (created is null)
                return BadRequest(new { message = "Create course failed" });

            // 201 + Location header tới GET /api/courses/{id}
            return CreatedAtAction(nameof(GetById), new { id = created.CourseId }, created);
        }



        [HttpPut("{id:int}")]
        public async Task<ActionResult<Course>> Update(int id, [FromBody] UpdateCourseDto dto)
        {
            if (id != dto.CourseId)
                return BadRequest(new { message = "Course ID mismatch" });

            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var updated = await _courseService.UpdateCourseAsync(dto);
            if (updated is null)
                return NotFound(new { message = $"Course {id} not found" });

            return Ok(updated);
        }


        // DELETE: /api/courses/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var success = await _courseService.DeleteCourseAsync(id);
            return success ? Ok(new { message = "Xóa thành công!" })
                           : NotFound(new { message = "Không tìm thấy course!" });
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
