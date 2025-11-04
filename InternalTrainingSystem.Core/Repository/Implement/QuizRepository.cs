using InternalTrainingSystem.Core.DB;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Repository.Interface;
using Microsoft.AspNetCore.Identity;

using Microsoft.EntityFrameworkCore;

namespace InternalTrainingSystem.Core.Repository.Implement
{
    public class QuizRepository : IQuizRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public QuizRepository(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<Quiz?> GetActiveQuizWithQuestionsAsync(int quizId, CancellationToken ct = default)
        {
            return await _context.Quizzes
                .AsNoTracking()
                .Include(q => q.Questions.Where(x => x.IsActive))
                    .ThenInclude(q => q.Answers.Where(a => a.IsActive))
                .FirstOrDefaultAsync(q => q.QuizId == quizId && q.IsActive, ct);
        }

        public async Task<int> GetQuizMaxScoreAsync(int quizId, CancellationToken ct = default)
        {
            return await _context.Questions
                .Where(x => x.QuizId == quizId && x.IsActive)
                .SumAsync(x => (int?)x.Points, ct) ?? 0;
        }
        
        public async Task<bool> CheckQuizPassedAsync(int quizId)
        {
            var quiz = await _context.Quizzes
                .AsNoTracking()
                .FirstOrDefaultAsync(q => q.QuizId == quizId);

            var attempt = await _context.QuizAttempts
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.QuizId == quizId);

            if (attempt == null)
                throw new InvalidOperationException("Quiz attempt not found.");

            if (attempt.Score == 0)
                return false;

            else if(attempt.Score >= quiz?.PassingScore)
                return true;
            else
                return false;
        }
    }
}
