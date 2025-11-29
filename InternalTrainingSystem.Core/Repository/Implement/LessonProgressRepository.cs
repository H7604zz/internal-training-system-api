using DocumentFormat.OpenXml.InkML;
using InternalTrainingSystem.Core.Common.Constants;
using InternalTrainingSystem.Core.DB;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace InternalTrainingSystem.Core.Repository.Implement
{
    public class LessonProgressRepository : ILessonProgressRepository
    {
        private readonly ApplicationDbContext _db;
        public LessonProgressRepository(ApplicationDbContext db) { _db = db; }

        public Task<LessonProgress?> GetAsync(string userId, int lessonId, CancellationToken ct = default) =>
            _db.LessonProgresses.FirstOrDefaultAsync(x => x.UserId == userId && x.LessonId == lessonId, ct);

        public async Task EnsureStartedAsync(string userId, int lessonId, CancellationToken ct = default)
        {
            var lp = await GetAsync(userId, lessonId, ct);
            if (lp == null)
            {
                _db.LessonProgresses.Add(new LessonProgress
                {
                    UserId = userId,
                    LessonId = lessonId,
                    IsDone = false,
                    StartedAt = DateTime.UtcNow
                });
            }
            else if (lp.StartedAt == null)
            {
                lp.StartedAt = DateTime.UtcNow;
            }
        }

        public async Task MarkDoneAsync(string userId, int lessonId, CancellationToken ct = default)
        {
            var lp = await GetAsync(userId, lessonId, ct);
            if (lp == null)
            {
                _db.LessonProgresses.Add(new LessonProgress
                {
                    UserId = userId,
                    LessonId = lessonId,
                    IsDone = true,
                    StartedAt = DateTime.UtcNow,
                    CompletedAt = DateTime.UtcNow
                });
            }
            else
            {
                lp.IsDone = true;
                lp.CompletedAt = DateTime.UtcNow;
                if (lp.StartedAt == null) lp.StartedAt = DateTime.UtcNow;
            }
        }
        public Task<bool> IsEnrolledAsync(int courseId, string userId, CancellationToken ct = default)
        => _db.CourseEnrollments.AnyAsync(
        e => e.CourseId == courseId
          && e.UserId == userId
          && (e.Status == EnrollmentConstants.Status.Enrolled 
            || e.Status == EnrollmentConstants.Status.Completed
            || e.Status == EnrollmentConstants.Status.NotPass
            || e.Status == EnrollmentConstants.Status.InProgress),
        ct);

        public Task<Course?> GetCourseWithStructureAsync(int courseId, CancellationToken ct = default)
            => _db.Courses
                .Include(c => c.Modules.OrderBy(m => m.OrderIndex))
                    .ThenInclude(m => m.Lessons.OrderBy(l => l.OrderIndex))
                .FirstOrDefaultAsync(c => c.CourseId == courseId, ct);

        public Task<Lesson?> GetLessonWithModuleCourseAsync(int lessonId, CancellationToken ct = default)
            => _db.Lessons
                .Include(l => l.Module)
                    .ThenInclude(m => m.Course)
                .FirstOrDefaultAsync(l => l.Id == lessonId, ct);

        public Task<LessonProgress?> GetProgressAsync(string userId, int lessonId, CancellationToken ct = default)
            => _db.LessonProgresses.FirstOrDefaultAsync(p => p.UserId == userId && p.LessonId == lessonId, ct);

        public async Task UpsertDoneAsync(string userId, int lessonId, bool done, CancellationToken ct = default)
        {
            var lp = await _db.LessonProgresses.FirstOrDefaultAsync(p => p.UserId == userId && p.LessonId == lessonId, ct);
            if (lp == null)
            {
                lp = new LessonProgress
                {
                    UserId = userId,
                    LessonId = lessonId,
                    IsDone = done,
                    StartedAt = done ? DateTime.UtcNow : DateTime.UtcNow,
                    CompletedAt = done ? DateTime.UtcNow : null
                };
                _db.LessonProgresses.Add(lp);
            }
            else
            {
                if (lp.IsDone == done) return; // idempotent
                lp.IsDone = done;
                if (done)
                {
                    lp.CompletedAt ??= DateTime.UtcNow;
                }
                else
                {
                    lp.CompletedAt = null;
                }
            }
        }

        public async Task<Dictionary<int, LessonProgress>> GetProgressMapAsync(string userId, IEnumerable<int> lessonIds, CancellationToken ct = default)
        {
            var ids = lessonIds.ToList();
            var list = await _db.LessonProgresses
                .Where(p => p.UserId == userId && ids.Contains(p.LessonId))
                .ToListAsync(ct);
            return list.ToDictionary(p => p.LessonId);
        }

        public Task<int> CountCourseCompletedLessonsAsync(string userId, int courseId, CancellationToken ct = default)
            => _db.LessonProgresses
                .Where(p => p.UserId == userId && p.IsDone
                            && _db.Lessons
                                .Where(l => l.Module.CourseId == courseId)
                                .Select(l => l.Id)
                                .Contains(p.LessonId))
                .CountAsync(ct);

        public Task<int> CountCourseTotalLessonsAsync(int courseId, CancellationToken ct = default)
            => _db.Lessons.CountAsync(l => l.Module.CourseId == courseId, ct);

        public async Task<bool> HasUserPassedQuizAsync(int quizId, string userId, CancellationToken ct = default)
        {
            var attempt = await _db.QuizAttempts
                .Where(a => a.QuizId == quizId && a.UserId == userId)
                .OrderByDescending(a => a.AttemptNumber)        // attempt mới nhất
                .FirstOrDefaultAsync(ct);

            if (attempt == null)
                return false; // chưa làm bài

            // Rule pass:
            // 1. Status Completed
            // 2. IsPassed = true
            if (attempt.Status != QuizConstants.Status.Completed)
                return false;

            return attempt.IsPassed;
        }
        public Task SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
    }
}
