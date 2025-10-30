using DocumentFormat.OpenXml.Wordprocessing;
using InternalTrainingSystem.Core.Configuration;
using InternalTrainingSystem.Core.Constants;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InternalTrainingSystem.Core.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClassController : ControllerBase
    {
        private readonly IClassService _classService;
        private readonly IUserService _userService;

        public ClassController(IClassService classService, IUserService userService)
        {
            _classService = classService;
            _userService = userService;
        }

        [HttpPost]
        public async Task<ActionResult<List<ClassDto>>> CreateClasses(CreateClassRequestDto request)
        {
            var pagedResult = _userService.GetStaffConfirmCourse(request.CourseId, 1, int.MaxValue);
            var confirmedUsers = pagedResult?.Items?.ToList() ?? new List<StaffConfirmCourseResponse>();

            if (confirmedUsers == null || !confirmedUsers.Any())
                return BadRequest("Không có học viên nào được xác nhận cho khóa học này.");

            var result = await _classService.CreateClassesAsync(request, confirmedUsers);

            if (!result.Success)
                return BadRequest();

            return Ok(result.Data);
        }

        [HttpPost("create-weekly")]
        public async Task<IActionResult> CreateWeeklySchedules([FromBody] CreateWeeklyScheduleRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ." });

            var result = await _classService.CreateWeeklySchedulesAsync(request);

            if (!result.Success)
                return BadRequest(new { success = false, message = result.Message });

            return Ok(new
            {
                success = true,
                message = result.Message,
                createdCount = result.Count
            });
        }

        [HttpGet("{classId}/schedule")]
        //[Authorize(Roles = UserRoles.DirectManager + "," + UserRoles.Staff + "," + UserRoles.Mentor)]
        public async Task<IActionResult> GetClassSchedule(int classId)
        {
            var result = await _classService.GetClassScheduleAsync(classId);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("{classId}/user")]
        //[Authorize]
        public async Task<IActionResult> GetStudentsByClass(int classId)
        {
            var students = await _classService.GetUserByClassAsync(classId);

            if (students.Count == 0)
                return NotFound(new { success = false, message = "Không tìm thấy người học hoặc lớp học." });

            return Ok(new
            {
                success = true,
                message = "Lấy danh sách người học thành công",
                data = students
            });
        }

        [HttpGet("{classId}")]
        //[Authorize]
        public async Task<IActionResult> GetClassDetail(int classId)
        {
            var classDetail = await _classService.GetClassDetailAsync(classId);

            if (classDetail == null)
                return NotFound(new { success = false, message = "Không tìm thấy lớp học." });

            return Ok(new
            {
                success = true,
                message = "Lấy thông tin lớp học thành công",
                data = classDetail
            });
        }
    }
}
