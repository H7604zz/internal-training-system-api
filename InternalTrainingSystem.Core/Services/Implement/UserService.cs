using InternalTrainingSystem.Core.Constants;
using InternalTrainingSystem.Core.DB;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Services.Interface;

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
            var users = (from u in _context.Users
                         join ur in _context.UserRoles on u.Id equals ur.UserId
                         join r in _context.Roles on ur.RoleId equals r.Id
                         where r.Name == UserRoles.Staff
                               && !_context.Certificates.Any(c => c.UserId == u.Id && c.CourseId == courseId)
                         select u)
             .Distinct()
             .ToList();

            return users;
        }
    }
}