using InternalTrainingSystem.Core.Configuration;
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

        public List<ApplicationUser> GetUsersByRole(string role)
        {
           return _userRepo.GetUsersByRole(role);
        }

        public async Task<bool> CreateUserAsync(CreateUserDto req)
        {
            return await _userRepo.CreateUserAsync(req);
        }

        public List<IdentityRole> GetRoles()
        {
           return _userRepo.GetRoles();
        }

        public async Task<ApplicationUser?> GetUserProfileAsync(string userId)
        {
            return await _userRepo.GetUserProfileAsync(userId);
        }

        public async Task<List<UserAttendanceResponse>> GetUserAttendanceSummaryAsync(string userId)
        {
            return await _userRepo.GetUserAttendanceSummaryAsync(userId);
        }
    }
}