using InternalTrainingSystem.Core.DB;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Services.Interface;
using Microsoft.EntityFrameworkCore;

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

        public CourseEnrollment? GetCourseEnrollment(int courseId, string userId)
        {
            return _context.CourseEnrollments
                .Include(e => e.User)
                .Include(e => e.Course)
                .FirstOrDefault(e => e.CourseId == courseId && e.UserId == userId);
        }
         
        public bool DeleteCourseEnrollment(int courseId, string userId)
        {

            var enrollment = _context.CourseEnrollments
             .FirstOrDefault(e => e.CourseId == courseId && e.UserId == userId);
            if (enrollment == null) return false;

            _context.CourseEnrollments.Remove(enrollment);
            try
            {
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool UpdateCourseEnrollment(CourseEnrollment courseEnrollment)
        {
            try
            {
                _context.CourseEnrollments.Update(courseEnrollment);
                _context.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
