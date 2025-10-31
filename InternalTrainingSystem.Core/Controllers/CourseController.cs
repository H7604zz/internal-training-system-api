using InternalTrainingSystem.Core.Configuration;
using InternalTrainingSystem.Core.Constants;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.Json;

namespace InternalTrainingSystem.Core.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseController : ControllerBase
    {
        private readonly ICourseService _courseService;
        private readonly IUserService _userService;
        private readonly ICourseEnrollmentService _courseEnrollmentService;
        private readonly IHubContext<EnrollmentHub> _hub;
        private readonly ICategoryService _categoryService;

        public CourseController(ICourseService courseService, ICourseEnrollmentService courseEnrollmentService, 
            IHubContext<EnrollmentHub> hub, IUserService userService, ICategoryService categoryService)
        {
            _courseService = courseService;
            _hub = hub;
            _courseEnrollmentService = courseEnrollmentService;
            _userService = userService;
            _categoryService = categoryService;
        } 

        // PUT: /api/courses/5
        [HttpPut("{id:int}")]
        [Authorize(Roles = UserRoles.TrainingDepartment)]
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
        [Authorize(Roles = UserRoles.TrainingDepartment)]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var success = await _courseService.DeleteCourseAsync(id);
            return success ? Ok("Xóa thành công!") : NotFound("Không tìm thấy course!");
        }


        [HttpPatch("{id:int}/status")]
        [Authorize(Roles = UserRoles.TrainingDepartment)]
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

        [HttpGet()]
        public async Task<ActionResult<PagedResult<CourseListItemDto>>> GetAllCoursesPaged([FromQuery] GetAllCoursesRequest request)
        {
            try
            {
                var result = await _courseService.GetAllCoursesPagedAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest( new { message = "Internal server error", error = ex.Message });
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
        [ProducesResponseType(typeof(IEnumerable<CourseListItemDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CourseListItemDto>>> GetPendingCourses()
        {
            var request = new GetAllCoursesRequest
            {
                Page = 1,
                PageSize = int.MaxValue,
                Status = CourseConstants.Status.Pending
            };
            var items = await _courseService.GetAllCoursesPagedAsync(request);
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
        [HttpPatch("{courseId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteActiveCourse(int courseId, [FromBody] string? rejectReason)
        {
            if (string.IsNullOrWhiteSpace(rejectReason))
                rejectReason = "Khóa học bị xóa bởi Ban giám đốc.";

            var ok = await _courseService.DeleteActiveCourseAsync(courseId, rejectReason);

            if (!ok)
                return BadRequest("Chỉ có thể xóa các khóa học đang ở trạng thái Active hoặc khóa học không tồn tại.");

            return Ok(new { message = "Khóa học đã được chuyển sang trạng thái Deleted.", reason = rejectReason });
        }

        [HttpPost("{courseId}/enrollments/{userId}/confirm")]
        [Authorize(Roles = UserRoles.DirectManager)]
        public async Task<IActionResult> ConfirmEnrollment(int courseId, string userId, [FromQuery] bool isConfirmed)
        {
            var enrollment = await _courseEnrollmentService.GetCourseEnrollment(courseId, userId);

            if (enrollment == null)
            {
                return NotFound();
            }
            if (isConfirmed)
            {
                var deleted = await _courseEnrollmentService.DeleteCourseEnrollment(courseId, userId);
                if (!deleted)
                    return BadRequest();

                await _hub.Clients.Group($"course-{courseId}")
                    .SendAsync("StaffListUpdated");

                return Ok(new { message = "Xác nhận xóa thành công! Đã xóa học viên." });
            }
            else
            {
                enrollment.Status = EnrollmentConstants.Status.Enrolled;
                var updated = await _courseEnrollmentService.UpdateCourseEnrollment(enrollment);

                if (!updated)
                    return BadRequest();

                await _hub.Clients.Group($"course-{courseId}")
                    .SendAsync("StaffListUpdated");

                return Ok(new { message = "Trạng thái đã được cập nhật." });
            }
        }

        [HttpPost("{courseId}/enrollments/{userId}/status")]
        [Authorize(Roles = UserRoles.Staff)]
        public async Task<IActionResult> UpdateEnrollmentStatus(int courseId, string userId, [FromBody] EnrollmentStatusUpdateRequest request)
        {
            var course = _courseService.GetCourseByCourseID(courseId);
            if (course == null)
                return NotFound();


            var enrollment = new CourseEnrollment
            {
                CourseId = course.CourseId,
                UserId = userId,
                EnrollmentDate = DateTime.UtcNow,
                LastAccessedDate = DateTime.UtcNow
            };

            if (course.IsOnline || course.IsMandatory)
            {
                enrollment.Status = EnrollmentConstants.Status.Enrolled;
            }
            else
            {
                if (request.IsConfirmed)
                {
                    enrollment.Status = EnrollmentConstants.Status.Enrolled;
                }
                else
                {
                    enrollment.Status = EnrollmentConstants.Status.Dropped;
                    enrollment.RejectionReason = string.IsNullOrWhiteSpace(request.Reason) ? "Không cung cấp lý do" : request.Reason;
                }
            }

            await _courseEnrollmentService.AddCourseEnrollment(enrollment);

            await _hub.Clients.Group($"course-{courseId}")
            .SendAsync("EnrollmentStatusChanged", new
            {
                CourseId = courseId,
                UserId = userId,
                Status = enrollment.Status,
                Reason = enrollment.RejectionReason
            });

            return Ok();
        }

        [HttpGet("{courseId}/eligible-staff")]
        //[Authorize(Roles = UserRoles.DirectManager + "," + UserRoles.TrainingDepartment)]
        public IActionResult GetEligibleUsers(int courseId, [FromQuery] UserSearchDto searchDto)
        {
            var result = _userService.GetEligibleStaff(courseId, searchDto);
            return Ok(result);
        }

        // POST: /api/courses
        [HttpPost]
        [RequestSizeLimit(600 * 1024 * 1024)]
        //[Authorize(Roles = UserRoles.TrainingDepartment)]
        public async Task<IActionResult> CreateCourse([FromForm] CreateCourseFormDto form, CancellationToken ct)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }
            if (string.IsNullOrWhiteSpace(form.Metadata))
                return BadRequest(new { message = "metadata is required and must be a JSON string" });

            CreateCourseMetadataDto meta;
            try
            {
                meta = JsonSerializer.Deserialize<CreateCourseMetadataDto>(
                    form.Metadata,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                ) ?? throw new ArgumentException("metadata invalid");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Invalid metadata JSON", error = ex.Message });
            }
            var validationResults = new List<ValidationResult>();
            var context = new ValidationContext(meta, serviceProvider: HttpContext.RequestServices, items: null);
            bool isValid = Validator.TryValidateObject(meta, context, validationResults, validateAllProperties: true);
            foreach (var module in meta.Modules)
            {
                var moduleContext = new ValidationContext(module, serviceProvider: HttpContext.RequestServices, items: null);
                isValid &= Validator.TryValidateObject(module, moduleContext, validationResults, true);

                foreach (var lesson in module.Lessons)
                {
                    var lessonContext = new ValidationContext(lesson, serviceProvider: HttpContext.RequestServices, items: null);
                    isValid &= Validator.TryValidateObject(lesson, lessonContext, validationResults, true);
                }
            }
            if (!isValid)
            {
                var errors = validationResults.Select(r => r.ErrorMessage).Where(msg => !string.IsNullOrWhiteSpace(msg)).ToList();
                return BadRequest(new { message = "Validation failed", errors });
            }
            if (await _courseService.GetCourseByCourseCodeAsync(meta.CourseCode) != null)
                return Conflict(new { message = $"Mã khóa học '{meta.CourseCode}' đã tồn tại. Vui lòng chọn mã khác." });
            try
            {
                var course = await _courseService.CreateCourseAsync(
                    meta,
                    form.LessonFiles,
                    userId,
                    ct
                );

                return CreatedAtAction(nameof(GetCourseDetail),
                    new { id = course.CourseId },
                    new { course.CourseId, course.CourseName });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpGet("{courseId}/confirmed-staff")]
        [Authorize(Roles = UserRoles.DirectManager + "," + UserRoles.TrainingDepartment)]
        public IActionResult GetConfirmedUsers(int courseId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var confirmedUsers = _userService.GetStaffConfirmCourse(courseId, page, pageSize);
            return Ok(confirmedUsers);

        }

        [HttpGet("/categories")]
        public ActionResult<IEnumerable<CourseCategory>> GetAll()
        {
            var items = _categoryService.GetCategories();
            return Ok(items);
        }
    }
}