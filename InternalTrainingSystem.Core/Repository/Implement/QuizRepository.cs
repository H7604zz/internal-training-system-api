using InternalTrainingSystem.Core.Constants;
using InternalTrainingSystem.Core.DB;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Repository.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace InternalTrainingSystem.Core.Repository.Implement
{
    public class QuizRepository: IQuizRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public QuizRepository(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _context = context;
            _userManager = userManager;
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
