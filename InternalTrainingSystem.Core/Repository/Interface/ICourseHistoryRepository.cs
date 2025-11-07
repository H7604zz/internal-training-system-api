using InternalTrainingSystem.Core.DTOs;

namespace InternalTrainingSystem.Core.Repository.Interface
{
	public interface ICourseHistoryRepository
	{
		Task<IEnumerable<UserQuizHistoryResponse>> GetUserQuizHistoryAsync(string userId, int courseId, int quizId);
	}
}
