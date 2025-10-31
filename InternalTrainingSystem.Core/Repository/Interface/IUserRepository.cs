using InternalTrainingSystem.Core.Configuration;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Models;
using Microsoft.AspNetCore.Identity;

namespace InternalTrainingSystem.Core.Repository.Interface
{
    public interface IUserRepository
    {
        public PagedResult<EligibleStaffResponse> GetEligibleStaff(int courseId, UserSearchDto searchDto);
        public PagedResult<StaffConfirmCourseResponse> GetStaffConfirmCourse(int courseId, int page, int pageSize);
        public List<ApplicationUser> GetUsersByRole(string role);
        Task<bool> CreateUserAsync(CreateUserDto req);
        List<IdentityRole> GetRoles();
        Task<ApplicationUser?> GetUserProfileAsync(string userId);
        Task<List<UserAttendanceResponse>> GetUserAttendanceSummaryAsync(string userId);
    }
}
