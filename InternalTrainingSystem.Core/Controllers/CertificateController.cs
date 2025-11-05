using DocumentFormat.OpenXml.Spreadsheet;
using InternalTrainingSystem.Core.Helper;
using InternalTrainingSystem.Core.Hubs;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Services.Implement;
using InternalTrainingSystem.Core.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace InternalTrainingSystem.Core.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CertificateController : ControllerBase
    {
        private readonly ICertificateService _certificateService;
        private readonly INotificationService _notificationService;
        private readonly IUserService _userService;
        private readonly string _baseUrl;

        public CertificateController(ICertificateService certificateService, INotificationService notificationService, 
            IUserService userServices, IConfiguration config)
        {
            _certificateService = certificateService;
            _notificationService = notificationService;
            _userService = userServices;
            _baseUrl = config["ApplicationSettings:ApiBaseUrl"] ?? "http://localhost:7001";
        }

        [HttpPost("{courseId}/issue-certificate")]
        public async Task<IActionResult> IssueCertificate(int courseId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var user = await _userService.GetUserProfileAsync(userId);

            if (user == null)
            {
                return NotFound("User not found");
            }

            var result = await _certificateService.IssueCertificateAsync(user.Id, courseId);

            string viewCertificatesUrl = $"{_baseUrl}/profile/certificates";

            string emailContent = $@"
                Xin chào {user.FullName},<br/><br/>
                Chúc mừng bạn đã <b>hoàn thành khóa học {result.CourseName}</b>! 🎉<br/><br/>
                Hệ thống đã cấp cho bạn chứng chỉ hoàn thành khóa học.<br/>
                Bạn có thể xem hoặc tải chứng chỉ trong trang <a href='{viewCertificatesUrl}'>Hồ sơ cá nhân</a>.<br/><br/>
                Trân trọng,<br/>
                <b>Phòng Đào Tạo</b>
            ";

            Hangfire.BackgroundJob.Enqueue(() => EmailHelper.SendEmailAsync(
                user.Email!,
                $"Chúc mừng bạn nhận chứng chỉ khóa học {result.CourseName}",
                emailContent
            ));

            return Ok(result);
        }

    }
}
