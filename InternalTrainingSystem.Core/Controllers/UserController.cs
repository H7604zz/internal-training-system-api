using DocumentFormat.OpenXml.Spreadsheet;
using InternalTrainingSystem.Core.Common.Constants;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Services.Implement;
using InternalTrainingSystem.Core.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace InternalTrainingSystem.Core.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IClassService _classService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICourseEnrollmentService _courseEnrollmentService;
        private readonly ICertificateService _certificateService;

        private static readonly string[] AllowedRoles = { UserRoles.Staff, UserRoles.Mentor, UserRoles.HR };

        public UserController(IUserService userServices, UserManager<ApplicationUser> userManager, 
            ICourseEnrollmentService courseEnrollmentService, IClassService classService, ICertificateService certificateService)
        {
            _userService = userServices;
            _userManager = userManager;
            _courseEnrollmentService = courseEnrollmentService;
            _classService = classService;
            _certificateService = certificateService;
        }

        /// <summary>
        /// Get current user profile
        /// </summary>
        [HttpGet("profile")]
        [Authorize]
        public async Task<ActionResult<ApiResponseDto>> GetProfile()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return NotFound("User not found");
                }

                // Sử dụng UserService để lấy user profile
                var user = await _userService.GetUserProfileAsync(userId);
                    
                if (user == null)
                {
                    return NotFound("User not found");
                }

                var roles = await _userManager.GetRolesAsync(user);

                var userProfile = new UserProfileDto
                {
                    Id = user.Id,
                    EmployeeId = user.EmployeeId,
                    FullName = user.FullName,
                    Email = user.Email!,
                    PhoneNumber = user.PhoneNumber!,
                    Department = user.Department?.Name,
                    Position = user.Position,
                    CurrentRole = roles.FirstOrDefault(),
                    IsActive = user.IsActive,
                    LastLoginDate = user.LastLoginDate
                };

                return Ok(userProfile);
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        /// <summary>
        /// Cập nhật thông tin người dùng (tên và sđt)
        /// </summary>
        [HttpPatch("update-profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto updateProfileDto)
        {
            try
            {
                // Lấy ID người dùng từ token
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("Không xác định được người dùng hiện tại.");
                }

                // Tìm user
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound("Không tìm thấy người dùng.");
                }

                // Validate FullName
                if (string.IsNullOrWhiteSpace(updateProfileDto.FullName))
                {
                    return BadRequest("Họ tên không được để trống.");
                }

                // Validate PhoneNumber
                if (!string.IsNullOrWhiteSpace(updateProfileDto.PhoneNumber))
                {
                    var phone = updateProfileDto.PhoneNumber.Trim();

                    // Kiểm tra đúng 10 chữ số
                    if (!Regex.IsMatch(phone, @"^\d{10}$"))
                    {
                        return BadRequest("Số điện thoại phải gồm đúng 10 chữ số");
                    }

                    user.PhoneNumber = phone;
                }
                else
                {
                    user.PhoneNumber = null;
                }

                // Cập nhật thông tin
                user.FullName = updateProfileDto.FullName.Trim();

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                    return BadRequest($"Cập nhật thất bại: {errors}");
                }

                return Ok("Cập nhật thông tin thành công.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Đã xảy ra lỗi khi cập nhật hồ sơ: {ex.Message}");
            }
        }


        [HttpGet("by-role")]
        [Authorize(Roles = UserRoles.DirectManager + "," + UserRoles.TrainingDepartment)]
        public IActionResult GetUsersByRole([FromQuery] string role)
        {
            if (!AllowedRoles.Any(r => r.Equals(role, StringComparison.OrdinalIgnoreCase)))
            {
                throw new UnauthorizedAccessException("You are not allowed to query this role");
            }

            var users = _userService.GetUsersByRole(role);

            var response = users.Select(m => new UserDetailResponse
            {
                Id = m.Id,
                EmployeeId = m.EmployeeId,
                FullName = m.FullName,
                Email = m.Email!,
                Department = m.Department?.Name,
                Position = m.Position
            }).ToList();

            return Ok(response);
        }

        /// <summary>
        /// API tạo mới người dùng và gửi email kích hoạt.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto req)
        {
            if (req == null)
                return BadRequest("Dữ liệu đầu vào không hợp lệ.");

            var success = await _userService.CreateUserAsync(req);

            if (!success)
                return BadRequest("Không thể tạo người dùng. Vui lòng kiểm tra lại dữ liệu hoặc email.");

            return Ok(new
            {
                Message = "Tạo người dùng thành công. Email kích hoạt đã được gửi.",
                Email = req.Email
            });
        }

        [HttpGet("courses")]
        [Authorize(Roles = UserRoles.Staff)]
        public async Task<IActionResult> GetUserCourses([FromQuery] GetAllCoursesRequest request)
        {
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(uid)) throw new UnauthorizedAccessException("Người dùng chưa được xác thực.");
            var result = await _courseEnrollmentService.GetAllCoursesEnrollmentsByStaffAsync(request, uid);
            return Ok(result);

        }

        [HttpGet("roles")]
        public IActionResult GetUserRoles()
        {
            var result =  _userService.GetRoles();
            return Ok(result);

        }

        [HttpGet("schedule")]
        public async Task<IActionResult> GetStudentSchedule()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            var result = await _classService.GetUserScheduleAsync(userId);

            if (!result.Any())
                return NotFound();

            return Ok(result);
        }

        /// <summary>
        /// lay thong tin user lien quan den khoa hoc(diem danh, diem so, trang thai)
        /// </summary>
        /// <returns></returns>
        [HttpGet("course-summary")]
        [Authorize(Roles = UserRoles.Staff)]
        public async Task<IActionResult> GetUserCourseSummary()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return NotFound("User not found");
            }

            var result = await _userService.GetUserCourseSummaryAsync(userId);

            if (result == null || !result.Any())
                return NotFound("Không có dữ liệu cho lớp này.");

            return Ok(result);
        }

        [HttpGet("certificates")]

        public async Task<IActionResult> GetCertificatesByUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var certificates = await _certificateService.GetCertificateByUserAsync(userId);
            if (certificates == null || certificates.Count == 0)
            {
                return NotFound("Không tìm thấy chứng chỉ nào cho người dùng này.");
            }
            return Ok(certificates);
        }

    }
}
                                                            