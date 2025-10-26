using InternalTrainingSystem.Core.Configuration;
using InternalTrainingSystem.Core.Constants;
using InternalTrainingSystem.Core.DTOs;
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
            var searchDto = new UserSearchDto
            {
                Page = 1,
                PageSize = int.MaxValue,
            };
            var eligiblePaged = _userService.GetEligibleStaff(courseId, searchDto);
            if (eligiblePaged.TotalCount == 0)
                return NotFound("Không có nhân viên nào cần học khóa này.");

            var course = _couseService.GetCourseByCourseID(courseId);
            if (course == null)
                return NotFound("Không tìm thấy khóa học tương ứng.");

            var now = DateTime.UtcNow;
            var sevenDaysAgo = now.AddDays(-7);

            if (_notificationService.HasRecentNotification(NotificationType.Start, courseId))
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

                Hangfire.BackgroundJob.Enqueue(() => _mailService.SendEmailAsync(
                    user.Email!,
                    "Thông báo mở lớp học " + course.CourseName,
                    emailContent
                ));
            }

            _notificationService.DeleteOldNotifications(courseId, NotificationType.Start);

            _notificationService.SaveNotificationAsync(new Notification
            {
                CourseId = courseId,
                Type = NotificationType.Start,
                SentAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
            });
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
    }
}