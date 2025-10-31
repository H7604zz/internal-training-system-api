using InternalTrainingSystem.Core.Models;

namespace InternalTrainingSystem.Core.Repository.Interface
{
    public interface ILessonProgressRepository
    {
        Task<LessonProgress?> GetAsync(string userId, int lessonId, CancellationToken ct = default);
        Task EnsureStartedAsync(string userId, int lessonId, CancellationToken ct = default);
        Task MarkDoneAsync(string userId, int lessonId, CancellationToken ct = default);
    }
}
