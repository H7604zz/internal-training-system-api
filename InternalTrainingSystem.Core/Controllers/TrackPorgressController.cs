using InternalTrainingSystem.Core.Repository.Implement;
using InternalTrainingSystem.Core.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InternalTrainingSystem.Core.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrackPorgressController : ControllerBase
    {
        private readonly ITrackProgressService _trackProgressService;

        public TrackPorgressController(ITrackProgressService trackProgressService)
        {
            _trackProgressService = trackProgressService;
        }

        /// <summary>
        /// Lấy % tiến độ hoàn thành của một module
        /// </summary>
        [HttpGet("modules/{moduleId}/percent")]
        public async Task<IActionResult> GetModuleProgress([FromRoute] int moduleId, CancellationToken ct)
        {
            var percent = await _trackProgressService.UpdateModuleProgressAsync(moduleId, ct);
            return Ok(new
            {
                moduleId,
                percent // ví dụ 66.67
            });
        }

        /// <summary>
        /// Lấy phần trăm tiến độ hoàn thành của 1 khóa học (course)
        /// </summary>
        [HttpGet("courses/{courseId}/percent")]
        public async Task<IActionResult> GetCourseProgress([FromRoute] int courseId, CancellationToken ct)
        {
            // Gọi repository để tính toán tiến độ
            var percent = await _trackProgressService.UpdateCourseProgressAsync(courseId, ct);

            // Trả về kết quả JSON
            return Ok(new
            {
                courseId,
                percent // ví dụ: 75.33
            });
        }
    }
}
