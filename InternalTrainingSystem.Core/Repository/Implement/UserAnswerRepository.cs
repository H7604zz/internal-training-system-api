using InternalTrainingSystem.Core.DB;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace InternalTrainingSystem.Core.Repository.Implement
{
    public class UserAnswerRepository : IUserAnswerRepository
    {
        private readonly ApplicationDbContext _db;
        public UserAnswerRepository(ApplicationDbContext db) { _db = db; }

        public async Task AddAsync(UserAnswer entity, CancellationToken ct = default)
        {
            await _db.UserAnswers.AddAsync(entity, ct);
        }

        public async Task AddRangeAsync(IEnumerable<UserAnswer> entities, CancellationToken ct = default)
        {
            await _db.UserAnswers.AddRangeAsync(entities, ct);
        }

        public async Task<IReadOnlyList<UserAnswer>> GetByAttemptAsync(int attemptId, CancellationToken ct = default)
        {
            return await _db.UserAnswers
                .Where(ua => ua.AttemptId == attemptId)
                .ToListAsync(ct);
        }
    }
}
