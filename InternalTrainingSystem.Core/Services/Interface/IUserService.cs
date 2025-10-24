using InternalTrainingSystem.Core.Configuration;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace InternalTrainingSystem.Core.Services.Interface
{
    public interface IUserService
    {
        public PagedResult<EligibleStaffResponse> GetUserRoleEligibleStaff(int courseId, int page, int pageSize);
        public PagedResult<StaffConfirmCourseResponse> GetUserRoleStaffConfirmCourse(int courseId, int page, int pageSize);
        public List<ApplicationUser> GetUsersByRole(string role);
        Task<IActionResult> CreateUserAsync(CreateUserDto req);
    }
}
