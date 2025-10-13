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

        public async Task<IEnumerable<CourseListDto>> GetAllCoursesAsync()
        {
            return await _context.Courses
                .Include(c => c.CourseCategory)
                .Where(c => c.IsActive)
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

        public async Task<CourseDetailDto?> GetCourseDetailAsync(int courseId)
        {
            var course = await _context.Courses
                .Include(c => c.CourseCategory)
                .Include(c => c.CourseEnrollments)
                .FirstOrDefaultAsync(c => c.CourseId == courseId);

            if (course == null)
            {
                return null;
            }

            // Calculate enrollment count
            var enrollmentCount = course.CourseEnrollments?.Count ?? 0;

            // For now, we'll use a default rating of 4.5. 
            // In the future, this should be calculated from actual ratings
            var averageRating = 4.5;

            return new CourseDetailDto
            {
                CourseId = course.CourseId,
                CourseName = course.CourseName,
                Description = course.Description,
                Duration = course.Duration,
                Level = course.Level,
                CategoryName = course.CourseCategory?.CategoryName ?? "Unknown",
                CategoryId = course.CourseCategoryId,
                IsActive = course.IsActive,
                CreatedDate = course.CreatedDate,
                UpdatedDate = course.UpdatedDate,
                Prerequisites = null, // Not available in current model
                Objectives = null, // Not available in current model
                Price = null, // Not available in current model
                EnrollmentCount = enrollmentCount,
                AverageRating = averageRating
            };
        }
    }
}
