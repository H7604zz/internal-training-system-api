using InternalTrainingSystem.Core.Configuration;
using InternalTrainingSystem.Core.Constants;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

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
        [Authorize(Roles = UserRoles.DirectManager + "," + UserRoles.TrainingDepartment)]
        public IActionResult NotifyEligibleUsers(int courseId)
        {
            var eligiblePaged = _userService.GetUserRoleEligibleStaff(courseId, 1, int.MaxValue);

            if (eligiblePaged.TotalCount == 0)
                return NotFound("Không có nhân viên nào cần học khóa này.");
            var EligibleStaff = eligiblePaged.Items;
            var course = _couseService.GetCourseByCourseID(courseId);
            if (course == null)
                return NotFound("Không tìm thấy khóa học tương ứng.");

            foreach (var user in EligibleStaff)
            {
                string confirmPageUrl = $"{_baseUrl}/courses/confirm?courseId={courseId}&userId={user.EmployeeId}";

                string emailContent = $@"
                    Xin chào {user.FullName},<br/><br/>
                    Lớp học <b>{course.CourseName}</b> đã được mở.<br/>
                ";

                if (course.IsMandatory)
                {
                    emailContent += "<span style='color:red;font-weight:bold'> Đây là khóa học BẮT BUỘC. Vui lòng xác nhận và tham gia đúng hạn.</span><br/><br/>";
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

                Hangfire.BackgroundJob.Enqueue(() => _mailService.SendEmailAsync(
                    user.Email!,
                    "Thông báo mở lớp học " + course.CourseName,
                    emailContent
                ));
            }

            return Ok();
        }

        [HttpGet("{courseId}/notification-status/{type}")]
        [Authorize]
        public IActionResult CheckNotificationStatus(int courseId, NotificationType type)
        {
            var notification = _notificationService.GetNotificationByCourseAndType(courseId, type);
            if (notification == null)
                return Ok(new { sent = false });

            return Ok(new { sent = true, sentAt = notification.SentAt });
        }

        [HttpPost("{courseId}/notify-staff")]
        [Authorize(Roles = UserRoles.DirectManager + "," + UserRoles.TrainingDepartment)]
        public IActionResult NotifyEligibleStaff(int courseId)
        {
            var course = _couseService.GetCourseByCourseID(courseId);
            if (course == null)
                return NotFound("Khóa học không tồn tại.");

            _notificationService.SaveNotificationAsync(new CourseNotification
            {
                CourseId = courseId,
                Type = NotificationType.StaffConfirm,
                SentAt = DateTime.UtcNow,
            });

            return Ok();
        }
    }
}