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
            return attempt;
        }

        public Task<QuizAttempt?> GetAttemptAsync(int attemptId, string userId, CancellationToken ct = default)
        {
            return _db.QuizAttempts
                .Include(a => a.Quiz)
                .FirstOrDefaultAsync(a => a.AttemptId == attemptId && a.UserId == userId, ct);
        }

        public async Task<List<QuizAttempt>> GetUserAttemptsAsync(int quizId, string userId, CancellationToken ct = default)
        {
            return await _db.QuizAttempts
                .AsNoTracking()
                .Where(a => a.QuizId == quizId && a.UserId == userId)
                .ToListAsync(ct);
        }

        public async Task UpdateStatusAsync(int attemptId, string status, CancellationToken ct = default)
        {
            await _db.QuizAttempts
                .Where(a => a.AttemptId == attemptId)
                .ExecuteUpdateAsync(setters => setters.SetProperty(a => a.Status, status), ct);
        }
        public Task<int> CountAttemptsTodayAsync(int quizId,string userId,DateTime from,DateTime to,CancellationToken ct)
        {
            return _db.QuizAttempts
                .Where(x => x.QuizId == quizId
                            && x.UserId == userId
                            && x.StartTime >= from
                            && x.StartTime < to)
                .CountAsync(ct);
        }
    }
}
