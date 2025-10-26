using InternalTrainingSystem.Core.Configuration;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Models;

namespace InternalTrainingSystem.Core.Services.Interface
{
    public interface ICourseEnrollmentService
    {
        public bool AddCourseEnrollment(CourseEnrollment courseEnrollment);

        public CourseEnrollment GetCourseEnrollment(int courseId, string userId);

        public bool DeleteCourseEnrollment(int courseId, string userId);

        public bool UpdateCourseEnrollment(CourseEnrollment courseEnrollment);

        public Task<PagedResult<CourseListItemDto>> GetAllCoursesEnrollmentsByStaffAsync(GetAllCoursesRequest request);

        public Task AddRangeAsync(IEnumerable<CourseEnrollment> enrollments);
    }
}
