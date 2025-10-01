using InternalTrainingSystem.Core.DB;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace InternalTrainingSystem.Core.Services.Implement
{
    public class CourseService : ICourseService
    {
        private readonly ApplicationDbContext _context;
        public CourseService( ApplicationDbContext context)
        {
            _context = context;
        }

        public Course? CreateCourses(Course course)
        {
            _context.Courses.Add(course);
            _context.SaveChanges();
            return course;
        }

        public bool DeleteCoursesByCourseId(int id)
        {
            try
            {
                var deleteCourse = _context.Courses.SingleOrDefault(m => m.CourseId == id);
                if (deleteCourse == null) return false;
                _context.Courses.Remove(deleteCourse);
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public List<Course> GetCourses()
        {
            return _context.Courses.ToList();
        }

        public bool UpdateCourses(Course course)
        {
            try
            {
                var existing = _context.Courses.Find(course.CourseId);
                if (existing == null)
                {
                    return false; 
                }
                _context.Entry(existing).CurrentValues.SetValues(course);
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating course {course.CourseId}: {ex.Message}");
                return false;
            }
        }

        public bool ToggleStatus(int id, bool isActive)
        {
            var course = _context.Courses.Find(id);
            if (course == null) return false;

            course.IsActive = isActive;
            course.UpdatedDate = DateTime.UtcNow;
            return _context.SaveChanges() > 0;
        }
    }
}
