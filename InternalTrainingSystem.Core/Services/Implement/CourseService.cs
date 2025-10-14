using InternalTrainingSystem.Core.DB;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InternalTrainingSystem.Core.Configuration;

namespace InternalTrainingSystem.Core.Services.Implement
{
    public class CourseService : ICourseService
    {
        private readonly ApplicationDbContext _context;

        public CourseService(ApplicationDbContext context)
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
        public async Task<PagedResult<CourseListItemDto>> SearchAsync(CourseSearchRequest req, CancellationToken ct = default)
        {
            var page = req.Page <= 0 ? 1 : req.Page;
            var pageSize = req.PageSize is < 1 or > 200 ? 20 : req.PageSize;

            IQueryable<Course> q = _context.Courses
                .Include(c => c.CourseCategory)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(req.Q))
            {
                var k = req.Q.Trim().ToLowerInvariant();
                q = q.Where(c =>
                    c.CourseName.ToLower().Contains(k) ||
                    (c.Description != null && c.Description.ToLower().Contains(k)));
            }

            if (!string.IsNullOrWhiteSpace(req.Category))
            {
                var cat = req.Category.Trim();
                var catUpper = cat.ToUpper();
                q = q.Where(c => c.CourseCategory.CategoryName.ToUpper() == catUpper);
            }

            if (req.Categories != null && req.Categories.Count > 0)
            {
                var set = req.Categories
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => s.Trim().ToUpper())
                    .ToHashSet();

                q = q.Where(c => set.Contains(c.CourseCategory.CategoryName.ToUpper()));
            }

            // Filters
            if (req.CategoryId.HasValue)
                q = q.Where(c => c.CourseCategoryId == req.CategoryId.Value);

            if (req.IsActive.HasValue)
                q = q.Where(c => c.IsActive == req.IsActive.Value);

            if (!string.IsNullOrWhiteSpace(req.Level))
                q = q.Where(c => c.Level == req.Level);

            if (!string.IsNullOrWhiteSpace(req.Department))
                q = q.Where(c => c.Department == req.Department);

            if (req.DurationFrom.HasValue)
                q = q.Where(c => c.Duration >= req.DurationFrom.Value);

            if (req.DurationTo.HasValue)
                q = q.Where(c => c.Duration <= req.DurationTo.Value);

            if (req.CreatedFrom.HasValue)
                q = q.Where(c => c.CreatedDate >= req.CreatedFrom.Value);

            if (req.CreatedTo.HasValue)
                q = q.Where(c => c.CreatedDate <= req.CreatedTo.Value);

            // Sorting
            q = ApplySort(q, req.Sort);

            // Total + Page
            var total = await q.CountAsync(ct);

            var items = await q
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CourseListItemDto
                {
                    CourseId = c.CourseId,
                    CourseName = c.CourseName,
                    Description = c.Description,
                    CourseCategoryId = c.CourseCategoryId,
                    CourseCategoryName = c.CourseCategory.CategoryName,
                    Duration = c.Duration,
                    Level = c.Level,
                    Department = c.Department,
                    IsActive = c.IsActive,
                    CreatedDate = c.CreatedDate
                })
                .ToListAsync(ct);

            return new PagedResult<CourseListItemDto>
            {
                Items = items,
                Total = total,
                Page = page,
                PageSize = pageSize
            };
        }
        private static IQueryable<Course> ApplySort(IQueryable<Course> q, string? sort)
        {
            return sort switch
            {
                "CourseName" => q.OrderBy(c => c.CourseName),
                "-CourseName" => q.OrderByDescending(c => c.CourseName),
                "Duration" => q.OrderBy(c => c.Duration),
                "-Duration" => q.OrderByDescending(c => c.Duration),
                "CreatedDate" => q.OrderBy(c => c.CreatedDate),
                "-CreatedDate" => q.OrderByDescending(c => c.CreatedDate),
                "Level" => q.OrderBy(c => c.Level),
                "-Level" => q.OrderByDescending(c => c.Level),
                _ => q.OrderByDescending(c => c.CreatedDate) // default
            };
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
                    Department = c.Department,
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
                    Department = c.Department,
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
                Department = course.Department,
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
