using InternalTrainingSystem.Core.Configuration;
using InternalTrainingSystem.Core.Constants;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Services.Implement;
using InternalTrainingSystem.Core.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
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

        public CourseController(ICourseService courseService, ICourseEnrollmentService courseEnrollmentService, 
            IHubContext<EnrollmentHub> hub, IUserService userService)
        {
            _courseService = courseService;
            _hub = hub;
            _courseEnrollmentService = courseEnrollmentService;
            _userService = userService;
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
            return success ? Ok(new { message = "Xóa thành công!" })
                           : NotFound(new { message = "Không tìm thấy course!" });
        }


        //[HttpPatch("{id:int}/status")]
        //[Authorize(Roles = UserRoles.TrainingDepartment)]
        //public IActionResult ToggleStatus(int id, [FromBody] ToggleStatusDto dto)
        //{
        //    var ok = _courseService.ToggleStatus(id, dto.Status);
        //    if (!ok) return NotFound(new { message = $"Course {id} not found" });

        //    return Ok(new { courseId = id, isActive = dto.Status });
        //}

        [HttpGet("search")]
        public async Task<ActionResult<PagedResult<CourseListItemDto>>> Search([FromQuery] CourseSearchRequest req,
            CancellationToken ct)
        {
            var result = await _courseService.SearchAsync(req, ct);
            return Ok(result);
        }

        [HttpGet("")]
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

        [HttpPost("by-identifiers")]
        public async Task<ActionResult<IEnumerable<CourseListItemDto>>> GetCoursesByIdentifiers(
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
        [ProducesResponseType(typeof(IEnumerable<CourseListItemDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CourseListItemDto>>> GetPendingCourses()
        {
            var items = await _courseService.GetPendingCoursesAsync();
            return Ok(items);
        }

        

        /// <summary>
        /// Ban giám đốc duyệt/từ chối khóa học đang Pending.
        /// </summary>
        [HttpPatch("{courseId}/status")]

        public async Task<IActionResult> UpdatePendingStatus(int courseId,[FromBody] UpdateCourseStatusRequest dto)
        {
            if (dto is null)
                return BadRequest(new { message = "Thiếu nội dung body." });

            try
            {
                var ok = await _courseService.UpdatePendingCourseStatusAsync(
                    courseId,
                    dto.NewStatus,
                    dto.Reason
                );

                if (!ok)
                {
                    // Service trả false khi: không tìm thấy hoặc trạng thái hiện tại không phải Pending
                    return Conflict(new
                    {
                        message = "Không thể cập nhật. Khóa học không tồn tại hoặc trạng thái hiện tại không phải Pending."
                    });
                }

                return NoContent(); // 204
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message, param = ex.ParamName });
            }
            catch (Exception ex)
            {
                // TODO: log ex
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "Đã xảy ra lỗi máy chủ.",
                    detail = ex.Message
                });
            }
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
            var enrollment = _courseEnrollmentService.GetCourseEnrollment(courseId, userId);

            if (enrollment == null)
            {
                return NotFound();
            }
            if (isConfirmed)
            {
                var deleted = _courseEnrollmentService.DeleteCourseEnrollment(courseId, userId);
                if (!deleted)
                    return BadRequest();

                await _hub.Clients.Group($"course-{courseId}")
                    .SendAsync("StaffListUpdated");

                return Ok(new { message = "Xác nhận xóa thành công! Đã xóa học viên." });
            }
            else
            {
                enrollment.Status = EnrollmentConstants.Status.Enrolled;
                var updated = _courseEnrollmentService.UpdateCourseEnrollment(enrollment);

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

            _courseEnrollmentService.AddCourseEnrollment(enrollment);

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
        public async Task<IActionResult> CreateFullCourse([FromForm] CreateFullCourseFormDto form,
                                                           CancellationToken ct)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }
            if (string.IsNullOrWhiteSpace(form.Metadata))
                return BadRequest(new { message = "metadata is required and must be a JSON string" });

            CreateFullCourseMetadataDto meta;
            try
            {
                meta = JsonSerializer.Deserialize<CreateFullCourseMetadataDto>(
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
                var course = await _courseService.CreateFullCourseAsync(
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

        /// <summary>
        /// Cập nhật bản nháp (hoặc bản bị từ chối) và gửi lại duyệt (đưa về trạng thái Pending).
        /// Chỉ hợp lệ khi khóa học đang Pending hoặc Reject.
        /// </summary>
        /// <param name="courseId">ID khóa học</param>
        /// <param name="dto">Nội dung cập nhật & danh sách phòng ban</param>
        /// <returns>NoContent nếu thành công; 409 nếu không đủ điều kiện; 400 nếu input sai.</returns>
        [HttpPut("{courseId}/resubmit")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ResubmitDraftAsync([FromRoute] int courseId,[FromBody] UpdateCourseRejectDto dto)
        {
            try
            {
                // Gọi service: trả true nếu lưu thành công, false nếu không tìm thấy
                // hoặc trạng thái hiện tại không cho phép resubmit.
                var ok = await _courseService.UpdateDraftAndResubmitAsync(courseId, dto);

                if (!ok)
                {
                    // Không phân biệt được NotFound vs InvalidState vì service trả bool.
                    // Quy ước: trả 409 Conflict, kèm message rõ nghĩa.
                    return Conflict(new
                    {
                        message = "Không thể gửi lại: khóa học không tồn tại hoặc không ở trạng thái Pending/Reject."
                    });
                }

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                // Lỗi validate đầu vào từ service (ví dụ tên rỗng, v.v.)
                return BadRequest(new
                {
                    message = ex.Message,
                    param = ex.ParamName
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "Đã xảy ra lỗi máy chủ khi xử lý yêu cầu."
                });
            }
        }
    }
}