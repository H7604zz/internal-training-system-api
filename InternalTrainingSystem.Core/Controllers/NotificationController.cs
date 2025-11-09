using DocumentFormat.OpenXml.Spreadsheet;
using InternalTrainingSystem.Core.Configuration;
using InternalTrainingSystem.Core.Constants;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Helper;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Services.Implement;
using InternalTrainingSystem.Core.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace InternalTrainingSystem.Core.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ICourseService _couseService;
        private readonly INotificationService _notificationService;
        private readonly ICourseEnrollmentService _courseEnrollmentService;
        private readonly string _baseUrl;

        public NotificationController(IUserService userServices, IEmailSender mailService,
            ICourseService couseService, INotificationService notificationService,
            IConfiguration config, ICourseEnrollmentService courseEnrollmentService)
        {
            _userService = userServices;
            _couseService = couseService;
            _notificationService = notificationService;
            _courseEnrollmentService = courseEnrollmentService;
            _baseUrl = config["ApplicationSettings:ApiBaseUrl"] ?? "http://localhost:7001";
        }

        [HttpPost("{courseId}/notify-eligible-users")]
        [Authorize(Roles = UserRoles.DirectManager + "," + UserRoles.TrainingDepartment)]
        public async Task<IActionResult> NotifyEligibleUsers(int courseId)
        {
            var searchDto = new UserSearchDto
            {
                Page = 1,
                PageSize = int.MaxValue,
            };
            var eligiblePaged = _userService.GetEligibleStaff(courseId, searchDto);
            if (eligiblePaged.TotalCount == 0)
                return NotFound("Không có nhân viên nào cần học khóa này.");

            var course = await _couseService.GetCourseByCourseIdAsync(courseId);
            if (course == null)
                return NotFound("Không tìm thấy khóa học tương ứng.");

            var now = DateTime.Now;
            var sevenDaysAgo = now.AddDays(-7);

            if (await _notificationService.HasRecentNotification(NotificationType.Start, courseId))
            {
                return BadRequest("Thông báo mở lớp đã được gửi trong vòng 7 ngày qua. Vui lòng thử lại sau.");
            }

            if (!course.IsOnline)
            {
                bool allClassesEnded = course.Classes.All(c => c.EndDate < now);
                if (!allClassesEnded)
                    return BadRequest("Không thể gửi lại thông báo vì vẫn còn lớp học đang diễn ra.");
            }

            var EligibleStaff = eligiblePaged.Items;
            foreach (var user in EligibleStaff)
            {
                string confirmPageUrl = $"{_baseUrl}/courses/confirm?courseId={courseId}&userId={user.EmployeeId}";

                string emailContent = $@"
                    Xin chào {user.FullName},<br/><br/>
                    Lớp học <b>{course.CourseName}</b> đã được mở.<br/>
                ";

                if (course.IsMandatory || course.IsOnline)
                {
                    emailContent += "<span style='color:red;font-weight:bold'> Đây là khóa học BẮT BUỘC, dự kiến bắt đầu trong 3 ngày nữa. Vui lòng xác nhận và tham gia đúng hạn.</span><br/><br/>";
                }
                else
                {
                    emailContent += "Bạn có thể xác nhận tham gia nếu phù hợp.<br/><br/>";
                }

                emailContent += $@"
                    Vui lòng truy cập liên kết sau để xác nhận tham gia:<br/><br/>
                    <a href='{confirmPageUrl}'>➡ Vào trang xác nhận tham gia khóa học</a><br/><br/>
                    Cảm ơn!
                ";

                Hangfire.BackgroundJob.Enqueue(() => EmailHelper.SendEmailAsync(
                    user.Email!,
                    "Thông báo mở lớp học " + course.CourseName,
                    emailContent
                ));
            }

            var newEnrollments = new List<CourseEnrollment>();

            foreach (var staff in eligiblePaged.Items)
            {
                newEnrollments.Add(new CourseEnrollment
                {
                    CourseId = course.CourseId,
                    UserId = staff.Id!,
                    Status = EnrollmentConstants.Status.NotEnrolled,
                    EnrollmentDate = DateTime.Now,
                    LastAccessedDate = DateTime.Now
                });
            }

            await _courseEnrollmentService.AddRangeAsync(newEnrollments);

            await _notificationService.DeleteOldNotificationsAsync(courseId, NotificationType.Start);

            await _notificationService.SaveNotificationAsync(new Notification
            {
                CourseId = courseId,
                Type = NotificationType.Start,
                SentAt = DateTime.Now,
            },
                userIds: eligiblePaged.Items.Select(u => u.Id).ToList()!
            );
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications([FromQuery] string? userId, [FromQuery] string? roleName)
        {
            var notifications = await _notificationService.GetNotificationsAsync(userId, roleName);
            return Ok(notifications.Select(n => new
            {
                n.Id,
                n.Type,
                n.Message,
                n.SentAt,
                Recipients = n.Recipients.Select(r => new
                {
                    r.UserId,
                    r.RoleName,
                    r.IsRead,
                    r.ReadAt
                })
            }));
        }

        [HttpGet("check-status")]
        public async Task<IActionResult> CheckNotifications([FromQuery] int courseId)
        {
            var notifications = await _notificationService.HasRecentNotification(NotificationType.Start, courseId, 0);
            if (!notifications) return NotFound();

            return Ok();
        }
    }
}