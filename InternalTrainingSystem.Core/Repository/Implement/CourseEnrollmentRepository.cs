using InternalTrainingSystem.Core.Configuration;
using InternalTrainingSystem.Core.DB;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace InternalTrainingSystem.Core.Repository.Implement
{
    public class CourseEnrollmentRepository : ICourseEnrollmentRepository
    {
        private readonly ApplicationDbContext _context;

        public CourseEnrollmentRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<bool> AddCourseEnrollment(CourseEnrollment courseEnrollment)
        {
            try
            {
                await _context.CourseEnrollments.AddAsync(courseEnrollment);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<CourseEnrollment?> GetCourseEnrollment(int courseId, string userId)
        {
            return await _context.CourseEnrollments
                .Include(e => e.User)
                .Include(e => e.Course)
                .FirstOrDefaultAsync(e => e.CourseId == courseId && e.UserId == userId);
        }

        public async Task<bool> DeleteCourseEnrollment(int courseId, string userId)
        {

            var enrollment = await _context.CourseEnrollments
             .FirstOrDefaultAsync(e => e.CourseId == courseId && e.UserId == userId);
            if (enrollment == null) return false;

            _context.CourseEnrollments.Remove(enrollment);
            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateCourseEnrollment(CourseEnrollment courseEnrollment)
        {
            try
            {
                _context.CourseEnrollments.Update(courseEnrollment);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<PagedResult<CourseListItemDto>> GetAllCoursesEnrollmentsByStaffAsync(GetAllCoursesRequest request)
        {
            // Query từ bảng CourseEnrollment
            var query = _context.CourseEnrollments
                .Include(e => e.Course)
                    .ThenInclude(c => c.CourseCategory)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Departments)
                .Include(e => e.Course)
                    .ThenInclude(c => c.CreatedBy)
                .Where(e => e.UserId == request.UserId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                var status = request.Status.Trim();
                query = query.Where(e => e.Status == status);
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var searchTerm = request.Search.Trim().ToLower();
                query = query.Where(e =>
                    e.Course.CourseName.ToLower().Contains(searchTerm) ||
                    (e.Course.Description != null && e.Course.Description.ToLower().Contains(searchTerm)) ||
                    e.Course.CourseCategory.CategoryName.ToLower().Contains(searchTerm));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(e => e.Course.CreatedDate)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(e => new CourseListItemDto
                {
                    Id = e.Course.CourseId,
                    CourseId = e.Course.CourseId,
                    CourseName = e.Course.CourseName,
                    Code = e.Course.Code,
                    Description = e.Course.Description,
                    Duration = e.Course.Duration,
                    Level = e.Course.Level,
                    Category = e.Course.CourseCategory.CategoryName,
                    CategoryName = e.Course.CourseCategory.CategoryName,
                    IsActive = true,
                    IsOnline = e.Course.IsOnline,
                    IsMandatory = e.Course.IsMandatory,
                    CreatedDate = e.Course.CreatedDate,
                    Status = e.Status,
                    Departments = e.Course.Departments.Select(d => new DepartmentDto
                    {
                        DepartmentId = d.Id,
                        DepartmentName = d.Name
                    }).ToList(),
                    CreatedBy = e.Course.CreatedBy != null ? e.Course.CreatedBy.UserName : string.Empty,
                    UpdatedDate = e.Course.UpdatedDate,
                    UpdatedBy = string.Empty
                })
                .ToListAsync();

            return new PagedResult<CourseListItemDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }

        public async Task AddRangeAsync(IEnumerable<CourseEnrollment> enrollments)
        {
            await _context.CourseEnrollments.AddRangeAsync(enrollments);
        }
    }
}
