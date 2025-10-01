using InternalTrainingSystem.Core.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;

namespace InternalTrainingSystem.Core.Controllers
{
    [Route("api/notifications")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IEmailSender _mailService;
        private readonly ICourseService _couseService;
        public NotificationController(IUserService userServices, IEmailSender mailService, ICourseService couseService)
        {
            _userService = userServices;
            _mailService = mailService;
            _couseService = couseService;
        }

        [HttpPost("{courseId}/notify-eligible-users")]
        public IActionResult NotifyEligibleUsers(int courseId)
        {
            var staffWithoutCertificate = _userService.GetUserRoleStaffWithoutCertificate(courseId);

            if (!staffWithoutCertificate.Any())
                return NotFound("Không có nhân viên nào cần học khóa này.");
            var course = _couseService.GetCourseByCourseID(courseId);
            if (course == null)
                return NotFound("Không tìm thấy khóa học tương ứng.");

            foreach (var user in staffWithoutCertificate)
            {
                _mailService.SendEmailAsync(user.Email!,
                    "Thông báo mở lớp học " + course.CourseName,
                    $"Xin chào {user.UserName}, nếu bạn chưa có chứng chỉ cho khóa học, vui lòng tham gia lớp học sắp tới.");
            }

            return Ok(new { Message = "Đã gửi mail cho danh sách nhân viên cần học.", Count = staffWithoutCertificate .Count});
        }
    }
}