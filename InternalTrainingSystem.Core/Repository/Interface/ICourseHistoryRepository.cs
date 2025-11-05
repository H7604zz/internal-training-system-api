using InternalTrainingSystem.Core.DTOs;

namespace InternalTrainingSystem.Core.Repository.Interface
{
	public interface ICourseHistoryRepository
	{
		Task<IEnumerable<UserQuizHistoryDto>> GetUserQuizHistoryAsync(string userId, int courseId);
	}
}
