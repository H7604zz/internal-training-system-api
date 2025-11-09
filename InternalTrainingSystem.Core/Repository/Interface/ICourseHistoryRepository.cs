using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Models;

namespace InternalTrainingSystem.Core.Repository.Interface
{
	public interface ICourseHistoryRepository
	{
        Task<IEnumerable<CourseHistory>> GetCourseHistoriesByIdAsync(int Id);
		Task<IEnumerable<UserQuizHistoryResponse>> GetUserQuizHistoryAsync(string userId, int courseId, int quizId);
        Task AddHistoryAsync(CourseHistory history, CancellationToken ct = default);
    }
}
