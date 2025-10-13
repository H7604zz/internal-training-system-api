using InternalTrainingSystem.Core.Models;

namespace InternalTrainingSystem.Core.Services.Interface
{
    public interface ICourseEnrollmentService
    {
        public bool AddCourseEnrollment(CourseEnrollment courseEnrollment);
    }
}
