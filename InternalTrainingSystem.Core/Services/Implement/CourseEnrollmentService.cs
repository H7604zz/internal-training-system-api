using InternalTrainingSystem.Core.DB;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Services.Interface;

namespace InternalTrainingSystem.Core.Services.Implement
{
    public class CourseEnrollmentService : ICourseEnrollmentService
    {
        private readonly ApplicationDbContext _context;

        public CourseEnrollmentService(ApplicationDbContext context)
        {
            _context = context;
        }
        public bool AddCourseEnrollment(CourseEnrollment courseEnrollment)
        {
            try
            {
                _context.CourseEnrollments.Add(courseEnrollment);
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
