using InternalTrainingSystem.Core.Constants;
using InternalTrainingSystem.Core.DB;
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

        public List<ApplicationUser> GetUserRoleStaffConfirmCourse(int courseId)
        {
            var users = _context.Users
               .Include(u => u.CourseEnrollments)
               .Include(u => u.Department)
               .Join(_context.UserRoles, u => u.Id, ur => ur.UserId, (u, ur) => new { u, ur })
               .Join(_context.Roles, x => x.ur.RoleId, r => r.Id, (x, r) => new { x.u, r })
               .Where(x => x.r.Name == UserRoles.Staff
                           && _context.CourseEnrollments.Any(c => c.UserId == x.u.Id && c.CourseId == courseId && c.Status == EnrollmentConstants.Status.Enrolled))
               .Select(x => x.u)
               .Distinct()
               .ToList();

            return users;
        }

        public List<ApplicationUser> GetUserRoleEligibleStaff(int courseId)
        {
            var departmentIds = _context.Courses
               .Where(c => c.CourseId == courseId)
               .SelectMany(c => c.Departments.Select(d => d.Id))
               .ToList();

            var users = _context.Users
                .Include(u => u.CourseEnrollments)
                .Join(_context.UserRoles, u => u.Id, ur => ur.UserId, (u, ur) => new { u, ur })
                .Join(_context.Roles, x => x.ur.RoleId, r => r.Id, (x, r) => new { x.u, r })
                .Where(x => x.r.Name == UserRoles.Staff
                    && !_context.Certificates.Any(c => c.UserId == x.u.Id && c.CourseId == courseId)
                    && departmentIds.Contains(x.u.Department!.Id)
                )
                .Select(x => x.u)
                .Distinct()
                .ToList();

            return users;
        }

        public List<ApplicationUser> GetMentors()
        {
            var mentors = _context.Users
                .Join(_context.UserRoles, u => u.Id, ur => ur.UserId, (u, ur) => new { u, ur })
                .Join(_context.Roles, x => x.ur.RoleId, r => r.Id, (x, r) => new { x.u, r })
                .Where(x => x.r.Name == UserRoles.Mentor && x.u.IsActive)
                .Select(x => x.u)
                .Distinct()
                .ToList();

            return mentors;
        }

        public List<ApplicationUser> GetAllStaff()
        {
            var staff = _context.Users
                .Join(_context.UserRoles, u => u.Id, ur => ur.UserId, (u, ur) => new { u, ur })
                .Join(_context.Roles, x => x.ur.RoleId, r => r.Id, (x, r) => new { x.u, r })
                .Where(x => x.r.Name == UserRoles.Staff && x.u.IsActive)
                .Select(x => x.u)
                .Distinct()
                .ToList();

            return staff;
        }
    }
}