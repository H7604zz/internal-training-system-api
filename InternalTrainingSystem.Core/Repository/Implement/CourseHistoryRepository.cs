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

		public async Task<IEnumerable<UserQuizHistoryDto>> GetUserQuizHistoryAsync(string userId, int courseId)
		{
			return await _context.CourseHistories
				.Include(x => x.QuizAttempt)
				.Where(h => h.UserId == userId && h.CourseId==courseId &&
				(h.Action == CourseAction.QuizCompleted || h.Action == CourseAction.QuizPassed 
				|| h.Action == CourseAction.QuizFailed))
				.Select(h => new UserQuizHistoryDto
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
	}
}
