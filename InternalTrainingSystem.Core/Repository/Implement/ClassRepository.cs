using InternalTrainingSystem.Core.Configuration;
using InternalTrainingSystem.Core.DB;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Repository.Interface;
using InternalTrainingSystem.Core.Services.Implement;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace InternalTrainingSystem.Core.Repository.Implement
{
    public class ClassRepository : IClassRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ClassRepository(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<PagedResult<ClassDto>> GetClassesAsync(GetAllClassesRequest request)
        {
            try
            {
                var query = _context.Classes
                    .Include(c => c.Course)
                    .Include(c => c.Mentor)
                    .Include(c => c.Capacity)
                    .Where(c => c.IsActive)
                    .AsQueryable();

                // Lọc theo từ khóa (tìm theo tên lớp, tên khóa học, tên mentor)
                if (!string.IsNullOrWhiteSpace(request.Search))
                {
                    string keyword = request.Search.Trim().ToLower();
                    query = query.Where(c =>
                        c.ClassName.ToLower().Contains(keyword) ||
                        (c.Course != null && c.Course.CourseName.ToLower().Contains(keyword)) ||
                        (c.Mentor != null && c.Mentor.FullName.ToLower().Contains(keyword))
                    );
                }

                // Tổng số bản ghi
                int totalCount = await query.CountAsync();

                // Phân trang và ánh xạ sang DTO trong một query
                var items = await query
                    .OrderByDescending(c => c.CreatedDate)
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(c => new ClassDto
                    {
                        ClassId = c.ClassId,
                        ClassName = c.ClassName,
                        CourseId = c.CourseId,
                        CourseName = c.Course != null ? c.Course.CourseName : null,
                        MentorId = c.MentorId,
                        MentorName = c.Mentor != null ? c.Mentor.FullName : null,
                        Employees = c.Employees
                            .Select(s => new ClassEmployeeDto
                            {
                                EmployeeId = s.Id,
                                FullName = s.FullName,
                                Email = s.Email
                            }).ToList(),
                        CreatedDate = c.CreatedDate,
                        IsActive = c.IsActive
                    })
                    .ToListAsync();

                // Trả về kết quả phân trang
                return new PagedResult<ClassDto>
                {
                    Items = items,
                    TotalCount = totalCount,
                    Page = request.Page,
                    PageSize = request.PageSize
                };
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<List<ClassDto>> CreateClassesAsync(CreateClassesDto createClassesDto)
        {
            try
            {
                // Lấy currentUserId từ claims
                var currentUserId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(currentUserId))
                {
                    // Lấy user đầu tiên trong database để làm CreatedBy
                    var firstUser = await _context.Users.FirstOrDefaultAsync(u => u.IsActive);
                    if (firstUser == null)
                    {
                        throw new InvalidOperationException("No active users found in system");
                    }
                    currentUserId = firstUser.Id;
                }

                var createdClasses = new List<ClassDto>();

                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    foreach (var classRequest in createClassesDto.Classes)
                    {
                        // Check if course exists
                        var course = await _context.Courses
                            .FirstOrDefaultAsync(c => c.CourseId == classRequest.CourseId && c.Status == Constants.CourseConstants.Status.Pending);
                        if (course == null)
                        {
                            throw new ArgumentException($"Course with ID {classRequest.CourseId} not found or inactive");
                        }

                        // Check if mentor exists
                        var mentor = await _context.Users
                            .FirstOrDefaultAsync(u => u.Id == classRequest.MentorId && u.IsActive);
                        if (mentor == null)
                        {
                            throw new ArgumentException($"Mentor with ID {classRequest.MentorId} not found or inactive");
                        }

                        // Validate all staff IDs exist
                        foreach (var staffId in classRequest.EmployeeIds)
                        {
                            var staffExists = await _context.Users.AnyAsync(u => u.Id == staffId && u.IsActive);
                            if (!staffExists)
                            {
                                throw new ArgumentException($"Staff with ID {staffId} not found or inactive");
                            }
                        }

                        // Create class
                        var classEntity = new Class
                        {
                            ClassName = $"{course.CourseName}",
                            CourseId = classRequest.CourseId,
                            MentorId = classRequest.MentorId,
                            StartDate = DateTime.UtcNow,
                            Capacity = classRequest.EmployeeIds.Count,
                            Status = "Active",
                            CreatedById = currentUserId,
                            CreatedDate = DateTime.UtcNow,
                            IsActive = true
                        };

                        _context.Classes.Add(classEntity);
                        await _context.SaveChangesAsync();

                        // Add all staff members to the class using many-to-many relationship
                        var staffUsers = await _context.Users
                            .Where(u => classRequest.EmployeeIds.Contains(u.Id) && u.IsActive)
                            .ToListAsync();

                        foreach (var staff in staffUsers)
                        {
                            classEntity.Employees.Add(staff);
                        }

                        await _context.SaveChangesAsync();

                        // Get created class with all info
                        var createdClass = await _context.Classes
                            .Include(c => c.Course)
                            .Include(c => c.Mentor)
                            .Include(c => c.Employees)
                            .FirstOrDefaultAsync(c => c.ClassId == classEntity.ClassId);

                        var classDto = new ClassDto
                        {
                            ClassId = createdClass!.ClassId,
                            ClassName = createdClass.ClassName,
                            CourseId = createdClass.CourseId,
                            CourseName = createdClass.Course?.CourseName,
                            MentorId = createdClass.MentorId,
                            MentorName = createdClass.Mentor?.FullName,
                            Employees = createdClass.Employees?.Select(s => new ClassEmployeeDto
                            {
                                EmployeeId = s.Id,
                                FullName = s.FullName,
                                Email = s.Email
                            }).ToList() ?? new List<ClassEmployeeDto>(),
                            CreatedDate = createdClass.CreatedDate,
                            IsActive = createdClass.IsActive
                        };

                        createdClasses.Add(classDto);
                    }

                    await transaction.CommitAsync();
                    return createdClasses;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
