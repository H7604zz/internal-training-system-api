using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Models;

namespace InternalTrainingSystem.Core.Repository.Interface
{
    public interface ICourseMaterialRepository
    {
        // Lessons
        Task<(string url, string relativePath)> UploadLessonBinaryAsync(int lessonId, IFormFile file, CancellationToken ct = default);
        Task<Lesson?> GetWithModuleAsync(int lessonId, CancellationToken ct = default);
        Task<Lesson?> GetByIdAsync(int lessonId, CancellationToken ct = default);
    }
}
