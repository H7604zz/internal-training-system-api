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
using InternalTrainingSystem.Core.Common.Constants;
using Microsoft.AspNetCore.Authorization;

namespace InternalTrainingSystem.Core.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CertificateController : ControllerBase
    {
        private readonly ICertificateService _certificateService;

        public CertificateController(ICertificateService certificateService)
        {
            _certificateService = certificateService;
        }

        /// <summary>
        /// Lấy chi tiết 1 chứng chỉ theo courseId và UId
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet("{courseId}")]
        [Authorize(Roles = UserRoles.Staff)]
        public async Task<IActionResult> GetCertificateByCourseId(int courseId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Bạn cần đăng nhập để xem chứng chỉ.");
            }

            var certificate = await _certificateService.GetCertificateAsync(courseId, userId);
            if (certificate == null)
            {
                return NotFound("Không tìm thấy chứng chỉ cho khóa học này.");
            }
            return Ok(certificate);
        }

        /// <summary>
        /// Tải xuống chứng chỉ dạng PDF
        /// </summary>
        /// <param name="courseId"></param>
        /// <returns></returns>
        [HttpGet("{courseId}/download")]
        [Authorize(Roles = UserRoles.Staff)]
        public async Task<IActionResult> DownloadCertificate(int courseId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("Bạn cần đăng nhập để tải chứng chỉ.");
                }

                var pdfBytes = await _certificateService.GenerateCertificatePdfAsync(courseId, userId);
                
                var fileName = $"ChungChi_{courseId}_{DateTime.Now:yyyyMMdd}.pdf";
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Đã xảy ra lỗi khi tạo chứng chỉ PDF.");
            }
        }
    }
}
