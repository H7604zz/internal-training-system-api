using InternalTrainingSystem.Core.DTOs;

namespace InternalTrainingSystem.Core.Services.Interface
{
	public interface ICourseHistoryService
	{
		public Task<IEnumerable<UserQuizHistoryDto>> GetUserQuizHistoryAsync(string userId, int courseId, int quizId);
	}
}
