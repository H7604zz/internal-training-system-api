using InternalTrainingSystem.Core.DB;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace InternalTrainingSystem.Core.Repository.Implement
{
    public class QuizRepository : IQuizRepository
    {
        private readonly ApplicationDbContext _db;
        public QuizRepository(ApplicationDbContext db) { _db = db; }

        public async Task<Quiz?> GetActiveQuizWithQuestionsAsync(int quizId, CancellationToken ct = default)
        {
            return await _db.Quizzes
                .AsNoTracking()
                .Include(q => q.Questions.Where(x => x.IsActive))
                    .ThenInclude(q => q.Answers.Where(a => a.IsActive))
                .FirstOrDefaultAsync(q => q.QuizId == quizId && q.IsActive, ct);
        }

        public async Task<int> GetQuizMaxScoreAsync(int quizId, CancellationToken ct = default)
        {
            return await _db.Questions
                .Where(x => x.QuizId == quizId && x.IsActive)
                .SumAsync(x => (int?)x.Points, ct) ?? 0;
        }
    }
}
