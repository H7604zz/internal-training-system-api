using InternalTrainingSystem.Core.DB;
using InternalTrainingSystem.Core.DTOs;
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
        public async Task<QuizDetailDto2?> GetDetailAsync(int quizId, CancellationToken ct)
        {
            var query =
                _context.Quizzes
                   .AsNoTracking()
                   .Where(q => q.QuizId == quizId)
                   .Select(q => new QuizDetailDto2
                   {
                       QuizId = q.QuizId,
                       Title = q.Title,
                       Description = q.Description,
                       TimeLimit = q.TimeLimit,
                       MaxAttempts = q.MaxAttempts,
                       PassingScore = q.PassingScore,
                       CourseId = q.CourseId,
                       CourseName = q.Course.CourseName,
                       AttemptCount = q.QuizAttempts.Count,

                       Questions = q.Questions
                                    .OrderBy(qq => qq.OrderIndex)
                                    .Select(qq => new QuizQuestionDto
                                    {
                                        QuestionId = qq.QuestionId,
                                        QuestionText = qq.QuestionText,
                                        QuestionType = qq.QuestionType,
                                        Points = qq.Points,
                                        OrderIndex = qq.OrderIndex,
                                        Answers = qq.Answers
                                                    .OrderBy(a => a.OrderIndex)
                                                    .Select(a => new QuizAnswerDto
                                                    {
                                                        AnswerId = a.AnswerId,
                                                        AnswerText = a.AnswerText,
                                                        IsCorrect = a.IsCorrect,
                                                        OrderIndex = a.OrderIndex,
                                                    }).ToList()
                                    }).ToList()
                   });

            return await query.FirstOrDefaultAsync(ct);
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
