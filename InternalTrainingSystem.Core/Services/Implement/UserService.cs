using InternalTrainingSystem.Core.Configuration;
using InternalTrainingSystem.Core.Constants;
using InternalTrainingSystem.Core.DB;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace InternalTrainingSystem.Core.Services.Implement
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        public PagedResult<StaffConfirmCourseResponse> GetUserRoleStaffConfirmCourse(int courseId, int page, int pageSize)
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

        public PagedResult<EligibleStaffResponse> GetUserRoleEligibleStaff(int courseId, int page, int pageSize)
        {
            var departmentIds = _context.Courses
                .Where(c => c.CourseId == courseId)
                .SelectMany(c => c.Departments.Select(d => d.Id))
                .ToList();

            var query = _context.Users
                .Include(u => u.CourseEnrollments)
                .Include(u => u.Department)
                .Join(_context.UserRoles, u => u.Id, ur => ur.UserId, (u, ur) => new { u, ur })
                .Join(_context.Roles, x => x.ur.RoleId, r => r.Id, (x, r) => new { x.u, r })
                .Where(x => x.r.Name == UserRoles.Staff
                    && !_context.Certificates.Any(c => c.UserId == x.u.Id && c.CourseId == courseId)
                    && departmentIds.Contains(x.u.Department!.Id)
                )
                .Select(x => x.u)
                .Distinct();

            int totalCount = query.Count();

            var users = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new EligibleStaffResponse
                {
                    EmployeeId = u.EmployeeId,
                    FullName = u.FullName,
                    Email = u.Email!,
                    Department = u.Department!.Name,
                    Position = u.Position,
                    Status = u.CourseEnrollments.FirstOrDefault(e => e.CourseId == courseId)!.Status,
                    Reason = u.CourseEnrollments.FirstOrDefault(e => e.CourseId == courseId)!.RejectionReason ?? "Không lý do"
                })
                .ToList();

            return new PagedResult<EligibleStaffResponse>
            {
                Items = users,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public List<ApplicationUser> GetUsersByRole(string role)
        {
            var mentors = _context.Users
                .Join(_context.UserRoles, u => u.Id, ur => ur.UserId, (u, ur) => new { u, ur })
                .Join(_context.Roles, x => x.ur.RoleId, r => r.Id, (x, r) => new { x.u, r })
                .Where(x => x.r.Name == role && x.u.IsActive)
                .Select(x => x.u)
                .Distinct()
                .ToList();

            return mentors;
        }

    }
}