using DocumentFormat.OpenXml.Spreadsheet;
using InternalTrainingSystem.Core.Configuration;
using InternalTrainingSystem.Core.Constants;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Hubs;
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
        private readonly INotificationService _notificationService;
        private readonly IHubContext<NotificationHub> _hub;

        public CourseController(ICourseService courseService, ICourseEnrollmentService courseEnrollmentService,
            IHubContext<NotificationHub> hub, IUserService userService, INotificationService notificationService)
        {
            _courseService = courseService;
            _hub = hub;
            _courseEnrollmentService = courseEnrollmentService;
            _userService = userService;
            _notificationService = notificationService;
        }

        // POST: /api/courses
        [HttpPost]
        [Authorize(Roles = UserRoles.TrainingDepartment)]
        public async Task<ActionResult<Course>> Create([FromBody] CreateCourseDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            // Chuẩn hóa dữ liệu đầu vào
            var code = (dto.CourseCode ?? string.Empty).Trim();
            var name = (dto.CourseName ?? string.Empty).Trim();

            // Kiểm tra mã khóa học trùng (case-insensitive)
            if (await _courseService.GetCourseByCourseCodeAsync(code) != null)
                return Conflict(new { message = $"Mã khóa học '{code}' đã tồn tại. Vui lòng chọn mã khác." });

            var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
            var now = DateTime.UtcNow;

            var entity = new Course
            {
                Code = code,
                CourseName = name,
                Description = dto.Description,
                CourseCategoryId = dto.CourseCategoryId,
                Duration = dto.Duration,
                Level = dto.Level, // đã được DTO validate
                Status = CourseConstants.Status.Pending, // server kiểm soát trạng thái ban đầu
                CreatedDate = now,
                UpdatedDate = null,
                CreatedById = userId
            };

            try
            {
                var created = await _courseService.CreateCourseAsync(entity, dto.Departments);

                // Phòng hờ service trả null (không mong đợi)
                if (created is null)
                    return BadRequest(new { message = "Tạo khóa học thất bại." });

                return CreatedAtAction(nameof(GetCourseDetail), new { id = created.CourseId }, created);
            }
            catch (ArgumentException ex)
            {
                // Ví dụ: Department ID không tồn tại, hoặc các lỗi business hợp lệ
                return BadRequest(new { message = ex.Message });
            }
            catch
            {
                // Lỗi không xác định
                return StatusCode(500, new { message = "Đã xảy ra lỗi máy chủ khi tạo khóa học." });
            }
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
                return BadRequest(new { message = "Internal server error", error = ex.Message });
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
                EnrollmentDate = DateTime.Now,
                LastAccessedDate = DateTime.Now
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

        [HttpPost("full")]
        [RequestSizeLimit(600 * 1024 * 1024)]
        //[Authorize(Roles = UserRoles.TrainingDepartment)]
        public async Task<IActionResult> CreateFullCourse(
                                                          [FromForm] CreateFullCourseFormDto form,
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

        [HttpPost("{courseId}/finalize-enrollments")]
        [Authorize(Roles = UserRoles.TrainingDepartment)]
        public async Task<IActionResult> FinalizeEnrollments(string courseCode)
        {

            var course = await _courseService.GetCourseByCourseCodeAsync(courseCode);
            if (course == null) return BadRequest();

            var existingNotification = _notificationService.GetNotificationByCourseAndType(course.CourseId, NotificationType.CourseFinalized);

            if (existingNotification != null)
            {
                return Ok(ApiResponseDto.SuccessResult(new { sent = false }, "Thông báo đã được gửi trước đó."));
            }

            var searchDto = new UserSearchDto
            {
                Page = 1,
                PageSize = int.MaxValue,
            };

            var eligiblePaged = _userService.GetEligibleStaff(course.CourseId, searchDto);
            var enrollmentsToAdd = new List<CourseEnrollment>();
            foreach (var user in eligiblePaged.Items)
            { 
                if(user.Status == EnrollmentConstants.Status.NotEnrolled)
                {
                    enrollmentsToAdd.Add(new CourseEnrollment
                    {
                        CourseId = course.CourseId,
                        UserId = user.EmployeeId!,
                        Status = EnrollmentConstants.Status.Enrolled,
                        EnrollmentDate = DateTime.Now,
                        LastAccessedDate = DateTime.Now
                    });
                }
            }
            if (enrollmentsToAdd.Any())
            {
                await _courseEnrollmentService.AddRangeAsync(enrollmentsToAdd);
            }

            await _notificationService.SaveNotificationAsync(new Notification
            {
                CourseId = course.CourseId,
                Type = NotificationType.CourseFinalized,
                SentAt = DateTime.Now,
            });

            await _hub.Clients.Group($"finalize-enrollments-{course.Code}").SendAsync("ReceiveNotification", new
            {
                CourseId = course.Code,
                Type = "EnrollmentsFinalized",
                Message = "Danh sách nhân viên trong khóa học đã được chốt, vui lòng xem xét."
            });

            return Ok(ApiResponseDto.SuccessResult(null, "Danh sách nhân viên tham gia khóa học đã được chốt thành công."));
        }
    }
}