using InternalTrainingSystem.Core.Models;

namespace InternalTrainingSystem.Core.Services.Interface
{
    public interface ICourseHistoryService
    {
        Task<IEnumerable<CourseHistory>> GetCourseHistoriesAsync();
    }
}

