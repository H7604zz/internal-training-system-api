using InternalTrainingSystem.Core.Constants;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Services.Implement;
using InternalTrainingSystem.Core.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InternalTrainingSystem.Core.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICourseEnrollmentService _courseEnrollmentService;

        private static readonly string[] AllowedRoles = { UserRoles.Staff, UserRoles.Mentor, UserRoles.HR};

        public UserController(IUserService userServices, UserManager<ApplicationUser> userManager, ICourseEnrollmentService courseEnrollmentService)
        {
            _userService = userServices;
            _userManager = userManager;
            _courseEnrollmentService = courseEnrollmentService;
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
                    Phone = user.PhoneNumber!,
                    Department = user.Department?.Name,
                    Position = user.Position,
                    Roles = roles.FirstOrDefault(),
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
            var result = await _courseEnrollmentService.GetAllCoursesEnrollmentsByStaffAsync(request);
            return Ok(result);

        }

        [HttpGet("roles")]
        public IActionResult GetUserRoles()
        {
            var result =  _userService.GetRoles();
            return Ok(result);

        }
    }
}
                                                            