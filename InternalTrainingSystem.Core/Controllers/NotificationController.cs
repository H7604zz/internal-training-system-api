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
        public NotificationController(IUserService userServices, IEmailSender mailService,
            ICourseService couseService, INotificationService notificationService)
        {
            _userService = userServices;
            _mailService = mailService;
            _couseService = couseService;
            _notificationService = notificationService;
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
                _mailService.SendEmailAsync(user.Email!,
                    "Thông báo mở lớp học " + course.CourseName,
                    $"Xin chào {user.UserName}, nếu bạn chưa có chứng chỉ cho khóa học, vui lòng tham gia lớp học sắp tới.");
            }

            _notificationService.SaveNotificationAsync(new CourseNotification
            {
                CourseId = courseId,
                Type = NotificationType.StaffConfirm,
                SentAt = DateTime.UtcNow,
            });

            return Ok(new { Message = "Đã gửi mail cho danh sách nhân viên cần học.", Count = EligibleStaff.Count});
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