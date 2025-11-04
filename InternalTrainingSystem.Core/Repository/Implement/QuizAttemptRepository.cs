using InternalTrainingSystem.Core.DB;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace InternalTrainingSystem.Core.Repository.Implement
{
    public class QuizAttemptRepository : IQuizAttemptRepository
    {
        private readonly ApplicationDbContext _db;
        public QuizAttemptRepository(ApplicationDbContext db) { _db = db; }

        public Task<int> CountAttemptsAsync(int quizId, string userId, CancellationToken ct = default)
        {
            return _db.QuizAttempts.CountAsync(a => a.QuizId == quizId && a.UserId == userId, ct);
        }

        public async Task<QuizAttempt> AddAttemptAsync(QuizAttempt attempt, CancellationToken ct = default)
        {
            _db.QuizAttempts.Add(attempt);
            await _db.SaveChangesAsync(ct);
            return attempt;
        }

        public Task<QuizAttempt?> GetAttemptAsync(int attemptId, string userId, CancellationToken ct = default)
        {
            return _db.QuizAttempts
                .Include(a => a.Quiz)
                .FirstOrDefaultAsync(a => a.AttemptId == attemptId && a.UserId == userId, ct);
        }

        public async Task<(IReadOnlyList<QuizAttempt> items, int total)> GetAttemptHistoryAsync(
            int quizId, string userId, int page, int pageSize, CancellationToken ct = default)
        {
            var q = _db.QuizAttempts.AsNoTracking()
                .Where(a => a.QuizId == quizId && a.UserId == userId)
                .OrderByDescending(a => a.AttemptNumber);

            var total = await q.CountAsync(ct);
            var items = await q.Skip((page - 1) * pageSize)
                               .Take(pageSize)
                               .ToListAsync(ct);

            return (items, total);
        }
    }
}
