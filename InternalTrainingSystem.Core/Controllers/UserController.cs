using InternalTrainingSystem.Core.Constants;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Services.Implement;
using InternalTrainingSystem.Core.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InternalTrainingSystem.Core.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userServices)
        {
            _userService = userServices;
        }

        [HttpGet("{courseId}/eligible-staff")]
        public IActionResult GetEligibleUsers(int courseId)
        {
            var staffWithoutCertificate = _userService.GetUserRoleEligibleStaff(courseId);

            var response = staffWithoutCertificate.Select(u => new EligibleStaffResponse
            {
                EmployeeId = u.EmployeeId,
                FullName = u.FullName,
                Email = u.Email!,
                Department = u.Department,
                Position = u.Position,
                Status = u.CourseEnrollments
                .FirstOrDefault(e => e.CourseId == courseId)?.Status ?? EnrollmentConstants.Status.NotEnrolled,
                Reason = u.CourseEnrollments
                .FirstOrDefault(e => e.CourseId == courseId)?.RejectionReason ?? "Không lí do!",
            }).ToList();

            return Ok(response);
        }

        [HttpGet("{courseId}/confirmed-staff")]
        public IActionResult GetConfirmedUsers(int courseId)
        {
            var confirmedUsers = _userService.GetUserRoleStaffConfirmCourse(courseId);

            var response = confirmedUsers.Select(u => new StaffConfirmCourseResponse
            {
                Id = u.Id,
                EmployeeId = u.EmployeeId,
                FullName = u.FullName,
                Email = u.Email!,
                Department = u.Department,
                Position = u.Position,
                Status = EnrollmentConstants.Status.Enrolled,
            }).ToList();

            return Ok(response);
        }

        [HttpGet("mentors")]
        public IActionResult GetMentors()
        {
            var mentors = _userService.GetMentors();

            var response = mentors.Select(m => new MentorResponse
            {
                Id = m.Id,
                EmployeeId = m.EmployeeId,
                FullName = m.FullName,
                Email = m.Email!,
                Department = m.Department,
                Position = m.Position
            }).ToList();

            return Ok(response);
        }
    }
}
