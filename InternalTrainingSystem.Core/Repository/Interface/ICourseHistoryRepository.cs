using InternalTrainingSystem.Core.Models;

namespace InternalTrainingSystem.Core.Repository.Interface
{
    public interface ICourseHistoryRepository
    {
        Task<IEnumerable<CourseHistory>> GetCourseHistoriesAsync();
    }
}
