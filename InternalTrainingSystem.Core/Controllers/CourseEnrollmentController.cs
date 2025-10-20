using InternalTrainingSystem.Core.Configuration;
using InternalTrainingSystem.Core.Constants;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace InternalTrainingSystem.Core.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseEnrollmentController : ControllerBase
    {
        private readonly ICourseEnrollmentService _courseEnrollmentService;
        private readonly IHubContext<EnrollmentHub> _hub;

        public CourseEnrollmentController(ICourseEnrollmentService courseEnrollmentService, IHubContext<EnrollmentHub> hub)
        {
            _courseEnrollmentService = courseEnrollmentService;
            _hub = hub;
        }

        [HttpPost("{courseId}/{userId}/confirm")]
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

        [HttpPost("{courseId}/{userId}/status")]
        [Authorize(Roles = UserRoles.Staff)]
        public async Task<IActionResult> UpdateEnrollmentStatus(int courseId, string userId, [FromBody] EnrollmentStatusUpdateRequest request)
        {
            var enrollment = new CourseEnrollment
            {
                CourseId = courseId,
                UserId = userId,
                EnrollmentDate = DateTime.UtcNow,
                LastAccessedDate = DateTime.UtcNow
            };

            if (request.IsConfirmed)
                enrollment.Status = EnrollmentConstants.Status.Enrolled;
            else 
                enrollment.Status = EnrollmentConstants.Status.Dropped;
                enrollment.RejectionReason = string.IsNullOrWhiteSpace(request.Reason) ? "Không cung cấp lý do" : request.Reason;

            _courseEnrollmentService.AddCourseEnrollment(enrollment);

            await _hub.Clients.Group($"course-{courseId}")
            .SendAsync("EnrollmentStatusChanged", new
            {
                CourseId = courseId,
                UserId = userId,
                Status = enrollment.Status,
                Reason = enrollment.RejectionReason
            });

            return Ok(new
            {
                Message = request.IsConfirmed
                     ? "Bạn đã xác nhận tham gia khóa học."
                     : "Bạn đã hủy tham gia khóa học."
            });
        }
    }
}
