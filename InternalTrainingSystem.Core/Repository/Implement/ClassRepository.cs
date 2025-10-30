using InternalTrainingSystem.Core.Configuration;
using InternalTrainingSystem.Core.Constants;
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

        public ClassRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, List<ClassDto>? Data)> CreateClassesAsync(
                            CreateClassRequestDto request,
                            List<StaffConfirmCourseResponse> confirmedUsers)
        {
            var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == request.CourseId);
            if (course == null) return (false, null);

            if (request.NumberOfClasses <= 0 || request.MaxMembers <= 0)
                return (false, null);

            // Shuffle users
            var rnd = new Random();
            var shuffledUsers = confirmedUsers.OrderBy(x => rnd.Next()).ToList();

            // Prepare classes
            var createdClasses = new List<Class>();
            for (int i = 1; i <= request.NumberOfClasses; i++)
            {
                var newClass = new Class
                {
                    ClassName = $"Lớp {course.Code}-{i}",
                    CourseId = request.CourseId,
                    Capacity = request.MaxMembers,
                    Status = ClassConstants.Status.Created,
                    IsActive = false,
                    CreatedDate = DateTime.UtcNow
                };
                createdClasses.Add(newClass);
            }

            var availableQueue = new Queue<int>(
                Enumerable.Range(0, createdClasses.Count)
            );

            int userIndex = 0;
            while (userIndex < shuffledUsers.Count && availableQueue.Count > 0)
            {
                var classIdx = availableQueue.Dequeue();
                var targetClass = createdClasses[classIdx];

                var user = shuffledUsers[userIndex];
                var appUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
                if (appUser != null)
                {
                    targetClass.Employees.Add(appUser);
                    userIndex++;
                }
                else
                {
                    userIndex++;
                }

                if (targetClass.Employees.Count < targetClass.Capacity)
                {
                    availableQueue.Enqueue(classIdx);
                }
            }

            _context.Classes.AddRange(createdClasses);
            await _context.SaveChangesAsync();

            var classDtos = createdClasses.Select(c => new ClassDto
            {
                ClassId = c.ClassId,
                ClassName = c.ClassName,
                CourseId = c.CourseId,
                CourseName = c.Course?.CourseName,
                MentorId = c.MentorId!,
                MentorName = c.Mentor != null ? c.Mentor.FullName : null,
                TotalMembers = c.Employees.Count,
                Employees = c.Employees.Select(e => new ClassEmployeeDto
                {
                    EmployeeId = e.Id,
                    FullName = e.FullName,
                    Email = e.Email
                }).ToList(),
                CreatedDate = c.CreatedDate,
                IsActive = c.IsActive
            }).ToList();

            return (true, classDtos);
        }

        public async Task<(bool Success, string Message, int Count)> CreateWeeklySchedulesAsync(CreateWeeklyScheduleRequest request)
        {
            var classEntity = await _context.Classes
                .Include(c => c.Employees)
                .FirstOrDefaultAsync(c => c.ClassId == request.ClassId);

            if (classEntity == null)
                return (false, "Không tìm thấy lớp học.", 0);

            var instructor = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.MentorId);
            if (instructor == null)
                return (false, "Không tìm thấy giảng viên.", 0);

            if (request.WeeklySchedules == null || request.WeeklySchedules.Count == 0)
                return (false, "Chưa có buổi học trong tuần đầu.", 0);

            var allSchedules = new List<Schedule>();

            for (int week = 0; week < request.NumberOfWeeks; week++)
            {
                var baseDate = request.StartWeek.AddDays(7 * week);

                foreach (var item in request.WeeklySchedules)
                {
                    if (!Enum.TryParse<DayOfWeek>(item.DayOfWeek, true, out var day))
                        return (false, $"Ngày '{item.DayOfWeek}' không hợp lệ.", 0);

                    var date = baseDate.AddDays((int)day - (int)request.StartWeek.DayOfWeek);
                    if (date < baseDate)
                        date = date.AddDays(7);

                    var classMemberIds = classEntity.Employees.Select(e => e.Id).ToList();

                    var start = item.StartTime;
                    var end = item.EndTime;

                    var conflictSchedules = await _context.Schedules
                        .Include(s => s.Class)
                        .ThenInclude(c => c.Employees)
                        .Where(s =>
                            s.Date == date &&
                            (
                                (s.StartTime <= start && s.EndTime > start) ||
                                (s.StartTime < end && s.EndTime >= end) ||   
                                (s.StartTime >= start && s.EndTime <= end) 
                            ) &&
                            s.Class!.Employees.Any(e => classMemberIds.Contains(e.Id))
                        )
                        .ToListAsync();

                    if (conflictSchedules.Count > 0)
                    {
                        var conflictedStudents = conflictSchedules
                            .SelectMany(s => s.Class!.Employees)
                            .Where(e => classMemberIds.Contains(e.Id))
                            .Select(e => e.FullName)
                            .Distinct()
                            .ToList();

                        var conflictList = string.Join(", ", conflictedStudents.Take(5));
                        return (false, $"Lịch học bị trùng cho các học viên: {conflictList}...", 0);
                    }

                    allSchedules.Add(new Schedule
                    {
                        Description = item.Description,
                        Date = date,
                        StartTime = item.StartTime,
                        EndTime = item.EndTime,
                        Location = item.Location,
                        IsOnline = false,
                        CourseId = request.CourseId,
                        InstructorId = request.MentorId,
                        ClassId = request.ClassId,
                        Status = ScheduleConstants.Status.Scheduled,
                        CreatedDate = DateTime.UtcNow
                    });
                }
            }

            _context.Schedules.AddRange(allSchedules);
            classEntity.Status = ClassConstants.Status.Scheduled;
            classEntity.IsActive = true;
            classEntity.MentorId = request.MentorId;
            _context.Classes.Update(classEntity);
            await _context.SaveChangesAsync();

            return (true, $"Đã tạo {allSchedules.Count} buổi học cho {request.NumberOfWeeks} tuần.", allSchedules.Count);
        }

        public async Task<ClassScheduleResponse> GetClassScheduleAsync(int classId)
        {
            var schedule = await _context.Schedules
                .Include(s => s.Course)
                .Include(s => s.Class)
                .Where(s => s.ClassId == classId)
                .Select(s => new ScheduleItemResponseDto
                {
                    ScheduleId = s.ScheduleId,
                    ClassId = s.ClassId,
                    ClassName = s.Class!.ClassName,
                    MentorId = s.InstructorId,
                    Mentor = s.Instructor.FullName,
                    CourseCode = s.Course.Code!,
                    CourseName = s.Course.CourseName!,
                    DayOfWeek = s.Date,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    Location = s.Location
                })
                .ToListAsync();

            return new ClassScheduleResponse
            {
                Success = true,
                Message = "Lấy lịch học thành công",
                Schedules = schedule
            };
        }

        public async Task<UserScheduleResponse> GetUserScheduleAsync(string staffId)
        {
            var staffClasses = await _context.Classes
                .Include(c => c.Employees)
                .Where(c => c.Employees.Any(e => e.Id == staffId))
                .Select(c => c.ClassId)
                .ToListAsync();

            if (staffClasses == null || staffClasses.Count == 0)
            {
                return new UserScheduleResponse
                {
                    Success = false,
                    Message = "Nhân viên chưa được xếp vào lớp nào.",
                    Schedules = new List<ScheduleItemResponseDto>()
                };
            }

            var schedules = await _context.Schedules
                .Include(s => s.Course)
                .Include(s => s.Class)
                .Include(s => s.Instructor)
                .Where(s => s.ClassId != null && staffClasses.Contains(s.ClassId.Value))
                .OrderBy(s => s.Date)
                .ThenBy(s => s.StartTime)
                .Select(s => new ScheduleItemResponseDto
                {
                    ScheduleId = s.ScheduleId,
                    ClassId = s.ClassId,
                    ClassName = s.Class!.ClassName,
                    MentorId = s.InstructorId,
                    Mentor = s.Instructor.FullName,
                    CourseCode = s.Course.Code!,
                    CourseName = s.Course.CourseName!,
                    DayOfWeek = s.Date,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    Location = s.Location
                })
                .ToListAsync();

            return new UserScheduleResponse
            {
                Success = true,
                Message = "Lấy lịch học thành công",
                Schedules = schedules
            };
        }

        public async Task<List<ClassEmployeeDto>> GetUserByClassAsync(int classId)
        {
            var classEntity = await _context.Classes
                .Include(c => c.Employees)
                .FirstOrDefaultAsync(c => c.ClassId == classId);

            if (classEntity == null)
                return new List<ClassEmployeeDto>();

            return classEntity.Employees.Select(e => new ClassEmployeeDto
            {
                EmployeeId = e.Id,
                FullName = e.FullName,
                Email = e.Email
            }).ToList();
        }

        public async Task<ClassDto?> GetClassDetailAsync(int classId)
        {
            var classEntity = await _context.Classes
                .Include(c => c.Course)
                .Include(c => c.Mentor)
                .Include(c => c.Employees)
                .FirstOrDefaultAsync(c => c.ClassId == classId);

            if (classEntity == null)
                return null;

            return new ClassDto
            {
                ClassId = classEntity.ClassId,
                ClassName = classEntity.ClassName,
                CourseId = classEntity.CourseId,
                CourseName = classEntity.Course?.CourseName,
                MentorId = classEntity.MentorId!,
                MentorName = classEntity.Mentor?.FullName,
                TotalMembers = classEntity.Employees.Count,
                IsActive = classEntity.IsActive,
                Status = classEntity.Status,
                CreatedDate = classEntity.CreatedDate,
            };
        }

        public async Task<List<ClassDto>> GetClassesByCourseAsync(int courseId)
        {
            var classes = await _context.Classes
                .Include(c => c.Mentor)
                .Include(c => c.Employees)
                .Where(c => c.CourseId == courseId)
                .ToListAsync();

            return classes.Select(c => new ClassDto
            {
                ClassId = c.ClassId,
                ClassName = c.ClassName,
                MentorId = c.MentorId!,
                MentorName = c.Mentor?.FullName,
                IsActive = c.IsActive,
                Status = c.Status,
                CreatedDate = c.CreatedDate,
                TotalMembers = c.Employees.Count
            }).ToList();
        }
    }
}
