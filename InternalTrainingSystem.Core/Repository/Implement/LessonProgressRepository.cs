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
    }
}
