using DocumentFormat.OpenXml.Spreadsheet;
using InternalTrainingSystem.Core.Common;
using InternalTrainingSystem.Core.Common.Constants;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Hubs;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Services.Implement;
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
        private readonly INotificationService _notificationService;
        private readonly IHubContext<NotificationHub> _hub;
        private readonly ICategoryService _categoryService;
        private readonly ICourseHistoryService _courseHistoryService;
        private readonly ICourseMaterialService _courseMaterialService;
        private readonly IClassService _classService;

        public CourseController(ICourseService courseService, ICourseEnrollmentService courseEnrollmentService,
            IHubContext<NotificationHub> hub, IUserService userService, INotificationService notificationService,
            ICategoryService categoryService, ICourseMaterialService courseMaterialService, ICourseHistoryService courseHistoryService,
            IClassService classService)
        {
            _courseService = courseService;
            _hub = hub;
            _courseEnrollmentService = courseEnrollmentService;
            _userService = userService;
            _categoryService = categoryService;
            _notificationService = notificationService;
            _courseHistoryService = courseHistoryService;
            _courseMaterialService = courseMaterialService;
            _classService = classService;
        }

        // PUT: /api/courses/{id}
        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(600 * 1024 * 1024)]
        //[Authorize(Roles = UserRoles.TrainingDepartment)]
        public async Task<IActionResult> UpdateCourse(
            int id,
            [FromForm] UpdateCourseFormDto form,
            CancellationToken ct)
        {
            // 1) Xác thực user
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // 2) Đọc & parse metadata JSON
            if (string.IsNullOrWhiteSpace(form.Metadata))
                return BadRequest(new { message = "metadata is required and must be a JSON string" });

            UpdateCourseMetadataDto meta;
            try
            {
                meta = JsonSerializer.Deserialize<UpdateCourseMetadataDto>(
                    form.Metadata,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                ) ?? throw new ArgumentException("metadata invalid");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Invalid metadata JSON", error = ex.Message });
            }

            // 3) Validate đệ quy (object + modules + lessons)
            var validationResults = new List<ValidationResult>();
            var context = new ValidationContext(meta, HttpContext.RequestServices, null);
            bool isValid = Validator.TryValidateObject(meta, context, validationResults, validateAllProperties: true);

            foreach (var module in meta.Modules)
            {
                var mctx = new ValidationContext(module, HttpContext.RequestServices, null);
                isValid &= Validator.TryValidateObject(module, mctx, validationResults, true);

                foreach (var lesson in module.Lessons)
                {
                    var lctx = new ValidationContext(lesson, HttpContext.RequestServices, null);
                    isValid &= Validator.TryValidateObject(lesson, lctx, validationResults, true);
                }
            }

            if (!isValid)
            {
                var errors = validationResults
                    .Select(r => r.ErrorMessage)
                    .Where(msg => !string.IsNullOrWhiteSpace(msg))
                    .ToList();

                return BadRequest(new { message = "Validation failed", errors });
            }

            // 4) Gọi service update
            try
            {
                var updated = await _courseService.UpdateCourseAsync(
                    id,
                    meta,
                    form.LessonFiles,
                    userId,
                    ct);

                return Ok(new { updated.CourseId, updated.CourseName });
            }
            catch (OperationCanceledException)
            {
                // Client hủy request hoặc server yêu cầu hủy
                return StatusCode(StatusCodes.Status499ClientClosedRequest,
                    new { message = "Request was cancelled." });
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

        //api này không cần thiết
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
                return BadRequest(new { message = "Internal server error", error = ex.Message });
            }
        }


        /// <summary>
        /// Lấy chi tiết khóa học theo ID.
        /// </summary>
        /// <param name="id">CourseId</param>
        [HttpGet("{id:int}/detail")]
        public async Task<IActionResult> GetCourseDetail([FromRoute][Required] int id, CancellationToken ct)
        {
            var dto = await _courseService.GetCourseDetailAsync(id, ct);
            if (dto is null)
                return NotFound(new { message = $"Course with ID {id} not found." });

            return Ok(dto);
        }

        [HttpPatch("update-pending-status/{courseId}")]
        //[Authorize(Roles = UserRoles.DirectManager)]
        public async Task<IActionResult> UpdatePendingCourseStatus(int courseId, [FromBody] UpdatePendingCourseStatusRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }

            try
            {
                var result = await _courseService.UpdatePendingCourseStatusAsync(userId, courseId, request.NewStatus, request.RejectReason);
                if (!result)
                    return BadRequest("Không thể cập nhật trạng thái. Có thể khóa học không tồn tại hoặc không ở trạng thái Pending.");
                
                await _notificationService.NotifyTrainingDepartmentAsync(courseId);
                
                return Ok(new
                {
                    message = $"Cập nhật trạng thái khóa học {courseId} thành công: {request.NewStatus}",
                    reason = request.RejectReason
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                // Log lỗi nếu cần
                return StatusCode(500, "Đã xảy ra lỗi khi cập nhật trạng thái khóa học.");
            }
        }

        /// <summary>Chuyển 1 course từ Active -> Deleted (xóa mềm theo status).</summary>
        [HttpPatch("{courseId}")]

        public async Task<IActionResult> DeleteActiveCourse(int courseId, [FromBody] string? rejectReason)
        {
            if (string.IsNullOrWhiteSpace(rejectReason))
                rejectReason = "Khóa học bị xóa bởi Ban giám đốc.";

            var ok = await _courseService.DeleteActiveCourseAsync(courseId, rejectReason);

            if (!ok)
                return BadRequest("Chỉ có thể xóa các khóa học đang ở trạng thái Active hoặc khóa học không tồn tại.");

            return Ok(new { message = "Khóa học đã được chuyển sang trạng thái Deleted.", reason = rejectReason });
        }

        /// <summary>
        /// DirectManager comfirm li do cua staff
        /// </summary>
        /// <param name="courseId"></param>
        /// <param name="isConfirmed"></param>
        /// <returns></returns>
        [HttpPost("{courseId}/enrollments/confirm")]
        [Authorize(Roles = UserRoles.DirectManager)]
        public async Task<IActionResult> ConfirmEnrollment(int courseId, [FromBody] ConfirmReasonRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            } 

            var enrollment = await _courseEnrollmentService.GetCourseEnrollment(courseId, request.UserId);

            if (enrollment == null)
            {
                return NotFound();
            }
            if (request.IsConfirmed)
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

        /// <summary>
        /// staff confirm khoa hoc
        /// </summary>
        /// <param name="courseId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPatch("{courseId}/enrollments/status")]
        [Authorize(Roles = UserRoles.Staff)]
        public async Task<IActionResult> UpdateEnrollmentStatus(int courseId, [FromBody] EnrollmentStatusUpdateRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }

            var course = await _courseService.GetCourseByCourseIdAsync(courseId);
            if (course == null)
                return NotFound();

            var enrollment = await _courseEnrollmentService.GetCourseEnrollment(courseId, userId);
            if (enrollment == null)
                return NotFound();

            if (course.IsMandatory)
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

            await _courseEnrollmentService.UpdateCourseEnrollment(enrollment);

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
        [Authorize(Roles = UserRoles.TrainingDepartment)]
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
        //[Authorize(Roles = UserRoles.DirectManager + "," + UserRoles.TrainingDepartment)]
        public IActionResult GetConfirmedUsers(int courseId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var notice = _notificationService.GetNotificationByCourseAndType(courseId, NotificationType.CourseFinalized);
            if (notice == null)
            {
                return Ok("Danh sách nhân viên chưa được chốt !!!");
            }
            var confirmedUsers = _userService.GetStaffConfirmCourse(courseId, page, pageSize);
            return Ok(confirmedUsers);

        }

        [HttpGet("{courseId}/confirmed-staff/count")]
        //[Authorize(Roles = UserRoles.DirectManager + "," + UserRoles.TrainingDepartment)]
        public IActionResult GetConfirmedUsersCount(int courseId)
        {
            var confirmedUsers = _userService.GetStaffConfirmCourse(courseId, 1, int.MaxValue);
            int countStaff = confirmedUsers.TotalCount;

            return Ok(countStaff);
        }

        [HttpPost("{courseId}/finalize-enrollments")]
        //[Authorize(Roles = UserRoles.DirectManager)]
        public async Task<IActionResult> FinalizeEnrollments(int courseId)
        {
            var course = await _courseService.GetCourseByCourseIdAsync(courseId);
            if (course == null) return BadRequest();

            var existingNotification = _notificationService.GetNotificationByCourseAndType(course.CourseId, NotificationType.CourseFinalized);

            if (existingNotification != null)
            {
                return Ok("Thông báo đã được gửi trước đó.");
            }

            var searchDto = new UserSearchDto
            {
                Page = 1,
                PageSize = int.MaxValue,
                Status = EnrollmentConstants.Status.NotEnrolled
            };

            var eligiblePaged = _userService.GetEligibleStaff(course.CourseId, searchDto);
            await _courseEnrollmentService.BulkUpdateEnrollmentsToEnrolledAsync(eligiblePaged.Items.ToList(), courseId);

            await _notificationService.SaveNotificationAsync(
                new Notification
                {
                    CourseId = course.CourseId,
                    Message = $"Danh sách nhân viên trong khóa học khoá học {course.CourseName} đã được hoàn tất.",
                    Type = NotificationType.CourseFinalized,
                    SentAt = DateTime.Now,
                },
                roleNames: new List<string> { UserRoles.TrainingDepartment }
            );

            await _hub.Clients.Group($"finalize-enrollments-{course.Code}").SendAsync("ReceiveNotification", new
            {
                CourseId = course.Code,
                Type = "EnrollmentsFinalized",
                Message = "Danh sách nhân viên trong khóa học đã được chốt, vui lòng xem xét."
            });

            return Ok("Danh sách nhân viên tham gia khóa học đã được chốt thành công.");
        }

        //HttpGet /categories
        [HttpGet("/categories")]
        public ActionResult<IEnumerable<CourseCategory>> GetCategories()
        {
            var items = _categoryService.GetCategories();
            return Ok(items);
        }

        [HttpPut("{id:int}/resubmit")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ResubmitAfterReject(int id, [FromForm(Name = "metadata")] string metadata, [FromForm] List<IFormFile> lessonFiles, [FromForm] string? resubmitNote,
        CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(metadata))
                return BadRequest(new { message = "metadata is required and must be a JSON string" });

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            UpdateCourseMetadataDto? dto;
            try
            {
                dto = JsonSerializer.Deserialize<UpdateCourseMetadataDto>(metadata, options);
                if (dto == null) throw new JsonException();
            }
            catch
            {
                return BadRequest(new { message = "metadata is not valid JSON" });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? User.Identity?.Name
                        ?? "system";

            try
            {
                var course = await _courseService.UpdateAndResubmitToPendingAsync(
                    id, dto, lessonFiles, userId, resubmitNote, ct);

                return Ok(new
                {
                    message = "Resubmitted to Pending successfully.",
                    courseId = course.CourseId,
                    status = course.Status,
                    note = course.RejectionReason,
                    updatedAt = course.UpdatedDate
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Failed to resubmit course." });
            }
        }

        //[Authorize(Roles = UserRoles.)]
        [HttpGet("histories")]
        public async Task<IActionResult> GetCourseHistoriesByIdAsync(int Id)
        {
            // Lấy danh sách lịch sử từ service
            var histories = await _courseHistoryService.GetCourseHistoriesByIdAsync(Id);

            if (histories == null || !histories.Any())
                return NotFound(new { message = "Không có lịch sử khóa học nào." });

            // Trả về list JSON
            return Ok(histories);
        }

        //Staff lam course
        private string RequireUserId()
        {
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(uid))
                throw new UnauthorizedAccessException("No user id.");
            return uid;
        }
        // lấy outline khóa học cho staff để học
        [HttpGet("{courseId:int}/outline")]
        [Authorize]
        public async Task<ActionResult<CourseOutlineDto>> GetOutline(int courseId, CancellationToken ct)
        {
            try
            {
                var userId = RequireUserId();
                var dto = await _courseService.GetOutlineAsync(courseId, userId, ct);
                return Ok(dto);
            }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
            catch (ArgumentException ex) { return NotFound(new { message = ex.Message }); }
        }
        // lấy tiến độ của staff
        [HttpGet("{courseId:int}/progress")]
        [Authorize]
        public async Task<ActionResult<CourseProgressDto>> GetCourseProgress(int courseId, CancellationToken ct)
        {
            try
            {
                var userId = RequireUserId();
                var dto = await _courseService.GetCourseProgressAsync(courseId, userId, ct);
                return Ok(dto);
            }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
            catch (ArgumentException ex) { return NotFound(new { message = ex.Message }); }
        }
        // đánh dấu hoàn thành lesson
        [HttpPost("lessons/{lessonId:int}/complete")]
        [Authorize]
        public async Task<IActionResult> CompleteLesson(int lessonId, CancellationToken ct)
        {
            try
            {
                var userId = RequireUserId();
                await _courseService.CompleteLessonAsync(lessonId, userId, ct);
                return NoContent();
            }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
            catch (ArgumentException ex) { return NotFound(ex.Message); }
            catch (InvalidOperationException ex) { return Conflict(ex.Message); } // 409 khi chưa pass quiz
        }
        // hủy đánh dấu hoàn thành lesson
        [HttpDelete("lessons/{lessonId:int}/complete")]
        [Authorize]
        public async Task<IActionResult> UndoCompleteLesson(int lessonId, CancellationToken ct)
        {
            try
            {
                var userId = RequireUserId();
                await _courseService.UndoCompleteLessonAsync(lessonId, userId, ct);
                return NoContent();
            }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
            catch (ArgumentException ex) { return NotFound(new { message = ex.Message }); }
        }
        // trả toàn bộ course với tình trạng đã hoàn thành lesson hoặc chưa
        [HttpGet("{courseId:int}/learning")]
        [Authorize]
        public async Task<ActionResult<CourseLearningDto>> GetLearning(int courseId, CancellationToken ct)
        {
            try
            {
                var userId = RequireUserId();
                var dto = await _courseService.GetCourseLearningAsync(courseId, userId, ct);
                return Ok(dto);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// thong ke so lieu theo khoa hoc
        /// </summary>
        /// <param name="courseId"></param>
        /// <returns></returns>
        [HttpGet("statistics/{courseId}")]
        public async Task<IActionResult> GetStatisticsCourse(int courseId)
        {
            var classList = await _classService.GetClassesByCourseAsync(courseId);
            var staffCount = 0;
            foreach (var c in classList)
            {
                var classResponse = await _classService.GetClassDetailAsync(c.ClassId);
                staffCount += classResponse!.Employees.Count;
            }
            var countClass = classList?.Count() ?? 0;
            var result = new StatisticsCourseDto
            {
                ClassCount = countClass,
                StaffCount = staffCount
            };

            return Ok(result);
        }

    }
}