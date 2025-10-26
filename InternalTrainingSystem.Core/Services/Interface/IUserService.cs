using InternalTrainingSystem.Core.Configuration;
using InternalTrainingSystem.Core.Constants;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace InternalTrainingSystem.Core.Services.Interface
{
    public interface IUserService
    {
        public PagedResult<EligibleStaffResponse> GetEligibleStaff(int courseId, UserSearchDto searchDto);
        public PagedResult<StaffConfirmCourseResponse> GetStaffConfirmCourse(int courseId, int page, int pageSize);
        public List<ApplicationUser> GetUsersByRole(string role);
        Task<bool> CreateUserAsync(CreateUserDto req);
        Task<bool> VerifyAccountAsync(string userId, string token);
        List<IdentityRole> GetRoles();
    }
}
