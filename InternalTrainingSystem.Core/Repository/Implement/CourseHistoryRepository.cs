using InternalTrainingSystem.Core.Common.Constants;
using InternalTrainingSystem.Core.DB;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace InternalTrainingSystem.Core.Repository.Implement
{
    public class CourseHistoryRepository : ICourseHistoryRepository
    {
        private readonly ApplicationDbContext _context;

        public CourseHistoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<CourseHistoryDto>> GetCourseHistoriesByIdAsync(int Id)
        {
            var courseHistory = await _context.CourseHistories
                .Include(h => h.User)
                .Where(m => m.CourseId == Id)
                .OrderByDescending(h => h.ActionDate)
                .ToListAsync();

            var result = courseHistory.Select(h => new CourseHistoryDto
            {
                FullName = h.User?.FullName,
                ActionName = h.Action.ToString(),
                Description = h.Description,
                ActionDate = h.ActionDate
            }).ToList();

            return result;
        }

        public async Task<IEnumerable<UserQuizHistoryResponse>> GetUserQuizHistoryAsync(string userId, int courseId, int quizId)
        {
            return await _context.QuizAttempts
                .Where(qa => qa.UserId == userId && qa.Quiz.CourseId == courseId && qa.QuizId == quizId )
                .Select(qa => new UserQuizHistoryResponse
                {
                    QuizId = qa.QuizId,
                    Action = qa.IsPassed ? CourseAction.QuizPassed : CourseAction.QuizFailed,
                    Score = qa.Score,
                    StartTime = qa.StartTime,
                    SubmissionTime = qa.EndTime,
                    IsPassed = qa.IsPassed
                })
                .OrderByDescending(x => x.SubmissionTime)
                .ToListAsync();
        }
        public Task AddHistoryAsync(CourseHistory history, CancellationToken ct = default)
        {
            _context.CourseHistories.Add(history);
            return Task.CompletedTask;
        }

    }
}
