using InternalTrainingSystem.Core.DB;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace InternalTrainingSystem.Core.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize] // Temporarily commented for testing
    public class ClassController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ClassController> _logger;

        public ClassController(ApplicationDbContext context, ILogger<ClassController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClassDto>>> GetClasses()
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

                return Ok(classesDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving classes");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<ActionResult<List<ClassDto>>> CreateClasses(CreateClassesDto createClassesDto)
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(currentUserId))
                {
                    // Lấy user đầu tiên trong database để làm CreatedBy
                    var firstUser = await _context.Users.FirstOrDefaultAsync(u => u.IsActive);
                    if (firstUser == null)
                    {
                        return BadRequest("No active users found in system");
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
                            .FirstOrDefaultAsync(c => c.CourseId == classRequest.CourseId && c.IsActive);
                        if (course == null)
                        {
                            return BadRequest($"Course with ID {classRequest.CourseId} not found or inactive");
                        }

                        // Check if mentor exists
                        var mentor = await _context.Users
                            .FirstOrDefaultAsync(u => u.Id == classRequest.MentorId && u.IsActive);
                        if (mentor == null)
                        {
                            return BadRequest($"Mentor with ID {classRequest.MentorId} not found or inactive");
                        }

                        // Validate all staff IDs exist
                        foreach (var staffId in classRequest.StaffIds)
                        {
                            var staffExists = await _context.Users.AnyAsync(u => u.Id == staffId && u.IsActive);
                            if (!staffExists)
                            {
                                return BadRequest($"Staff with ID {staffId} not found or inactive");
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
                    return Ok(createdClasses);
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
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
