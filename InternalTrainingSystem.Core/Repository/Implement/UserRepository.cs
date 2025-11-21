using InternalTrainingSystem.Core.Common;
using InternalTrainingSystem.Core.Common.Constants;
using InternalTrainingSystem.Core.DB;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Helper;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Repository.Interface;
using InternalTrainingSystem.Core.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;

namespace InternalTrainingSystem.Core.Repository.Implement
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserRepository(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _context = context;
            _userManager = userManager;
        }

        public PagedResult<StaffConfirmCourseResponse> GetStaffConfirmCourse(int courseId, int page, int pageSize)
        {
            var query = _context.Users
                .Include(u => u.CourseEnrollments)
                .Include(u => u.Department)
                .Join(_context.UserRoles, u => u.Id, ur => ur.UserId, (u, ur) => new { u, ur })
                .Join(_context.Roles, x => x.ur.RoleId, r => r.Id, (x, r) => new { x.u, r })
                .Where(x => x.r.Name == UserRoles.Staff
                            && _context.CourseEnrollments.Any(c => c.UserId == x.u.Id
                                                                   && c.CourseId == courseId
                                                                   && c.Status == EnrollmentConstants.Status.Enrolled))
                .Select(x => x.u)
                .Distinct()
                .AsQueryable();

            int totalCount = query.Count();

            var items = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new StaffConfirmCourseResponse
                {
                    Id = u.Id,
                    EmployeeId = u.EmployeeId,
                    FullName = u.FullName,
                    Email = u.Email!,
                    Department = u.Department!.Name,
                    Position = u.Position,
                    Status = EnrollmentConstants.Status.Enrolled,
                })
                .ToList();

            return new PagedResult<StaffConfirmCourseResponse>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public PagedResult<EligibleStaffResponse> GetEligibleStaff(int courseId, UserSearchDto searchDto)
        {
            var query = from u in _context.Users
                        join ur in _context.UserRoles on u.Id equals ur.UserId
                        join r in _context.Roles on ur.RoleId equals r.Id
                        join d in _context.Departments on u.DepartmentId equals d.Id
                        join e in _context.CourseEnrollments
                            .Where(x => x.CourseId == courseId)
                            on u.Id equals e.UserId into ue
                        from e in ue.DefaultIfEmpty()
                        where r.Name == UserRoles.Staff
                              && _context.Courses
                                  .Where(c => c.CourseId == courseId)
                                  .SelectMany(c => c.Departments)
                                  .Any(dep => dep.Id == d.Id)
                              && !_context.Certificates
                                  .Any(c => c.UserId == u.Id && c.CourseId == courseId)
                              && (string.IsNullOrEmpty(searchDto.Search)
                                  || u.FullName.Contains(searchDto.Search)
                                  || u.EmployeeId!.Contains(searchDto.Search)
                                  || u.Email!.Contains(searchDto.Search))
                              && (string.IsNullOrEmpty(searchDto.Status)
                                  || (searchDto.Status == EnrollmentConstants.Status.NotEnrolled && e == null)
                                  || (e != null && e.Status == searchDto.Status))
                        orderby u.FullName
                        select new EligibleStaffResponse
                        {
                            Id = u.Id,
                            EmployeeId = u.EmployeeId,
                            FullName = u.FullName,
                            Email = u.Email!,
                            Department = d.Name,
                            Position = u.Position,
                            Status = e.Status ?? EnrollmentConstants.Status.NotEnrolled,
                            Reason = e.RejectionReason ?? "N/A"
                        };

            int totalCount = query.Count();

            var items = query
                .Skip((searchDto.Page - 1) * searchDto.PageSize)
                .Take(searchDto.PageSize)
                .ToList();

            return new PagedResult<EligibleStaffResponse>
            {
                Items = items,
                TotalCount = totalCount,
                Page = searchDto.Page,
                PageSize = searchDto.PageSize
            };
        }

        public async Task<List<ApplicationUser>> GetUsersByRoleAsync(string role)
        {
            var users = await _context.Users
                .Include(u => u.Department)
                .Join(_context.UserRoles, u => u.Id, ur => ur.UserId, (u, ur) => new { u, ur })
                .Join(_context.Roles, x => x.ur.RoleId, r => r.Id, (x, r) => new { x.u, r })
                .Where(x => x.r.Name == role && x.u.IsActive)
                .Select(x => x.u)
                .Distinct()
                .OrderBy(u => u.FullName)
                .ToListAsync();

            return users;
        }


        public async Task CreateUserAsync(CreateUserDto req)
        {
            var existingEmail = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Email == req.Email);

            if (existingEmail != null)
            {
                throw new InvalidOperationException($"Email '{req.Email}' đã tồn tại trong hệ thống.");
            }

            // Kiểm tra EmployeeId đã tồn tại chưa
            var existingUser = await _userManager.Users
                .FirstOrDefaultAsync(u => u.EmployeeId == req.EmployeeId);

            if (existingUser != null)
            {
                throw new InvalidOperationException($"Mã Nhân viên '{req.EmployeeId}' đã tồn tại trong hệ thống.");
            }

            // Map sang ApplicationUser
            var user = new ApplicationUser
            {
                EmployeeId = req.EmployeeId,
                UserName = req.Email,
                Email = req.Email,
                PhoneNumber = req.Phone,
                FullName = req.FullName,
                Position = req.Position,
                DepartmentId = req.DepartmentId,
                IsActive = true,
                CreatedDate = DateTime.Now
            };

            // Tạo user với mật khẩu tạm
            var tempPassword = PasswordUtils.Generate(_userManager.Options.Password);
            var createResult = await _userManager.CreateAsync(user, tempPassword);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Không thể tạo người dùng: {errors}");
            }

            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);

            var addRoleResult = await _userManager.AddToRoleAsync(user, req.RoleName!);
            if (!addRoleResult.Succeeded)
            {
                var errors = string.Join(", ", addRoleResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Không thể gán vai trò cho người dùng: {errors}");
            }

            // Gửi email HTML
            var subject = "Kích hoạt tài khoản hệ thống đào tạo";
            var body = $@"
                <html>
                <body style='font-family:Arial, sans-serif; line-height:1.6; color:#333'>
                    <p>Chào <strong>{user.FullName}</strong>,</p>

                    <p>Tài khoản của bạn đã được tạo trên <strong>hệ thống đào tạo nội bộ</strong>.</p>

                    <p>
                        <b>Tên đăng nhập (Email):</b> {user.Email}<br/>
                        <b>Mật khẩu tạm:</b> {tempPassword}
                    </p>

                    <p><i>Hãy đổi mật khẩu ngay sau khi đăng nhập.</i></p>

                    <p>Trân trọng,<br/>
                    <b>Phòng IT</b></p>
                </body>
                </html>";

            Hangfire.BackgroundJob.Enqueue(() => EmailHelper.SendEmailAsync(
                    user.Email!,
                    subject,
                    body
                ));
        }

        public List<IdentityRole> GetRoles()
        {
            return _context.Roles.ToList();
        }

        public async Task<ApplicationUser?> GetUserProfileAsync(string userId)
        {
            return await _context.Users
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<List<UserCourseSummaryDto>> GetUserCouresSummaryAsync(string userId)
        {
            var classes = await _context.Classes
                .Include(c => c.Course)
                .Include(c => c.Schedules)
                .Include(c => c.Employees)
                .Where(c => c.Employees.Any(e => e.Id == userId))
                .ToListAsync();

            var result = new List<UserCourseSummaryDto>();

            foreach (var cls in classes)
            {
                var scheduleIds = cls.Schedules.Select(s => s.ScheduleId).ToList();

                var attendances = await _context.Attendances
                    .Where(a => scheduleIds.Contains(a.ScheduleId) && a.UserId == userId)
                    .ToListAsync();

                int totalSessions = cls.Schedules.Count;
                int absentDays = attendances.Count(a => a.Status == AttendanceConstants.Status.Absent);

                double attendanceRate = totalSessions > 0
                    ? Math.Round((double)absentDays / totalSessions * 100, 2)
                    : 0;
                var enrollment = await _context.CourseEnrollments
                    .FirstOrDefaultAsync(e => e.CourseId == cls.CourseId && e.UserId == userId);

                result.Add(new UserCourseSummaryDto
                {
                    ClassId = cls.ClassId,
                    ClassName = cls.ClassName,
                    CourseCode = cls.Course?.Code!,
                    CourseName = cls.Course?.CourseName!,
                    TotalSessions = totalSessions,
                    AbsentDays = absentDays,
                    AttendanceRate = attendanceRate,

                    Status = enrollment?.Status ?? EnrollmentConstants.Status.InProgress,
                    Score = enrollment?.Score,
                });
            }

            return result;
        }
    }
}
