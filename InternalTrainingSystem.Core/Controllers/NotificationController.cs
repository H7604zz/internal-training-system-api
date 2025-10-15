using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;

namespace InternalTrainingSystem.Core.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IEmailSender _mailService;
        private readonly ICourseService _couseService;
        private readonly INotificationService _notificationService;
        private readonly string _baseUrl;
        public NotificationController(IUserService userServices, IEmailSender mailService,
            ICourseService couseService, INotificationService notificationService, IConfiguration config)
        {
            _userService = userServices;
            _mailService = mailService;
            _couseService = couseService;
            _notificationService = notificationService;
            _baseUrl = config["ApplicationSettings:ApiBaseUrl"] ?? "http://localhost:7001";
        }

        [HttpPost("{courseId}/notify-eligible-users")]
        public IActionResult NotifyEligibleUsers(int courseId)
        {
            var EligibleStaff = _userService.GetUserRoleEligibleStaff(courseId);

            if (!EligibleStaff.Any())
                return NotFound("Không có nhân viên nào cần học khóa này.");
            var course = _couseService.GetCourseByCourseID(courseId);
            if (course == null)
                return NotFound("Không tìm thấy khóa học tương ứng.");

            foreach (var user in EligibleStaff)
            {
                string confirmPageUrl = $"{_baseUrl}/courses/confirm?courseId={courseId}&userId={user.Id}";

                Hangfire.BackgroundJob.Enqueue(() => _mailService.SendEmailAsync(
                    user.Email!,
                    "Thông báo mở lớp học " + course.CourseName,
                    $@"
                    Xin chào {user.UserName},<br/><br/>
                    Lớp học <b>{course.CourseName}</b> đã được mở.<br/>
                    Vui lòng truy cập liên kết sau để xác nhận tham gia:<br/><br/>
                    <a href='{confirmPageUrl}'>➡ Vào trang xác nhận tham gia khóa học</a><br/><br/>
                    Cảm ơn!
                    "
                ));
            }

            _notificationService.SaveNotificationAsync(new CourseNotification
            {
                CourseId = courseId,
                Type = NotificationType.StaffConfirm,
                SentAt = DateTime.UtcNow,
            });

            return Ok(new { Message = "Đã gửi mail cho danh sách nhân viên cần học." });
        }

        [HttpGet("{courseId}/notification-status/{type}")]
        public IActionResult CheckNotificationStatus(int courseId, NotificationType type)
        {
            var notification = _notificationService.GetNotificationByCourseAndType(courseId, type);
            if (notification == null)
                return Ok(new { sent = false });

            return Ok(new { sent = true, sentAt = notification.SentAt });
        }
    }
}