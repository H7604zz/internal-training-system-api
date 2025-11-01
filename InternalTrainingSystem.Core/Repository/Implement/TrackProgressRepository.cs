using InternalTrainingSystem.Core.DB;
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
        public async Task<decimal> UpdateModuleProgressAsync(int moduleId, CancellationToken ct = default)
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
                if (!await CheckLessonPassedAsync(lessonId, ct))
                {
                    doneLessons++;
                }
            }

            var percent = Math.Round(100m * doneLessons / totalLessons, 2);
            return percent;
        }

        public async Task<decimal> UpdateCourseProgressAsync(int courseId, CancellationToken ct = default)
        {
            var modules = await _context.CourseModules
                .AsNoTracking()
                .Where(m => m.CourseId == courseId)
                .Select(m => m.Id)
                .ToListAsync(ct);

            var totalModules = modules.Count;

            var doneModeles = 0;
            foreach (var moduleID in modules)
            {
                if (await UpdateModuleProgressAsync(moduleID, ct)==100m)
                    doneModeles++;
            }
            var percent = Math.Round(100m * doneModeles / totalModules, 2);
            return percent;
        }
    }
}
