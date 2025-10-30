using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Models;

namespace InternalTrainingSystem.Core.Services.Interface
{
    public interface ICourseMaterialService
    {
        // Modules
        Task<ModuleDetailDto?> GetModuleAsync(int moduleId, CancellationToken ct = default);
        Task<IReadOnlyList<ModuleDetailDto>> GetModulesByCourseAsync(int courseId, CancellationToken ct = default);
        Task<CourseModule> CreateModuleAsync(CreateModuleDto dto, CancellationToken ct = default);
        Task<bool> UpdateModuleAsync(int moduleId, UpdateModuleDto dto, CancellationToken ct = default);
        Task<bool> DeleteModuleAsync(int moduleId, CancellationToken ct = default);

        // Lessons
        Task<LessonListItemDto?> GetLessonAsync(int lessonId, CancellationToken ct = default);
        Task<IReadOnlyList<LessonListItemDto>> GetLessonsByModuleAsync(int moduleId, CancellationToken ct = default);
        Task<Lesson> CreateLessonAsync(CreateLessonDto dto, CancellationToken ct = default);
        Task<bool> UpdateLessonAsync(int lessonId, UpdateLessonDto dto, CancellationToken ct = default);
        Task<bool> DeleteLessonAsync(int lessonId, CancellationToken ct = default);
        Task<(string url, string relativePath)> UploadLessonBinaryAsync(int lessonId, IFormFile file, CancellationToken ct = default);
        Task<(string url, string relativePath)> UploadLessonAttachmentAsync(int lessonId, IFormFile file, CancellationToken ct = default);
        Task<bool> ClearLessonFileAsync(int lessonId, CancellationToken ct = default);
        Task<Lesson> CreateQuizLessonFromExcelAsync(CreateQuizLessonRequest req, CancellationToken ct = default);

    }
}
