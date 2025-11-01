using InternalTrainingSystem.Core.DB;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Repository.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace InternalTrainingSystem.Core.Repository.Implement
{
    public class TrackProgressRepository : ITrackProgressRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IQuizRepository _quizRepo;

        public TrackProgressRepository(UserManager<ApplicationUser> userManager, ApplicationDbContext context, IQuizRepository quizRepo)
        {
            _context = context;
            _userManager = userManager;
            _quizRepo = quizRepo;
        }
        /// <summary>
        /// Kiểm tra lesson đã hoàn thành chưa:
        /// - Nếu có quiz: quiz phải pass (>=80)
        /// - Nếu không có quiz: IsDone = true
        /// </summary>
        public async Task<bool> CheckLessonPassedAsync(int lessonId, CancellationToken ct = default)
        {
            var lesson = await _context.Lessons
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == lessonId, ct);

            if (lesson == null)
                throw new InvalidOperationException("Lesson not found.");

            var progress = await _context.LessonProgresses
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.LessonId == lessonId, ct);

            bool isDone = progress?.IsDone ?? false;

            if (lesson.QuizId == null)
                return isDone;

            bool quizPassed = await _quizRepo.CheckQuizPassedAsync(lesson.QuizId.Value);

            return quizPassed && isDone;
        }

        /// <summary>
        public async Task<decimal> UpdateModuleProgressAsync(string userId, int moduleId, CancellationToken ct = default)
        {
            var lessonIds = await _context.Lessons
                            .AsNoTracking()
                            .Where(l => l.ModuleId == moduleId)
                            .Select(l => l.Id)
                            .ToListAsync(ct);

            var totalLessons = lessonIds.Count;
            if (totalLessons == 0) return 0m;

            var doneLessons = 0;
            foreach (var lessonId in lessonIds)
            {
                var progress = await _context.LessonProgresses.AsNoTracking()
                                       .FirstOrDefaultAsync(p => p.LessonId == lessonId && p.UserId == userId, ct);

                if (progress != null && progress.IsDone)
                    doneLessons++;
            }

            var percent = Math.Round(100m * doneLessons / totalLessons, 2);
            return percent;
        }

        public async Task<decimal> UpdateCourseProgressAsync(string userId,int courseId, CancellationToken ct = default)
        {
            var modules = await _context.CourseModules
                .AsNoTracking()
                .Where(m => m.CourseId == courseId)
                .Select(m => m.Id)
                .ToListAsync(ct);

            var totalModules = modules.Count;
            if (totalModules == 0) return 0m;

            var doneModeles = 0;
            foreach (var moduleID in modules)
            {
                if (await UpdateModuleProgressAsync(userId,moduleID, ct)==100m)
                    doneModeles++;
            }
            var percent = Math.Round(100m * doneModeles / totalModules, 2);
            // 🔗 Cập nhật sang bảng CourseEnrollments
            var enrollment = await _context.CourseEnrollments
                .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId, ct);

            if (enrollment != null)
            {
                enrollment.Progress = (int)percent;
                enrollment.LastAccessedDate = DateTime.Now;
                await _context.SaveChangesAsync(ct);
            }
            return percent;
        }

        //theo doi cả phòng ban
        public async Task<DepartmentDetailDto> TrackProgressDepartment(int departmentId)
        {
            // 1) Lấy department + users + courses
            var department = await _context.Departments
                .AsNoTracking()
                .Include(d => d.Courses)
                .Include(d => d.Users)
                .FirstOrDefaultAsync(d => d.Id == departmentId);

            if (department == null)
                throw new KeyNotFoundException("Không tìm thấy phòng ban.");

            // 2) Chuẩn bị danh sách courseId thuộc phòng ban
            var courseIds = department.Courses?.Select(c => c.CourseId).ToList() ?? new List<int>();
            var hasCourses = courseIds.Count > 0;

            // 3) Tính Avg Progress cho từng user (trên các khóa của phòng ban)
            //    Dùng một query LINQ duy nhất để database tính trung bình.
            var usersWithAvgProgress = await _context.Users
                .AsNoTracking()
                .Where(u => u.DepartmentId == departmentId)
                .Select(u => new
                {
                    u.Id,
                    u.EmployeeId,
                    u.FullName,
                    u.Email,
                    u.Position,
                    u.IsActive,
                    AvgProgress = hasCourses
                        ? ((from e in _context.CourseEnrollments
                            where e.UserId == u.Id && courseIds.Contains(e.CourseId)
                            select (int?)e.Progress).Average() ?? 0)
                        : 0
                })
                .OrderBy(x => x.FullName)
                .ToListAsync();

            // 4) Map sang DTO
            var dto = new DepartmentDetailDto
            {
                DepartmentId = department.Id,
                DepartmentName = department.Name,
                Description = department.Description,

                CourseDetail = department.Courses?
                    .OrderBy(c => c.CourseName)
                    .Select(c => new CourseDetailDto
                    {
                        CourseId = c.CourseId,
                        Code = c.Code,
                        CourseName = c.CourseName,
                        Description = c.Description,
                        IsOnline = c.IsOnline,
                        IsMandatory = c.IsMandatory,
                    })
                    .ToList(),

                userDetail = usersWithAvgProgress
                    .Select(u => new UserProfileDto
                    {
                        Id = u.Id,
                        EmployeeId = u.EmployeeId,
                        FullName = u.FullName,
                        Email = u.Email ?? string.Empty,
                        Department = department.Name,     // tránh u.Department.Name
                        Position = u.Position,
                        IsActive = u.IsActive,
                        ProgressPercent = Math.Round((decimal)u.AvgProgress, 2) // 0–100
                    })
                    .ToList()
            };

            return dto;
        }

    }
}
