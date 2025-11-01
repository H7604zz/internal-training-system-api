using InternalTrainingSystem.Core.Configuration;
using InternalTrainingSystem.Core.Constants;
using InternalTrainingSystem.Core.DB;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Helper;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Repository.Interface;
using InternalTrainingSystem.Core.Services.Implement;
using Microsoft.AspNetCore.Mvc;
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

        public async Task<bool> CreateClassesAsync(
                CreateClassRequestDto request,
                List<StaffConfirmCourseResponse> confirmedUsers)
        {
            var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == request.CourseId);
            if (course == null)
                return false;

            if (request.NumberOfClasses <= 0)
                return false;

            if (confirmedUsers == null || confirmedUsers.Count == 0)
                return false;

            var rnd = new Random();
            var shuffledUsers = confirmedUsers.OrderBy(x => rnd.Next()).ToList();

            int totalUsers = shuffledUsers.Count;
            int numClasses = request.NumberOfClasses;

            int baseCount = totalUsers / numClasses;          
            int remainder = totalUsers % numClasses;          

            var createdClasses = new List<Class>();
            int userIndex = 0;

            for (int i = 1; i <= numClasses; i++)
            {
                int currentClassSize = baseCount + (i <= remainder ? 1 : 0);

                var newClass = new Class
                {
                    ClassName = $"Lớp {course.Code}-{i}",
                    CourseId = request.CourseId,
                    Capacity = currentClassSize,
                    Status = ClassConstants.Status.Created,
                    IsActive = false,
                    CreatedDate = DateTime.UtcNow
                };

                for (int j = 0; j < currentClassSize && userIndex < totalUsers; j++)
                {
                    var user = shuffledUsers[userIndex];
                    var appUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
                    if (appUser != null)
                    {
                        newClass.Employees.Add(appUser);
                    }
                    userIndex++;
                }

                createdClasses.Add(newClass);
            }

            _context.Classes.AddRange(createdClasses);
            await _context.SaveChangesAsync();

            return true;
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

            string joinUrl = await ZoomHelper.CreateRecurringMeetingAndGetJoinUrlAsync();

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

                    if (!string.IsNullOrWhiteSpace(item.Location))
                    {
                        var roomConflicts = await _context.Schedules
                            .Include(s => s.Class)
                            .Where(s =>
                                s.Date == date &&
                                s.Location == item.Location &&
                                s.ClassId != request.ClassId &&
                                (
                                    (s.StartTime <= start && s.EndTime > start) ||
                                    (s.StartTime < end && s.EndTime >= end) ||
                                    (s.StartTime >= start && s.EndTime <= end)
                                )
                            )
                            .ToListAsync();

                        if (roomConflicts.Count > 0)
                        {
                            var classNames = roomConflicts
                                .Select(s => s.Class!.ClassName)
                                .Distinct()
                                .ToList();

                            var classList = string.Join(", ", classNames.Take(3));
                            return (false, $"Phòng '{item.Location}' đã có lớp ({classList}) trong khung giờ này.", 0);
                        }
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
                        CreatedDate = DateTime.UtcNow,
                        OnlineLink = joinUrl != null ? joinUrl : "Chưa có link học"
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
                .Include(c => c.Schedules)
                .FirstOrDefaultAsync(c => c.ClassId == classId);

            if (classEntity == null)
                return new List<ClassEmployeeDto>();

            var scheduleIds = classEntity.Schedules.Select(s => s.ScheduleId).ToList();

            var attendances = await _context.Attendances
                .Where(a => scheduleIds.Contains(a.ScheduleId))
                .ToListAsync();

            var result = classEntity.Employees.Select(e => new ClassEmployeeDto
            {
                EmployeeId = e.Id,
                FullName = e.FullName,
                Email = e.Email,
                AbsentNumberDay = attendances
            .Count(a => a.UserId == e.Id && a.Status == AttendanceConstants.Status.Absent)
            }).ToList();

            return result;
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
                MaxStudents = classEntity.Employees.Count,
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
                MaxStudents = c.Employees.Count
            }).ToList();
        }

        public async Task<(bool Success, string Message)> SwapClassesAsync(SwapClassRequest request)
        {
            var user1 = await _context.Users.FirstOrDefaultAsync(u => u.EmployeeId == request.EmployeeId1);
            var user2 = await _context.Users.FirstOrDefaultAsync(u => u.EmployeeId == request.EmployeeId2);

            if (user1 == null || user2 == null)
                return (false, "Không tìm thấy 1 hoặc cả 2 học viên.");

            var class1 = await _context.Classes
                .Include(c => c.Employees)
                .Include(c => c.Course)
                .FirstOrDefaultAsync(c => c.ClassName == request.ClassName1);

            var class2 = await _context.Classes
                .Include(c => c.Employees)
                .Include(c => c.Course)
                .FirstOrDefaultAsync(c => c.ClassName == request.ClassName2);

            if (class1 == null || class2 == null)
                return (false, "Không tìm thấy 1 hoặc cả 2 lớp học.");

            if (class1.CourseId != class2.CourseId)
                return (false, $"Hai lớp '{class1.ClassName}' và '{class2.ClassName}' không cùng một môn học, không thể đổi.");

            if (!class1.Employees.Any(e => e.EmployeeId == user1.EmployeeId))
                return (false, $"{user1.FullName} không thuộc lớp {class1.ClassName}.");

            if (!class2.Employees.Any(e => e.EmployeeId == user2.EmployeeId))
                return (false, $"{user2.FullName} không thuộc lớp {class2.ClassName}.");

            class1.Employees.Remove(user1);
            class2.Employees.Remove(user2);

            class1.Employees.Add(user2);
            class2.Employees.Add(user1);

            await _context.SaveChangesAsync();

            return (true, $"Đã đổi lớp giữa {user1.FullName} ({class1.ClassName}) và {user2.FullName} ({class2.ClassName}) trong môn học '{class1.Course.CourseName}'.");
        }

        public async Task<ActionResult<PagedResult<ClassDto>>> GetClassesAsync(int page, int pageSize)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            var query = _context.Classes
                .Include(c => c.Course)
                .Include(c => c.Mentor)
                .AsNoTracking();

            var totalCount = await query.CountAsync();

            var classes = await query
                .OrderByDescending(c => c.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new ClassDto
                {
                    ClassId = c.ClassId,
                    ClassName = c.ClassName,
                    CourseId = c.CourseId,
                    CourseName = c.Course.CourseName,
                    MentorId = c.MentorId ?? string.Empty,
                    MentorName = c.Mentor != null ? c.Mentor.FullName : null,
                    MaxStudents = c.Capacity,
                    CreatedDate = c.CreatedDate,
                    IsActive = c.IsActive,
                    Status = c.Status
                })
                .ToListAsync();

            return new PagedResult<ClassDto>
            {
                Items = classes,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }
    }
}
