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

        public List<ApplicationUser> GetUserRoleStaffWithoutCertificate(int courseId)
        {
            var users = _context.Users
                .Include(u => u.CourseEnrollments)
                .Join(_context.UserRoles, u => u.Id, ur => ur.UserId, (u, ur) => new { u, ur })
                .Join(_context.Roles, x => x.ur.RoleId, r => r.Id, (x, r) => new { x.u, r })
                .Where(x => x.r.Name == UserRoles.Staff
                            && !_context.Certificates.Any(c => c.UserId == x.u.Id && c.CourseId == courseId))
                .Select(x => x.u)
                .Distinct()
                .ToList();

            return users;
        }
    }
}