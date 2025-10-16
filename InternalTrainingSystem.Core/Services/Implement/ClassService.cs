using InternalTrainingSystem.Core.DB;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace InternalTrainingSystem.Core.Services.Implement
{
    public class ClassService : IClassService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ClassService> _logger;

        public ClassService(ApplicationDbContext context, ILogger<ClassService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<ClassDto>> GetClassesAsync()
        {
            try
            {
                var classes = await _context.Classes
                    .Include(c => c.Course)
                    .Include(c => c.Mentor)
                    .Include(c => c.ClassEnrollments)
                        .ThenInclude(ce => ce.Student)
                    .Where(c => c.IsActive)
                    .OrderByDescending(c => c.CreatedDate)
                    .ToListAsync();

                var classesDto = classes.Select(c => new ClassDto
                {
                    ClassId = c.ClassId,
                    ClassName = c.ClassName,
                    CourseId = c.CourseId,
                    CourseName = c.Course?.CourseName,
                    MentorId = c.MentorId,
                    MentorName = c.Mentor?.FullName,
                    Students = c.ClassEnrollments?.Where(ce => ce.IsActive).Select(ce => new ClassStudentDto
                    {
                        StudentId = ce.StudentId,
                        StudentName = ce.Student?.FullName,
                        StudentEmail = ce.Student?.Email
                    }).ToList() ?? new List<ClassStudentDto>(),
                    CreatedDate = c.CreatedDate,
                    IsActive = c.IsActive
                }).ToList();

                return classesDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving classes");
                throw;
            }
        }

        public async Task<List<ClassDto>> CreateClassesAsync(CreateClassesDto createClassesDto, string? currentUserId)
        {
            try
            {
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
                            .FirstOrDefaultAsync(c => c.CourseId == classRequest.CourseId && c.Status == Constants.CourseConstants.Status.Active);
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
                        foreach (var staffId in classRequest.StaffIds)
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
                            ClassName = $"{course.CourseName} - Class",
                            CourseId = classRequest.CourseId,
                            MentorId = classRequest.MentorId,
                            StartDate = DateTime.UtcNow,
                            MaxStudents = classRequest.StaffIds.Count,
                            Status = "Active",
                            CreatedById = currentUserId,
                            CreatedDate = DateTime.UtcNow,
                            IsActive = true
                        };

                        _context.Classes.Add(classEntity);
                        await _context.SaveChangesAsync();

                        // Enroll all staff members
                        foreach (var staffId in classRequest.StaffIds)
                        {
                            var enrollment = new ClassEnrollment
                            {
                                ClassId = classEntity.ClassId,
                                StudentId = staffId,
                                Status = "Enrolled",
                                EnrollmentDate = DateTime.UtcNow,
                                CreatedDate = DateTime.UtcNow,
                                IsActive = true
                            };

                            _context.ClassEnrollments.Add(enrollment);
                        }

                        await _context.SaveChangesAsync();

                        // Get created class with all info
                        var createdClass = await _context.Classes
                            .Include(c => c.Course)
                            .Include(c => c.Mentor)
                            .Include(c => c.ClassEnrollments)
                                .ThenInclude(ce => ce.Student)
                            .FirstOrDefaultAsync(c => c.ClassId == classEntity.ClassId);

                        var classDto = new ClassDto
                        {
                            ClassId = createdClass!.ClassId,
                            ClassName = createdClass.ClassName,
                            CourseId = createdClass.CourseId,
                            CourseName = createdClass.Course?.CourseName,
                            MentorId = createdClass.MentorId,
                            MentorName = createdClass.Mentor?.FullName,
                            Students = createdClass.ClassEnrollments?.Where(ce => ce.IsActive).Select(ce => new ClassStudentDto
                            {
                                StudentId = ce.StudentId,
                                StudentName = ce.Student?.FullName,
                                StudentEmail = ce.Student?.Email
                            }).ToList() ?? new List<ClassStudentDto>(),
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
                _logger.LogError(ex, "Error creating classes");
                throw;
            }
        }
    }
}
