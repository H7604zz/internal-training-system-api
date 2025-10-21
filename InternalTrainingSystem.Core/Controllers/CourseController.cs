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

        /// <summary>Hiển thị các course có status = Pending (Ban giám đốc duyệt).</summary>
        [HttpGet("pending")]
        [ProducesResponseType(typeof(IEnumerable<CourseListDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CourseListDto>>> GetPendingCourses()
        {
            var items = await _courseService.GetPendingCoursesAsync();
            return Ok(items);
        }

        public class UpdateCourseStatusRequest
        {
            public string NewStatus { get; set; } = default!;
        }

        /// <summary>Duyệt/ Từ chối 1 course đang Pending: newStatus = "Apporove" | "Reject".</summary>
        [HttpPut("{courseId:int}/status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdatePendingCourseStatus(int courseId, [FromBody] UpdateCourseStatusRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.NewStatus))
                return BadRequest("newStatus không được rỗng.");

            var ok = await _courseService.UpdatePendingCourseStatusAsync(courseId, request.NewStatus);
            if (!ok)
                return BadRequest("Chỉ có thể cập nhật trạng thái các khóa học đang ở Pending hoặc khóa học không tồn tại.");

            return Ok(new { message = $"Cập nhật trạng thái thành công: {request.NewStatus}" });
        }

        /// <summary>Chuyển 1 course từ Active -> Deleted (xóa mềm theo status).</summary>
        [HttpPut("{courseId:int}/delete")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteActiveCourse(int courseId)
        {
            var ok = await _courseService.DeleteActiveCourseAsync(courseId);
            if (!ok)
                return BadRequest("Chỉ có thể chuyển trạng thái các khóa học đang ở Active hoặc khóa học không tồn tại.");

            return Ok(new { message = "Khóa học đã được chuyển sang trạng thái Deleted." });
        }

    }
}
