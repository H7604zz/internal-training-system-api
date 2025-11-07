using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Models;
  
namespace InternalTrainingSystem.Core.Repository.Interface
{
	public interface ICourseHistoryRepository
	{
    Task<IEnumerable<CourseHistory>> GetCourseHistoriesAsync();
		Task<IEnumerable<UserQuizHistoryResponse>> GetUserQuizHistoryAsync(string userId, int courseId, int quizId);
	}
}
