using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Repository.Interface;
using InternalTrainingSystem.Core.Services.Interface;

namespace InternalTrainingSystem.Core.Services.Implement
{
	public class CourseHistoryService : ICourseHistoryService
	{
		private readonly ICourseHistoryRepository _courseHistoryRepository;

		public CourseHistoryService(ICourseHistoryRepository courseHistoryRepository)
		{
			_courseHistoryRepository = courseHistoryRepository;
		}

		public Task<IEnumerable<UserQuizHistoryDto>> GetUserQuizHistoryAsync(string userId, int courseId)
		{
			return _courseHistoryRepository.GetUserQuizHistoryAsync(userId, courseId);
		}
	}
}
