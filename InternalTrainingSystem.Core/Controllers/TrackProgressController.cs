using InternalTrainingSystem.Core.Repository.Implement;
using InternalTrainingSystem.Core.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InternalTrainingSystem.Core.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrackProgressController : ControllerBase
    {
        private readonly ITrackProgressService _trackProgressService;

        public TrackProgressController(ITrackProgressService trackProgressService)
        {
            _trackProgressService = trackProgressService;
        }

        /// <summary>
        /// Lấy % tiến độ hoàn thành của một module
        /// </summary>
        [HttpGet("modules/{moduleId}/percent")]
        public async Task<IActionResult> GetModuleProgress([FromRoute] int moduleId, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var percent = await _trackProgressService.UpdateModuleProgressAsync(userId, moduleId, ct);
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
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            // Gọi repository để tính toán tiến độ
            var percent = await _trackProgressService.UpdateCourseProgressAsync(userId, courseId, ct);

            // Trả về kết quả JSON
            return Ok(new
            {
                courseId,
                percent // ví dụ: 75.33
            });
        }

        /// <summary>
        /// Lấy chi tiết phòng ban (thông tin, danh sách khóa học, danh sách nhân viên + % tiến độ)
        /// </summary>
        /// <param name="departmentId">Id phòng ban</param>
        [HttpGet("{departmentId:int}")]
        public async Task<IActionResult> TrackProgressDepartment(
            [FromRoute] int departmentId,
            CancellationToken ct)
        {
            if (departmentId <= 0)
                return BadRequest("DepartmentId không hợp lệ.");

            try
            {
                var dto = await _trackProgressService.TrackProgressDepartment(departmentId);
                return Ok(dto);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                // TODO: log exception
                return StatusCode(500, "Đã xảy ra lỗi khi lấy chi tiết phòng ban.");
            }
        }
    }
}
