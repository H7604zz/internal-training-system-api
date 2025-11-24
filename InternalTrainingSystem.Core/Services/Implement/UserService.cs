using InternalTrainingSystem.Core.Common;
using InternalTrainingSystem.Core.DB;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Helper;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Repository.Interface;
using InternalTrainingSystem.Core.Services.Interface;
using InternalTrainingSystem.Core.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace InternalTrainingSystem.Core.Services.Implement
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepo;

        public UserService(IUserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        public PagedResult<StaffConfirmCourseResponse> GetStaffConfirmCourse(int courseId, int page, int pageSize)
        {
            return _userRepo.GetStaffConfirmCourse(courseId, page, pageSize);
        }

        public PagedResult<EligibleStaffResponse> GetEligibleStaff(int courseId, UserSearchDto searchDto)
        {
            return _userRepo.GetEligibleStaff(courseId, searchDto);
        }

        public async Task<List<UserDetailResponse>> GetUsersByRoleAsync(string role)
        {
            // Call repository to get users by role
            var users = await _userRepo.GetUsersByRoleAsync(role);

            // Map to DTO (business logic layer)
            var response = users.Select(user => new UserDetailResponse
            {
                Id = user.Id,
                EmployeeId = user.EmployeeId,
                FullName = user.FullName,
                Email = user.Email!,
                Department = user.Department?.Name,
                Position = user.Position
            }).ToList();

            return response;
        }

        public async Task CreateUserAsync(CreateUserDto req)
        {
            await _userRepo.CreateUserAsync(req);
        }

        public List<IdentityRole> GetRoles()
        {
           return _userRepo.GetRoles();
        }

        public async Task<ApplicationUser?> GetUserProfileAsync(string userId)
        {
            return await _userRepo.GetUserProfileAsync(userId);
        }

        public async Task<List<UserCourseSummaryDto>> GetUserCourseSummaryAsync(string userId)
        {
            return await _userRepo.GetUserCouresSummaryAsync(userId);
        }

        public async Task<PagedResult<UserListDto>> GetAllUsersAsync(GetUsersRequestDto request)
        {
            return await _userRepo.GetAllUsersAsync(request);
        }
    }
}