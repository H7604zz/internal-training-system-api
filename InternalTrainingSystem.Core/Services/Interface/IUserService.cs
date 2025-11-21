using DocumentFormat.OpenXml.Spreadsheet;
using InternalTrainingSystem.Core.Common;
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
        Task<List<UserDetailResponse>> GetUsersByRoleAsync(string role);
        Task CreateUserAsync(CreateUserDto req);
        List<IdentityRole> GetRoles();
        Task<ApplicationUser?> GetUserProfileAsync(string userId);

        Task<List<UserCourseSummaryDto>> GetUserCourseSummaryAsync(string userId);
    }
}
