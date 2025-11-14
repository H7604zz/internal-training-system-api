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
        private readonly IUserService _userService;

        public CertificateController(ICertificateService certificateService,
            IUserService userServices, IConfiguration config)
        {
            _certificateService = certificateService;
            _userService = userServices;
        }

        /// <summary>
        /// Lấy chi tiết 1 chứng chỉ theo Id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCertificateById(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var certificate = await _certificateService.GetCertificateByIdAsync(id, userId);
            if (certificate == null)
            {
                return NotFound("Không tìm thấy chứng chỉ.");
            }
            return Ok(certificate);
        }
    }
}
