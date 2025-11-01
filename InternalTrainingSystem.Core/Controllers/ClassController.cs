using DocumentFormat.OpenXml.Wordprocessing;
using InternalTrainingSystem.Core.Configuration;
using InternalTrainingSystem.Core.Constants;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using static InternalTrainingSystem.Core.DTOs.AttendanceDto;

namespace InternalTrainingSystem.Core.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClassController : ControllerBase
    {
        private readonly IClassService _classService;
        private readonly IUserService _userService;
        private readonly IAttendanceService _attendanceService;

        public ClassController(IClassService classService, IUserService userService, IAttendanceService attendanceService)
        {
            _classService = classService;
            _userService = userService;
            _attendanceService = attendanceService;
        }

        //tao course
        [HttpPost]
        public async Task<ActionResult<List<ClassDto>>> CreateClasses(CreateClassRequestDto request)
        {
            var pagedResult = _userService.GetStaffConfirmCourse(request.CourseId, 1, int.MaxValue);
            var confirmedUsers = pagedResult?.Items?.ToList() ?? new List<StaffConfirmCourseResponse>();

            if (confirmedUsers == null || !confirmedUsers.Any())
                return BadRequest("Không có học viên nào được xác nhận cho khóa học này.");

            var result = await _classService.CreateClassesAsync(request, confirmedUsers);

            if (!result)
                return BadRequest();

            return Ok();
        }

        //tao thoi khoa bieu
        [HttpPost("create-weekly")]
        public async Task<IActionResult> CreateWeeklySchedules([FromBody] CreateWeeklyScheduleRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest("Dữ liệu không hợp lệ.");

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

        // lay lich hoc cua 1 class
        [HttpGet("{classId}/schedule")]
        //[Authorize(Roles = UserRoles.DirectManager + "," + UserRoles.Staff + "," + UserRoles.Mentor)]
        public async Task<IActionResult> GetClassSchedule(int classId)
        {
            var result = await _classService.GetClassScheduleAsync(classId);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        // lay ra danh sach user trong 1 lop
        [HttpGet("{classId}/user")]
        //[Authorize]
        public async Task<IActionResult> GetStudentsByClass(int classId)
        {
            var students = await _classService.GetUserByClassAsync(classId);

            if (students.Count == 0)
                return NotFound("Không tìm thấy người học hoặc lớp học.");

            return Ok(students);
        }

        // lay ra chi tiet 1 lop
        [HttpGet("{classId}")]
        //[Authorize]
        public async Task<IActionResult> GetClassDetail(int classId)
        {
            var classDetail = await _classService.GetClassDetailAsync(classId);

            if (classDetail == null)
                return NotFound("Không tìm thấy lớp học.");

            return Ok(classDetail);
        }

        // diem danh
        [HttpPost("{scheduleId}/attendance")]
        public async Task<IActionResult> MarkAttendance(int scheduleId, [FromBody] List<AttendanceRequest> attendanceList)
        {
            if (attendanceList == null || !attendanceList.Any())
                return BadRequest("Danh sách điểm danh trống.");

            await _attendanceService.MarkAttendanceAsync(scheduleId, attendanceList);

            return Ok("Điểm danh thành công.");
        }

        // sua diem danh
        [HttpPut("{scheduleId}/attendance")]
        public async Task<IActionResult> UpdateAttendance(int scheduleId, [FromBody] List<AttendanceRequest> attendanceList)
        {
            if (attendanceList == null || !attendanceList.Any())
                return BadRequest( "Danh sách điểm danh trống.");

            var result = await _attendanceService.UpdateAttendanceAsync(scheduleId, attendanceList);

            if (!result)
                return NotFound("Không tìm thấy dữ liệu điểm danh cần cập nhật.");

            return Ok("Cập nhật điểm danh thành công.");
        }

        //lay ra thong tin diem danh cho 1 buoi hoc
        [HttpGet("schedules/{scheduleId}/attendance")]
        public async Task<IActionResult> GetAttendanceBySchedule(int scheduleId)
        {
            var attendances = await _attendanceService.GetAttendanceByScheduleAsync(scheduleId);

            if (attendances == null || !attendances.Any())
                return NotFound("Không tìm thấy thông tin điểm danh cho buổi học này.");

            return Ok(attendances);
        }

        // chuyen lop giua 2 user
        [HttpPost("change-class")]
        //[Authorize(Roles = UserRoles.Staff)]
        public async Task<IActionResult> SwapStudentsBetweenClasses([FromBody] SwapClassRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return NotFound("User not found");
            }

            var u = await _userService.GetUserProfileAsync(userId);
            if (u!.EmployeeId!= request.EmployeeId1)
            {
                return NotFound("Không tìm thấy học viên.");
            }

            var result = await _classService.SwapClassesAsync(request);
            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Message);
        }

        // lay ra tat ca lop hoc
        [HttpGet]
        //[Authorize]
        public async Task<ActionResult<PagedResult<ClassDto>>> GetClasses([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;
            var result = await _classService.GetClassesAsync(page, pageSize);
            
            return Ok(result);
        }
    }
}
