using InternalTrainingSystem.Core.Models;
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
        public async Task<IEnumerable<CourseHistory>> GetCourseHistoriesAsync()
        {
            return await _courseHistoryRepository.GetCourseHistoriesAsync();
        }
    }
}
