using InternalTrainingSystem.Core.Models;

namespace InternalTrainingSystem.Core.Services.Interface
{
    public interface IUserService
    {
        public List<ApplicationUser> GetUserRoleEligibleStaff(int courseId);
        public List<ApplicationUser> GetUserRoleStaffConfirmCourse(int courseId);
        public List<ApplicationUser> GetMentors();
        public List<ApplicationUser> GetAllStaff();
    }
}
