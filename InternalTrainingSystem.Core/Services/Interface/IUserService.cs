using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Models;

namespace InternalTrainingSystem.Core.Services.Interface
{
    public interface IUserService
    {
        public PagedResultDto<EligibleStaffResponse> GetUserRoleEligibleStaff(int courseId, int page, int pageSize);
        public PagedResultDto<StaffConfirmCourseResponse> GetUserRoleStaffConfirmCourse(int courseId, int page, int pageSize);
        public List<ApplicationUser> GetUsersByRole(string role);
    }
}
