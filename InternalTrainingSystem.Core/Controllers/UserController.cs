using InternalTrainingSystem.Core.Constants;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Services.Implement;
using InternalTrainingSystem.Core.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InternalTrainingSystem.Core.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        private static readonly string[] AllowedRoles = { UserRoles.Staff, UserRoles.Mentor, UserRoles.HR};

        public UserController(IUserService userServices)
        {
            _userService = userServices;
        }

        [HttpGet("{courseId}/eligible-staff")]
        [Authorize(Roles = UserRoles.DirectManager + "," + UserRoles.TrainingDepartment)]
        public IActionResult GetEligibleUsers(int courseId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = _userService.GetUserRoleEligibleStaff(courseId, page, pageSize);
            return Ok(result);
        }

        [HttpGet("{courseId}/confirmed-staff")]
        [Authorize(Roles = UserRoles.DirectManager + "," + UserRoles.TrainingDepartment)]
        public IActionResult GetConfirmedUsers(int courseId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var confirmedUsers = _userService.GetUserRoleStaffConfirmCourse(courseId, page, pageSize);
            return Ok(confirmedUsers);
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
    }
}
