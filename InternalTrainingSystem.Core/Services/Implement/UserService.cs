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

        public PagedResult<EligibleStaffResponse> GetEligibleStaff(int courseId, int page, int pageSize)
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
                        orderby u.FullName
                        select new EligibleStaffResponse
                        {
                            EmployeeId = u.EmployeeId,
                            FullName = u.FullName,
                            Email = u.Email!,
                            Department = d.Name,
                            Position = u.Position,
                            Status = e.Status ?? EnrollmentConstants.Status.NotEnrolled,
                            Reason = e.RejectionReason ?? "Chưa có phản hồi"
                        };

            int totalCount = query.Count();

            var items = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedResult<EligibleStaffResponse>
            {
                Items = items,
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