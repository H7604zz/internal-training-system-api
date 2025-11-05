using DocumentFormat.OpenXml.Spreadsheet;
using InternalTrainingSystem.Core.Configuration;
using InternalTrainingSystem.Core.Constants;
using InternalTrainingSystem.Core.DB;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Helper;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Repository.Interface;
using InternalTrainingSystem.Core.Services.Implement;
using Microsoft.EntityFrameworkCore;


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
                List<StaffConfirmCourseResponse> confirmedUsers, string createdById)
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

                string baseClassName = $"Lớp {course.Code}-{i}";
                string finalClassName = baseClassName;
                int suffix = 1;

                while (await _context.Classes.AnyAsync(c => c.ClassName == finalClassName) ||
                        createdClasses.Any(c => c.ClassName == finalClassName)
                )
                {
                    finalClassName = $"{baseClassName}-{suffix}";
                    suffix++;
                }
                var newClass = new Class
                {
                    ClassName = finalClassName,
                    CourseId = request.CourseId,
                    Capacity = currentClassSize,
                    Description = request.Description,
                    Status = ClassConstants.Status.Created,
                    IsActive = false,
                    CreatedById = createdById,
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

        public async Task<(bool Success, string Message)> CreateWeeklySchedulesAsync(CreateWeeklyScheduleRequest request)
        {
            var classEntity = await _context.Classes
                .Include(c => c.Employees)
                .FirstOrDefaultAsync(c => c.ClassId == request.ClassId);

            if (classEntity == null)
                return (false, "Không tìm thấy lớp học.");

            var instructor = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.MentorId);
            if (instructor == null)
                return (false, "Không tìm thấy giảng viên.");

            if (request.WeeklySchedules == null || request.WeeklySchedules.Count == 0)
                return (false, "Chưa có buổi học trong tuần đầu.");

            string joinUrl = await ZoomHelper.CreateRecurringMeetingAndGetJoinUrlAsync();

            var allSchedules = new List<Schedule>();

            for (int week = 0; week < request.NumberOfWeeks; week++)
            {
                var baseDate = request.StartWeek.AddDays(7 * week);

                foreach (var item in request.WeeklySchedules)
                {
                    if (!Enum.TryParse<DayOfWeek>(item.DayOfWeek, true, out var day))
                        return (false, $"Ngày '{item.DayOfWeek}' không hợp lệ.");

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
                        return (false, $"Lịch học bị trùng cho các học viên: {conflictList}...");
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
                            return (false, $"Phòng '{item.Location}' đã có lớp ({classList}) trong khung giờ này.");
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

            if (allSchedules.Any())
            {
                var startDate = allSchedules.Min(s => s.Date);
                var endDate = allSchedules.Max(s => s.Date);

                classEntity.StartDate = startDate;
                classEntity.EndDate = endDate;
            }

            _context.Schedules.AddRange(allSchedules);

            classEntity.Status = ClassConstants.Status.Scheduled;
            classEntity.IsActive = true;
            classEntity.MentorId = request.MentorId;
            _context.Classes.Update(classEntity);
            await _context.SaveChangesAsync();

            var scheduleParticipants = new List<ScheduleParticipant>();

            foreach (var schedule in allSchedules)
            {
                foreach (var student in classEntity.Employees)
                {
                    scheduleParticipants.Add(new ScheduleParticipant
                    {
                        ScheduleId = schedule.ScheduleId,
                        UserId = student.Id,
                        AttendanceDate = null
                    });
                }
            }

            if (scheduleParticipants.Count > 0)
            {
                _context.ScheduleParticipants.AddRange(scheduleParticipants);
                await _context.SaveChangesAsync();
            }

            return (true, $"Đã tạo {allSchedules.Count} buổi học cho {request.NumberOfWeeks} tuần.");
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

        public async Task<List<ClassEmployeeAttendanceDto>> GetUserByClassAsync(int classId)
        {
            var classEntity = await _context.Classes
                .Include(c => c.Employees)
                .Include(c => c.Schedules)
                .FirstOrDefaultAsync(c => c.ClassId == classId);

            if (classEntity == null)
                return new List<ClassEmployeeAttendanceDto>();

            var scheduleIds = classEntity.Schedules.Select(s => s.ScheduleId).ToList();

            var attendances = await _context.Attendances
                .Where(a => scheduleIds.Contains(a.ScheduleId))
                .ToListAsync();

            var result = classEntity.Employees.Select(e => new ClassEmployeeAttendanceDto
            {
                EmployeeId = e.EmployeeId ?? "",
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
                Employees = classEntity.Employees.Select(e => new ClassEmployeeDto
                {
                    EmployeeId = e.EmployeeId ?? "",
                    FullName = e.FullName,
                    Email = e.Email
                }).ToList(),
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

        public async Task<(bool Success, string Message)> CreateClassSwapRequestAsync(SwapClassRequest request)
        {
            if (request.EmployeeIdFrom == request.EmployeeIdTo)
                return (false, "Không thể đổi lớp với chính mình.");

            var userFrom = await _context.Users.FirstOrDefaultAsync(u => u.EmployeeId == request.EmployeeIdFrom);
            var userTo = await _context.Users.FirstOrDefaultAsync(u => u.EmployeeId == request.EmployeeIdTo);

            if (userFrom == null || userTo == null)
                return (false, "Không tìm thấy 1 hoặc cả 2 học viên.");

            var class1 = await _context.Classes
                .Include(c => c.Employees)
                .Include(c => c.Course)
                .FirstOrDefaultAsync(c => c.ClassId == request.ClassIdFrom);

            var class2 = await _context.Classes
                .Include(c => c.Employees)
                .Include(c => c.Course)
                .FirstOrDefaultAsync(c => c.ClassId == request.ClassIdTo);

            if (class1 == null || class2 == null)
                return (false, "Không tìm thấy 1 hoặc cả 2 lớp học.");

            if (class1.CourseId != class2.CourseId)
                return (false, $"Hai lớp '{class1.ClassName}' và '{class2.ClassName}' không cùng một môn học, không thể đổi.");

            if (!class1.Employees.Any(e => e.EmployeeId == userFrom.EmployeeId))
                return (false, $"{userFrom.FullName} không thuộc lớp {class1.ClassName}.");

            if (!class2.Employees.Any(e => e.EmployeeId == userTo.EmployeeId))
                return (false, $"{userTo.FullName} không thuộc lớp {class2.ClassName}.");

            var existingRequest = await _context.ClassSwaps.FirstOrDefaultAsync(x =>
            (x.RequesterId == request.EmployeeIdFrom && x.TargetId == request.EmployeeIdTo) ||
            (x.RequesterId == request.EmployeeIdTo && x.TargetId == request.EmployeeIdFrom));

            if (existingRequest != null && existingRequest.Status == ClassSwapConstants.Pending)
                return (false, "Đã có yêu cầu đổi lớp đang chờ xử lý giữa hai học viên.");

            var swapRequest = new ClassSwap
            {
                RequesterId = request.EmployeeIdFrom,
                TargetId = request.EmployeeIdTo,

                FromClassId = request.ClassIdFrom,
                ToClassId = request.ClassIdTo,

                Status = ClassSwapConstants.Pending,
                RequestedAt = DateTime.Now
            };

            _context.ClassSwaps.Add(swapRequest);
            await _context.SaveChangesAsync();

            return (true, $"Gửi yêu cầu đổi lớp thành công. Vui lòng chờ phản hồi.");
        }

        public async Task<PagedResult<ClassDto>> GetClassesAsync(int page, int pageSize)
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

        public async Task<(bool Success, string Message)> RespondToClassSwapAsync(RespondSwapRequest request, string responderId)
        {
            try
            {
                var swapRequest = await _context.ClassSwaps
                    .FirstOrDefaultAsync(r => r.Id == request.SwapRequestId);

                if (swapRequest == null)
                    return (false, "Không tìm thấy yêu cầu đổi lớp.");

                if (swapRequest.Status != ClassSwapConstants.Pending)
                    return (false, "Yêu cầu này đã được xử lý trước đó.");

                var classTo = await _context.Classes.FirstOrDefaultAsync(c => c.ClassId == swapRequest.ToClassId);
                var classFrom = await _context.Classes.FirstOrDefaultAsync(c => c.ClassId == swapRequest.FromClassId);

                if (classTo == null || classFrom == null)
                    return (false, "Không tìm thấy thông tin lớp học.");

                if (classTo.StartDate <= DateTime.Now || classFrom.StartDate <= DateTime.Now)
                {
                    swapRequest.Status = ClassSwapConstants.Cancelled;
                    await _context.SaveChangesAsync();

                    return (false, "Không thể đổi lớp vì một trong hai lớp đã bắt đầu. Yêu cầu đã bị hủy.");
                }

                if (request.Accepted)
                {
                    var userFrom = await _context.Users.Include(u => u.EnrolledClasses)
                        .FirstOrDefaultAsync(u => u.EmployeeId == swapRequest.RequesterId);
                    var userTo = await _context.Users.Include(u => u.EnrolledClasses)
                        .FirstOrDefaultAsync(u => u.EmployeeId == swapRequest.TargetId);

                    if (userFrom == null || userTo == null)
                        return (false, "Không tìm thấy thông tin học viên.");
    
                    classFrom.Employees.Remove(userFrom);
                    classTo.Employees.Remove(userTo);

                    classFrom.Employees.Add(userTo);
                    classTo.Employees.Add(userFrom);

                    swapRequest.Status = ClassSwapConstants.Approved;
                    swapRequest.RespondedById = responderId;

                    await _context.SaveChangesAsync();

                    return (true, "Đổi lớp thành công.");
                }
                else
                {
                    swapRequest.Status = ClassSwapConstants.Rejected;
                    swapRequest.RespondedById = responderId;
                    await _context.SaveChangesAsync();

                    return (true, "Từ chối yêu cầu đổi lớp thành công.");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi xử lý yêu cầu đổi lớp: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> RescheduleAsync(int scheduleId, RescheduleRequest request)
        {
            var schedule = await _context.Schedules.FirstOrDefaultAsync(sh => sh.ScheduleId == scheduleId);
            if (schedule == null)
                return (false, "Không tìm thấy lịch học.");

            if (schedule.Date < DateTime.Today)
                return (false, "Không thể đổi lịch cho buổi học đã diễn ra.");

            var instructorConflict = await _context.Schedules.AnyAsync(sh =>
                sh.InstructorId == schedule.InstructorId &&
                sh.ScheduleId != schedule.ScheduleId &&
                sh.Date == request.NewDate &&
                (
                    (request.NewStartTime >= sh.StartTime && request.NewStartTime < sh.EndTime) ||
                    (request.NewEndTime > sh.StartTime && request.NewEndTime <= sh.EndTime) ||
                    (request.NewStartTime <= sh.StartTime && request.NewEndTime >= sh.EndTime)
                )
            );

            if (instructorConflict)
                return (false, "Giảng viên đã có buổi học khác trong khoảng thời gian này.");

            if (!string.IsNullOrWhiteSpace(schedule.Location))
            {
                var roomConflict = await _context.Schedules.AnyAsync(sh =>
                    sh.Location == schedule.Location &&
                    sh.ScheduleId != schedule.ScheduleId &&
                    sh.Date == request.NewDate &&
                    (
                        (request.NewStartTime >= sh.StartTime && request.NewStartTime < sh.EndTime) ||
                        (request.NewEndTime > sh.StartTime && request.NewEndTime <= sh.EndTime) ||
                        (request.NewStartTime <= sh.StartTime && request.NewEndTime >= sh.EndTime)
                    )
                );

                if (roomConflict)
                    return (false, $"Phòng học \"{schedule.Location}\" đã có buổi khác trong khoảng thời gian này.");
            }

            var studentIds = schedule.ScheduleParticipants.Select(p => p.UserId).ToList();

            if (studentIds.Any())
            {
                var studentConflict = await _context.ScheduleParticipants
                    .Include(sp => sp.Schedule)
                    .AnyAsync(sp =>
                        studentIds.Contains(sp.UserId) &&
                        sp.ScheduleId != schedule.ScheduleId &&
                        sp.Schedule.Date == request.NewDate &&
                        (
                            (request.NewStartTime >= sp.Schedule.StartTime && request.NewStartTime < sp.Schedule.EndTime) ||
                            (request.NewEndTime > sp.Schedule.StartTime && request.NewEndTime <= sp.Schedule.EndTime) ||
                            (request.NewStartTime <= sp.Schedule.StartTime && request.NewEndTime >= sp.Schedule.EndTime)
                        )
                    );

                if (studentConflict)
                    return (false, "Một hoặc nhiều học viên đã có lịch học khác trong khoảng thời gian này.");
            }

            schedule.Date = request.NewDate;
            schedule.StartTime = request.NewStartTime;
            schedule.EndTime = request.NewEndTime;
            schedule.Status = ScheduleConstants.Status.Rescheduled;


            await _context.SaveChangesAsync();
            return (true, "Đổi lịch thành công.");
        }

        public async Task<Schedule?> GetClassScheduleByIdAsync(int scheduleId)
        {
            return await _context.Schedules
                .Include(s => s.ScheduleParticipants)
                .FirstOrDefaultAsync(sc => sc.ScheduleId == scheduleId);
        }
    }
}
