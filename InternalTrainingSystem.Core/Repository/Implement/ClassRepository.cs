using InternalTrainingSystem.Core.Common;
using InternalTrainingSystem.Core.Common.Constants;
using InternalTrainingSystem.Core.DB;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Helper;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Repository.Interface;
using InternalTrainingSystem.Core.Utils;
using Microsoft.EntityFrameworkCore;

namespace InternalTrainingSystem.Core.Repository.Implement;

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

        var totalUsers = shuffledUsers.Count;
        var numClasses = request.NumberOfClasses;

        var baseCount = totalUsers / numClasses;
        var remainder = totalUsers % numClasses;

        var createdClasses = new List<Class>();
        var userIndex = 0;

        for (var i = 1; i <= numClasses; i++)
        {
            var currentClassSize = baseCount + (i <= remainder ? 1 : 0);

            var baseClassName = $"Lớp {course.Code}-{i}";
            var finalClassName = baseClassName;
            var suffix = 1;

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
                CreatedById = createdById
            };

            for (var j = 0; j < currentClassSize && userIndex < totalUsers; j++)
            {
                var user = shuffledUsers[userIndex];
                var appUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
                if (appUser != null) newClass.Employees.Add(appUser);
                userIndex++;
            }

            createdClasses.Add(newClass);
        }

        _context.Classes.AddRange(createdClasses);

        var userIds = confirmedUsers.Select(u => u.Id).ToList();
        var enrollments = await _context.CourseEnrollments
            .Where(e => e.CourseId == request.CourseId && userIds.Contains(e.UserId))
            .ToListAsync();

        foreach (var e in enrollments) e.Status = EnrollmentConstants.Status.InProgress;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> CreateWeeklySchedulesAsync(CreateWeeklyScheduleRequest request)
    {
        var classEntity = await _context.Classes
            .Include(c => c.Employees)
            .FirstOrDefaultAsync(c => c.ClassId == request.ClassId);

        if (classEntity == null)
            throw new ArgumentException("Không tìm thấy lớp học.");

        var instructor = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.MentorId);
        if (instructor == null)
            throw new ArgumentException("Không tìm thấy giảng viên.");

        if (request.WeeklySchedules == null || request.WeeklySchedules.Count == 0)
            throw new ArgumentException("Chưa có buổi học trong tuần đầu.");

        var joinUrl = await ZoomHelper.CreateRecurringMeetingAndGetJoinUrlAsync();

        var allSchedules = new List<Schedule>();

        for (var week = 0; week < request.NumberOfWeeks; week++)
        {
            var baseDate = request.StartWeek.AddDays(7 * week);

            foreach (var item in request.WeeklySchedules)
            {
                if (!Enum.TryParse<DayOfWeek>(item.DayOfWeek, true, out var day))
                    throw new ArgumentException($"Ngày '{item.DayOfWeek}' không hợp lệ.");

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
                        s.Date.Date == date.Date &&
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
                    throw new ArgumentException($"Lịch học bị trùng cho các học viên: {conflictList}...");
                }

                if (!string.IsNullOrWhiteSpace(item.Location))
                {
                    var roomConflicts = await _context.Schedules
                        .Include(s => s.Class)
                        .Where(s =>
                            s.Date.Date == date.Date &&
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
                        throw new ArgumentException(
                            $"Phòng '{item.Location}' đã có lớp ({classList}) trong khung giờ này.");
                    }
                }

                allSchedules.Add(new Schedule
                {
                    DayOfWeek = item.DayOfWeek,
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
        foreach (var student in classEntity.Employees)
            scheduleParticipants.Add(new ScheduleParticipant
            {
                ScheduleId = schedule.ScheduleId,
                UserId = student.Id,
                AttendanceDate = null
            });

        if (scheduleParticipants.Count > 0)
        {
            _context.ScheduleParticipants.AddRange(scheduleParticipants);
            await _context.SaveChangesAsync();
        }

        var attendances = new List<Attendance>();

        foreach (var schedule in allSchedules)
        foreach (var staff in classEntity.Employees)
            attendances.Add(new Attendance
            {
                UserId = staff.Id,
                ScheduleId = schedule.ScheduleId,
                Status = AttendanceConstants.Status.NotYet,
                CheckInTime = DateTimeUtils.Now(),
                CreatedDate = DateTimeUtils.Now()
            });

        _context.Attendances.AddRange(attendances);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<ScheduleItemResponseDto>> GetClassScheduleAsync(int classId)
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
                Day = s.Date,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                Location = s.Location,
                OnlineLink = s.OnlineLink
            })
            .ToListAsync();

        return schedule;
    }

    public async Task<List<ScheduleItemResponseDto>> GetUserScheduleAsync(string userId)
    {
        // Lấy các lớp mà user là học viên
        var staffClasses = await _context.Classes
            .Include(c => c.Employees)
            .Where(c => c.Employees.Any(e => e.Id == userId))
            .Select(c => c.ClassId)
            .ToListAsync();

        // Lấy tất cả schedules mà user tham gia (là học viên hoặc là mentor)
        var query = _context.Schedules
            .Include(s => s.Course)
            .Include(s => s.Class)
            .Include(s => s.Instructor)
            .Where(s =>
                (s.ClassId != null && staffClasses.Contains(s.ClassId.Value))
                || s.InstructorId == userId
            )
            .OrderBy(s => s.Date)
            .ThenBy(s => s.StartTime);

        var schedules = await query.ToListAsync();

        // Lấy attendance status chỉ cho các schedule mà user là học viên (không phải mentor)
        var scheduleIds = schedules.Select(s => s.ScheduleId).ToList();
        var attendances = await _context.Attendances
            .Where(a => a.UserId == userId && scheduleIds.Contains(a.ScheduleId))
            .ToDictionaryAsync(a => a.ScheduleId, a => a.Status);

        var result = schedules.Select(s => new ScheduleItemResponseDto
        {
            ScheduleId = s.ScheduleId,
            ClassId = s.ClassId,
            ClassName = s.Class!.ClassName,
            MentorId = s.InstructorId,
            Mentor = s.Instructor.FullName,
            CourseCode = s.Course.Code!,
            CourseName = s.Course.CourseName!,
            Day = s.Date,
            StartTime = s.StartTime,
            EndTime = s.EndTime,
            Location = s.Location,
            OnlineLink = s.OnlineLink,
            // Nếu user là mentor của schedule này, không hiển thị attendance status
            // Chỉ hiển thị attendance status nếu user là học viên
            AttendanceStatus = s.InstructorId == userId
                ? null
                : attendances.ContainsKey(s.ScheduleId)
                    ? attendances[s.ScheduleId]
                    : AttendanceConstants.Status.NotYet
        }).ToList();

        return result;
    }

    public async Task<ClassDto?> GetClassDetailAsync(int classId)
    {
        var classEntity = await _context.Classes
            .Include(c => c.Course)
            .Include(c => c.Mentor)
            .Include(c => c.Employees)
            .Include(c => c.Schedules)
            .FirstOrDefaultAsync(c => c.ClassId == classId);

        if (classEntity == null)
            return null;

        var scheduleIds = classEntity.Schedules.Select(s => s.ScheduleId).ToList();

        var attendances = await _context.Attendances
            .Where(a => scheduleIds.Contains(a.ScheduleId))
            .ToListAsync();

        var enrollments = await _context.CourseEnrollments
            .Where(e => e.CourseId == classEntity.CourseId)
            .ToListAsync();

        return new ClassDto
        {
            ClassId = classEntity.ClassId,
            ClassName = classEntity.ClassName,
            CourseId = classEntity.CourseId,
            CourseName = classEntity.Course?.CourseName,
            MentorId = classEntity.Mentor?.EmployeeId,
            MentorName = classEntity.Mentor?.FullName,
            MaxStudents = classEntity.Employees.Count,

            Employees = classEntity.Employees.Select(e =>
            {
                var enrollment = enrollments.FirstOrDefault(x => x.UserId == e.Id);

                return new ClassEmployeeDto
                {
                    EmployeeId = e.EmployeeId ?? "",
                    FullName = e.FullName,
                    Email = e.Email,

                    AbsentNumberDay = attendances.Count(a =>
                        a.UserId == e.Id &&
                        a.Status == AttendanceConstants.Status.Absent),

                    ScoreFinal = enrollment?.Score
                };
            }).ToList(),

            IsActive = classEntity.IsActive,
            Status = classEntity.Status,
            CreatedDate = classEntity.CreatedDate
        };
    }

    public async Task<List<ClassListDto>> GetClassesByCourseAsync(int courseId)
    {
        var classes = await _context.Classes
            .Include(c => c.Mentor)
            .Include(c => c.Employees)
            .Where(c => c.CourseId == courseId)
            .ToListAsync();

        return classes.Select(c => new ClassListDto
        {
            ClassId = c.ClassId,
            ClassName = c.ClassName,
            MentorId = c.MentorId!,
            MentorName = c.Mentor?.FullName ?? "N/A",
            IsActive = c.IsActive,
            Status = c.Status
        }).ToList();
    }

    public async Task<bool> CreateClassSwapRequestAsync(SwapClassRequest request)
    {
        if (request.EmployeeIdFrom == request.EmployeeIdTo)
            return false;

        var userFrom = await _context.Users.FirstOrDefaultAsync(u => u.EmployeeId == request.EmployeeIdFrom);
        var userTo = await _context.Users.FirstOrDefaultAsync(u => u.EmployeeId == request.EmployeeIdTo);

        if (userFrom == null || userTo == null)
            throw new ArgumentException("Không tìm thấy 1 hoặc cả 2 học viên.");

        var class1 = await _context.Classes
            .Include(c => c.Employees)
            .Include(c => c.Course)
            .FirstOrDefaultAsync(c => c.ClassId == request.ClassIdFrom);

        var class2 = await _context.Classes
            .Include(c => c.Employees)
            .Include(c => c.Course)
            .FirstOrDefaultAsync(c => c.ClassId == request.ClassIdTo);

        if (class1 == null || class2 == null)
            return false;
        
        if (class1.StartDate.Date <= DateTime.Now.Date || class2.StartDate.Date <= DateTime.Now.Date)
        {
            throw new ArgumentException("Không thể đổi lớp vì một trong hai lớp đã bắt đầu. Yêu cầu đã bị hủy.");
        }

        if (class1.CourseId != class2.CourseId)
            throw new ArgumentException(
                $"Hai lớp '{class1.ClassName}' và '{class2.ClassName}' không cùng một môn học, không thể đổi.");

        if (!class1.Employees.Any(e => e.EmployeeId == userFrom.EmployeeId))
            throw new ArgumentException($"{userFrom.FullName} không thuộc lớp {class1.ClassName}.");

        if (!class2.Employees.Any(e => e.EmployeeId == userTo.EmployeeId))
            throw new ArgumentException($"{userTo.FullName} không thuộc lớp {class2.ClassName}.");

        var existingRequest = await _context.ClassSwaps.FirstOrDefaultAsync(x =>
            (x.RequesterId == userFrom.Id && x.TargetId == userTo.Id) ||
            (x.RequesterId == userTo.Id && x.TargetId == userFrom.Id));

        if (existingRequest != null && existingRequest.Status == ClassSwapConstants.Pending)
            throw new ArgumentException("Đã có yêu cầu đổi lớp đang chờ xử lý giữa hai học viên.");

        var swapRequest = new ClassSwap
        {
            RequesterId = userFrom.Id,
            TargetId = userTo.Id,

            FromClassId = request.ClassIdFrom,
            ToClassId = request.ClassIdTo,

            Status = ClassSwapConstants.Pending,
            RequestedAt = DateTime.Now
        };

        _context.ClassSwaps.Add(swapRequest);
        await _context.SaveChangesAsync();

        return true;
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

    public async Task<bool> RespondToClassSwapAsync(RespondSwapRequest request, string responderId)
    {
        try
        {
            var swapRequest = await _context.ClassSwaps
                .FirstOrDefaultAsync(r => r.Id == request.SwapRequestId);

            if (swapRequest == null)
                return false;

            if (swapRequest.Status != ClassSwapConstants.Pending)
                return false;

            var classTo = await _context.Classes.FirstOrDefaultAsync(c => c.ClassId == swapRequest.ToClassId);
            var classFrom = await _context.Classes.FirstOrDefaultAsync(c => c.ClassId == swapRequest.FromClassId);

            if (classTo == null || classFrom == null)
                return false;

            if (classTo.StartDate.Date <= DateTime.Now.Date || classFrom.StartDate.Date <= DateTime.Now.Date)
            {
                swapRequest.Status = ClassSwapConstants.Cancelled;
                await _context.SaveChangesAsync();

                throw new ArgumentException("Không thể đổi lớp vì một trong hai lớp đã bắt đầu. Yêu cầu đã bị hủy.");
            }

            if (request.Accepted)
            {
                var userFrom = await _context.Users.Include(u => u.EnrolledClasses)
                    .FirstOrDefaultAsync(u => u.EmployeeId == swapRequest.RequesterId);
                var userTo = await _context.Users.Include(u => u.EnrolledClasses)
                    .FirstOrDefaultAsync(u => u.EmployeeId == swapRequest.TargetId);

                if (userFrom == null || userTo == null)
                    return false;

                classFrom.Employees.Remove(userFrom);
                classTo.Employees.Remove(userTo);

                classFrom.Employees.Add(userTo);
                classTo.Employees.Add(userFrom);

                swapRequest.Status = ClassSwapConstants.Approved;
                swapRequest.RespondedById = responderId;

                await _context.SaveChangesAsync();

                return true;
            }

            swapRequest.Status = ClassSwapConstants.Rejected;
            swapRequest.RespondedById = responderId;
            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"Lỗi khi xử lý yêu cầu đổi lớp: {ex.Message}");
        }
    }

    public async Task<bool> RescheduleAsync(int scheduleId, RescheduleRequest request)
    {
        var schedule = await _context.Schedules
            .Include(s => s.ScheduleParticipants)
            .Include(s => s.Class)
            .FirstOrDefaultAsync(sh => sh.ScheduleId == scheduleId);

        if (schedule == null)
            return false;

        if (schedule.Date.Date < DateTime.Today.Date)
            return false;

        if (request.NewDate.Date < schedule.Class.StartDate.Date)
            return false;

        var instructorConflict = await _context.Schedules.AnyAsync(sh =>
            sh.InstructorId == schedule.InstructorId &&
            sh.ScheduleId != schedule.ScheduleId &&
            sh.Date.Date == request.NewDate.Date &&
            (
                (request.NewStartTime >= sh.StartTime && request.NewStartTime < sh.EndTime) ||
                (request.NewEndTime > sh.StartTime && request.NewEndTime <= sh.EndTime) ||
                (request.NewStartTime <= sh.StartTime && request.NewEndTime >= sh.EndTime)
            )
        );

        if (instructorConflict)
            throw new ArgumentException("Giảng viên đã có buổi học khác trong khoảng thời gian này.");

        if (!string.IsNullOrWhiteSpace(request.NewLocation))
        {
            var roomConflict = await _context.Schedules.AnyAsync(sh =>
                sh.Location == request.NewLocation &&
                sh.ScheduleId != schedule.ScheduleId &&
                sh.Date.Date == request.NewDate.Date &&
                (
                    (request.NewStartTime >= sh.StartTime && request.NewStartTime < sh.EndTime) ||
                    (request.NewEndTime > sh.StartTime && request.NewEndTime <= sh.EndTime) ||
                    (request.NewStartTime <= sh.StartTime && request.NewEndTime >= sh.EndTime)
                )
            );

            if (roomConflict)
                throw new ArgumentException(
                    $"Phòng học \"{request.NewLocation}\" đã có buổi khác trong khoảng thời gian này.");
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
                throw new ArgumentException("Một hoặc nhiều học viên đã có lịch học khác trong khoảng thời gian này.");
        }

        schedule.DayOfWeek = request.NewDate.DayOfWeek.ToString();
        schedule.Date = request.NewDate;
        schedule.StartTime = request.NewStartTime;
        schedule.EndTime = request.NewEndTime;
        schedule.Location = request.NewLocation;
        schedule.Status = ScheduleConstants.Status.Rescheduled;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Schedule?> GetClassScheduleByIdAsync(int scheduleId)
    {
        return await _context.Schedules
            .Include(s => s.ScheduleParticipants)
            .FirstOrDefaultAsync(sc => sc.ScheduleId == scheduleId);
    }

    public async Task<List<ClassSwapDto>> GetSwapClassRequestAsync(string userId, int classSwapId)
    {
        return await _context.ClassSwaps
            .Include(cs => cs.Requester)
            .Include(cs => cs.Target)
            .Include(cs => cs.FromClass)
            .Include(cs => cs.ToClass)
            .Include(cs => cs.RespondedBy)
            .Where(cs => cs.TargetId == userId && cs.Id == classSwapId)
            .Select(cs => new ClassSwapDto
            {
                RequesterName = cs.Requester.FullName,
                TargetName = cs.Target.UserName!,
                FromClassName = cs.FromClass.ClassName,
                ToClassName = cs.ToClass.ClassName,
                RequestedAt = cs.RequestedAt
            })
            .ToListAsync();
    }

    public async Task<bool> UpdateScoresAsync(string mentorId, ScoreFinalRequest request)
    {
        if (request.UserScore == null || !request.UserScore.Any())
            throw new ArgumentException("Danh sách sinh viên trống.");

        var classEntity = await _context.Classes
            .Include(c => c.Employees)
            .Include(c => c.Schedules)
            .Include(c => c.Course)
            .FirstOrDefaultAsync(c => c.ClassId == request.ClassId);

        if (classEntity == null)
            return false;

        if (classEntity.MentorId != mentorId)
            return false;

        var inProgressEnrollments = await _context.CourseEnrollments
            .Where(e => e.CourseId == classEntity.CourseId
                        && e.Status == EnrollmentConstants.Status.InProgress
                        && classEntity.Employees.Any(emp => emp.Id == e.UserId))
            .ToListAsync();

        if (!inProgressEnrollments.Any())
            return false;

        foreach (var employee in request.UserScore)
        {
            var enrollment = await _context.CourseEnrollments
                .FirstOrDefaultAsync(e => e.CourseId == classEntity.CourseId
                                          && e.UserId == employee.UserId
                                          && e.Status == EnrollmentConstants.Status.InProgress);
            if (enrollment != null)
                try
                {
                    enrollment.Score = employee.Score;

                    if (request.IsSubmitted)
                    {
                        var isPass = enrollment.Score.HasValue &&
                                     enrollment.Score.Value >= classEntity.Course.PassScore;

                        enrollment.Status = isPass
                            ? EnrollmentConstants.Status.Completed
                            : EnrollmentConstants.Status.NotPass;

                        if (isPass) enrollment.CompletionDate = DateTime.Now;
                    }
                }
                catch (Exception)
                {
                }
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public Task<bool> IsMentorOfClassAsync(int classId, string mentorId, CancellationToken ct = default)
    {
        return _context.Classes.AnyAsync(
            c => c.ClassId == classId
                 && c.MentorId == mentorId
                 && c.IsActive,
            ct);
    }

    public Task<bool> IsInClassAsync(int classId, string userId, CancellationToken ct = default)
    {
        return _context.Classes
            .Where(c => c.ClassId == classId)
            .AnyAsync(c => c.Employees.Any(e => e.Id == userId), ct);
    }
    public async Task<List<Class>> GetClassesForMentorAsync(string userId, CancellationToken ct)
    {
        return await _context.Classes
            .Where(c => c.MentorId == userId)
            .ToListAsync(ct);
    }

    public async Task<List<Class>> GetClassesForStaffAsync(string userId, CancellationToken ct)
    {
        return await _context.Classes
            .Where(c => c.Employees.Any(e => e.Id == userId))
            .ToListAsync(ct);
    }

    public async Task<List<MyClassDto>> GetClassesOfUserAsync(string userId, CancellationToken ct)
    {
        // Get user's EmployeeId
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        var employeeId = user?.EmployeeId ?? string.Empty;

        var classes = await _context.Classes
            .Include(c => c.Course)
            .Include(c => c.Mentor)
            .Where(c => c.MentorId == userId || c.Employees.Any(e => e.Id == userId))
            .ToListAsync(ct);

        return classes.Select(c => new MyClassDto
        {
            ClassId = c.ClassId,
            ClassName = c.ClassName,
            CourseId = c.CourseId,
            CourseName = c.Course.CourseName,
            Mentor = c.Mentor != null ? c.Mentor.FullName : null,
            Status = c.Status ?? string.Empty,
            StartDate = c.StartDate,
            EmployeeId = employeeId
        }).ToList();
    }
}