using InternalTrainingSystem.Core.Common;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Models;
using Microsoft.AspNetCore.Identity;

namespace InternalTrainingSystem.Core.Repository.Interface
{
    public interface IUserRepository
    {
        public PagedResult<EligibleStaffResponse> GetEligibleStaff(int courseId, UserSearchDto searchDto);
        public PagedResult<StaffConfirmCourseResponse> GetStaffConfirmCourse(int courseId, int page, int pageSize);
        Task<List<ApplicationUser>> GetUsersByRoleAsync(string role);
        Task CreateUserAsync(CreateUserDto req);
        List<IdentityRole> GetRoles();
        Task<ApplicationUser?> GetUserProfileAsync(string userId);
        Task<List<UserCourseSummaryDto>> GetUserCouresSummaryAsync(string userId);
        Task<PagedResult<UserListDto>> GetAllUsersAsync(GetUsersRequestDto request);
    }
}
