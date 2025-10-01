using InternalTrainingSystem.Core.DB;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace InternalTrainingSystem.Core.Services.Implement
{
    public class CourseService : ICourseService
    {
        private readonly ApplicationDbContext _context;

        public CourseService(ApplicationDbContext context)
        {
            _context = context;
        }

        public Course? GetCourseByCourseID(int? couseId)
        {
            return _context.Courses
                 .Include(c => c.CourseCategory)
                 .Include(c => c.CreatedBy)
                 .FirstOrDefault(c => c.CourseId == couseId);
        }
    }
}
