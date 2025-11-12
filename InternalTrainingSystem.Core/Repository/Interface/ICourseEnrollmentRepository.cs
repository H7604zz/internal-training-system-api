using InternalTrainingSystem.Core.Configuration;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Models;

namespace InternalTrainingSystem.Core.Repository.Interface
{
    public interface ICourseEnrollmentRepository
    {
        public Task<bool> AddCourseEnrollment(CourseEnrollment courseEnrollment);

        public Task<CourseEnrollment> GetCourseEnrollment(int courseId, string userId);

        public Task<bool> DeleteCourseEnrollment(int courseId, string userId);

        public Task<bool> UpdateCourseEnrollment(CourseEnrollment courseEnrollment);

        public Task<PagedResult<CourseListItemDto>> GetAllCoursesEnrollmentsByStaffAsync(GetAllCoursesRequest request, string uid);

        public Task AddRangeAsync(IEnumerable<CourseEnrollment> enrollments);

        public Task BulkUpdateEnrollmentsToEnrolledAsync(List<EligibleStaffResponse> eligibleUsers, int courseId);
    }
}
