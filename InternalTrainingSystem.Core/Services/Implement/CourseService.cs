using InternalTrainingSystem.Core.DB;
using InternalTrainingSystem.Core.DTOs;
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
            return _context.Courses.FirstOrDefault(c => c.CourseId == couseId);
        }

        public async Task<IEnumerable<CourseListDto>> GetCoursesByIdentifiersAsync(List<string> identifiers)
        {
            if (identifiers == null || !identifiers.Any())
            {
                return new List<CourseListDto>();
            }

            var courseIds = new List<int>();
            var courseNames = new List<string>();

            foreach (var identifier in identifiers)
            {
                if (int.TryParse(identifier, out int courseId))
                {
                    courseIds.Add(courseId);
                }
                else
                {
                    courseNames.Add(identifier);
                }
            }

            return await _context.Courses
                .Include(c => c.CourseCategory)
                .Where(c => courseIds.Contains(c.CourseId) || 
                           courseNames.Any(name => c.CourseName.ToLower().Contains(name.ToLower())))
                .OrderByDescending(c => c.CreatedDate)
                .Select(c => new CourseListDto
                {
                    CourseId = c.CourseId,
                    CourseName = c.CourseName,
                    Description = c.Description,
                    Duration = c.Duration,
                    Level = c.Level,
                    CategoryName = c.CourseCategory.CategoryName,
                    IsActive = c.IsActive,
                    CreatedDate = c.CreatedDate
                })
                .ToListAsync();
        }
    }
}
