using InternalTrainingSystem.Core.Dto.Courses;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InternalTrainingSystem.Core.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseController : ControllerBase
    {
        private readonly ICourseService _service;

        public CourseController(ICourseService service)
        {
            _service = service;
        }

        // GET: /api/courses
        [HttpGet]
        public ActionResult<IEnumerable<Course>> GetAll()
        {
            var items = _service.GetCourses();
            return Ok(items);
        }

        // GET: /api/courses/5
        [HttpGet("{id:int}")]
        public ActionResult<Course> GetById(int id)
        {
            var item = _service.GetCourses().FirstOrDefault(x => x.CourseId == id);
            if (item == null) return NotFound(new { message = $"Course {id} not found" });
            return Ok(item);
        }

        // POST: /api/courses
        [HttpPost]
        public ActionResult<Course> Create([FromBody] CreateCourseDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            // Lấy user hiện tại (nếu có auth)
            var userId = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                         ?? "system"; // fallback nếu chưa có auth

            var now = DateTime.UtcNow;

            // Map DTO -> Entity, server tự set các field hệ thống
            var entity = new Course
            {
                CourseName = dto.CourseName.Trim(),
                Description = dto.Description,
                CourseCategoryId = dto.CourseCategoryId,
                Duration = dto.Duration,
                Level = dto.Level,
                IsActive = true,       // mặc định Active khi tạo (tùy business của bạn)
                CreatedDate = now,
                UpdatedDate = null,
                CreatedById = userId
            };

            var created = _service.CreateCourses(entity);
            if (created == null) return BadRequest(new { message = "Create course failed" });

            return CreatedAtAction(nameof(GetById), new { id = created.CourseId }, created);
        }


        // PUT: /api/courses/5
        [HttpPut("{id:int}")]
        public IActionResult Update(int id, [FromBody] Course course)
        {
            if (id != course.CourseId)
                return BadRequest(new { message = "Id in route and body must match" });

            var ok = _service.UpdateCourses(course);
            if (!ok) return NotFound(new { message = $"Course {id} not found or update failed" });

            return NoContent();
        }

        // DELETE: /api/courses/5
        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            var ok = _service.DeleteCoursesByCourseId(id);
            if (!ok) return NotFound(new { message = $"Course {id} not found or delete failed" });

            return NoContent();
        }

        public record ToggleStatusDto(bool IsActive);

        [HttpPatch("{id:int}/status")]
        public IActionResult ToggleStatus(int id, [FromBody] ToggleStatusDto dto)
        {
            var ok = _service.ToggleStatus(id, dto.IsActive);
            if (!ok) return NotFound(new { message = $"Course {id} not found" });

            return Ok(new { courseId = id, isActive = dto.IsActive });
        }

    }
}
