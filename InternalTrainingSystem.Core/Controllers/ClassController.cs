using DocumentFormat.OpenXml.Wordprocessing;
using InternalTrainingSystem.Core.Configuration;
using InternalTrainingSystem.Core.Constants;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Hubs;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Services.Implement;
using InternalTrainingSystem.Core.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
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
        private readonly INotificationService _notificationService;
        private readonly IHubContext<NotificationHub> _hub;

        public ClassController(IClassService classService, IUserService userService,
            IAttendanceService attendanceService, INotificationService notificationService, IHubContext<NotificationHub> hub)
        {
            _classService = classService;
            _userService = userService;
            _attendanceService = attendanceService;
            _notificationService = notificationService;
            _hub = hub;
        }

        //tao class
        [HttpPost]
        [Authorize(Roles = UserRoles.TrainingDepartment)]
        public async Task<ActionResult<List<ClassDto>>> CreateClasses(CreateClassRequestDto request)
        {
            var createdById = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(createdById))
            {
                return Unauthorized("Không thể xác định người dùng.");
            }

            var pagedResult = _userService.GetStaffConfirmCourse(request.CourseId, 1, int.MaxValue);
            var confirmedUsers = pagedResult?.Items?.ToList() ?? new List<StaffConfirmCourseResponse>();

            if (confirmedUsers == null || !confirmedUsers.Any())
                return BadRequest("Không có học viên nào được xác nhận cho khóa học này.");

            var result = await _classService.CreateClassesAsync(request, confirmedUsers, createdById);

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

            var schedule = await _classService.GetClassScheduleByIdAsync(scheduleId);
            var scheduleStart = schedule!.Date + schedule.StartTime;
            var scheduleEnd = schedule.Date + schedule.EndTime;
            var now = DateTime.Now;

            if (now < scheduleStart)
                return BadRequest("Buổi học chưa bắt đầu, không thể điểm danh.");

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

        // tao yeu cau chuyen lop giua 2 user
        [HttpPost("request-swap")]
        //[Authorize(Roles = UserRoles.Staff)]
        public async Task<IActionResult> RequestClassSwap([FromBody] SwapClassRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return NotFound("User not found");
            }

            var u = await _userService.GetUserProfileAsync(userId);
            if (u!.EmployeeId!= request.EmployeeIdFrom)
            {
                return NotFound("Không tìm thấy học viên.");
            }

            var result = await _classService.CreateClassSwapRequestAsync(request);
            if (!result.Success)
                return BadRequest(result.Message);

            await _notificationService.SaveNotificationAsync(new Notification
            {
                Type = NotificationType.UserSwapClass,
                SentAt = DateTime.Now,
                Message = "Có 1 yêu cầu đổi lớp đang đợi bạn phản hồi",
            },
               userIds: new List<string> { request.EmployeeIdTo }
            );

            await _hub.Clients.User(request.EmployeeIdTo).SendAsync("ReceiveNotification", new
            {
                Type = "UserSwapClass",
                Message = "Bạn có một yêu cầu đổi lớp đang chờ phản hồi."
            });

            return Ok(result.Message);
        }

        // phan hoi yeu cau chuyen lop giua 2 user
        [HttpPost("respond-swap-request")]
        //[Authorize(Roles = UserRoles.Staff)]
        public async Task<IActionResult> RespondToSwapRequest([FromBody] RespondSwapRequest request)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized("Không xác định được người dùng.");

            var result = await _classService.RespondToClassSwapAsync(request, currentUserId);
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

        /// <summary>
        /// cho phep mentor chuyen lich hoc 1 ngay trong tuan
        /// </summary>
        /// <param name="scheduleId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("reschedule/{scheduleId}")]
        //[Authorize(Roles = UserRoles.Mentor)]
        public async Task<IActionResult> Reschedule(int scheduleId, [FromBody] RescheduleRequest request)
        {
            var result = await _classService.RescheduleAsync(scheduleId, request);
            if (!result.Success)
                return BadRequest(result.Message);

            return Ok();
        }
    }
}
