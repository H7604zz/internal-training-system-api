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
    
    public async Task<IEnumerable<CourseHistory>> GetCourseHistoriesByIdAsync(int Id)
        {
            return await _context.CourseHistories
                .Include(h => h.User) 
				.Where(m=>m.CourseId==Id)
                .OrderByDescending(h => h.ActionDate)
                .ToListAsync();
        }

		public async Task<IEnumerable<UserQuizHistoryResponse>> GetUserQuizHistoryAsync(string userId, int courseId, int quizId)
		{
			return await _context.CourseHistories
				.Include(x => x.QuizAttempt)
				.Where(h => h.UserId == userId && h.CourseId==courseId && h.QuizId == quizId &&
				(h.Action == CourseAction.QuizCompleted || h.Action == CourseAction.QuizPassed 
				|| h.Action == CourseAction.QuizFailed))
				.Select(h => new UserQuizHistoryResponse
				{
					QuizId = h.QuizId ?? 0,
					Action = h.Action,
					Score = h.QuizAttempt != null ? h.QuizAttempt.Score : 0,
					StartTime = h.QuizAttempt.StartTime,
					SubmissionTime = h.QuizAttempt.EndTime
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
