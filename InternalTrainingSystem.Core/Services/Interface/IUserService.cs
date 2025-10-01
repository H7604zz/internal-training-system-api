using InternalTrainingSystem.Core.Models;

namespace InternalTrainingSystem.Core.Services.Interface
{
    public interface IUserService
    {
        public List<ApplicationUser> GetUserRoleStaffWithoutCertificate(int courseId);
    }
}
