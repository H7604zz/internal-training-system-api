using InternalTrainingSystem.Core.Common.Constants;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternalTrainingSystem.Core.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DepartmentController : ControllerBase
    {
        private readonly IDepartmentService _departmentService;

        public DepartmentController(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetDepartments()
        {
            var departments = await _departmentService.GetDepartmentsAsync();

            return Ok(departments);
        }

        /// <summary>
        /// chi tiết phòng ban
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("detail")]
        [Authorize]
        public async Task<IActionResult> GetDepartmentDetail([FromQuery] DepartmentDetailRequestDto request)
        {
            var department = await _departmentService.GetDepartmentDetailAsync(request);
            return Ok(department);
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.Administrator)]
        public async Task<IActionResult> CreateDepartment([FromBody] DepartmentRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var success = await _departmentService.CreateDepartmentAsync(request);
            if (!success)
                return BadRequest();

            return Ok();
        }

        [HttpPut("{departmentId}")]
        [Authorize(Roles = UserRoles.Administrator)]
        public async Task<IActionResult> Update(int departmentId, [FromBody] DepartmentRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var success = await _departmentService.UpdateDepartmentAsync(departmentId, request);
            if (!success)
                return NotFound();

            return Ok();
        }

        [HttpDelete("{departmentId}")]
        [Authorize(Roles = UserRoles.Administrator)]
        public async Task<IActionResult> Delete(int departmentId)
        {
            try
            {
                var success = await _departmentService.DeleteDepartmentAsync(departmentId);
                if (!success)
                    return BadRequest(new { message = "Không thể xóa phòng ban." });

                return Ok(new { message = "Xóa phòng ban thành công." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Có lỗi xảy ra khi xóa phòng ban.", error = ex.Message });
            }
        }

        [HttpPost("transfer-employee")]
        [Authorize(Roles = UserRoles.Administrator + "," + UserRoles.HR)]
        public async Task<IActionResult> TransferEmployee([FromBody] TransferEmployeeDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var success = await _departmentService.TransferEmployeeAsync(request);
                if (!success)
                    return BadRequest(new { message = "Không thể chuyển nhân viên." });

                return Ok(new { message = "Chuyển nhân viên thành công." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Có lỗi xảy ra khi chuyển nhân viên.", error = ex.Message });
            }
        }

        /// <summary>
        /// Báo cáo tỉ lệ hoàn thành khóa học theo phòng ban
        /// </summary>
        [HttpGet("report/course-completion")]
        [Authorize(Roles = UserRoles.Administrator + "," + UserRoles.TrainingDepartment)]
        public async Task<IActionResult> GetCourseCompletionReport([FromQuery] DepartmentReportRequestDto request)
        {
            try
            {
                var report = await _departmentService.GetDepartmentCourseCompletionAsync(request);
                return Ok(report);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Có lỗi xảy ra khi tạo báo cáo.", error = ex.Message });
            }
        }

        /// <summary>
        /// Báo cáo top phòng ban học tập tích cực nhất
        /// </summary>
        [HttpGet("report/top-active")]
        [Authorize(Roles = UserRoles.Administrator + "," + UserRoles.TrainingDepartment)]
        public async Task<IActionResult> GetTopActiveDepartments([FromQuery] int top = 10, [FromQuery] DepartmentReportRequestDto? request = null)
        {
            try
            {
                request ??= new DepartmentReportRequestDto();
                var report = await _departmentService.GetTopActiveDepartmentsAsync(top, request);
                return Ok(report);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Có lỗi xảy ra khi tạo báo cáo.", error = ex.Message });
            }
        }
    }
}