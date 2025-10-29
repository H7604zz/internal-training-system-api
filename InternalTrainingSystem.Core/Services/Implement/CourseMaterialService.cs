using ClosedXML.Excel;
using Humanizer;
using InternalTrainingSystem.Core.Constants;
using InternalTrainingSystem.Core.DB;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Repository.Interface;
using InternalTrainingSystem.Core.Services.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client.Extensions.Msal;
using System.Reflection;

namespace InternalTrainingSystem.Core.Services.Implement
{
    public class CourseMaterialService : ICourseMaterialService
    {
        private readonly ICourseMaterialRepository _courseMaterialRepo;

        public CourseMaterialService(ICourseMaterialRepository courseMaterialRepo)
        {
            _courseMaterialRepo = courseMaterialRepo;
        }


        // ========== Modules ==========
        public async Task<ModuleDetailDto?> GetModuleAsync(int moduleId, CancellationToken ct = default)
        {
            return await _courseMaterialRepo.GetModuleAsync(moduleId, ct);
        }

        public async Task<IReadOnlyList<ModuleDetailDto>> GetModulesByCourseAsync(int courseId, CancellationToken ct = default)
        {
            return await _courseMaterialRepo.GetModulesByCourseAsync(courseId, ct);
        }

        public async Task<CourseModule> CreateModuleAsync(CreateModuleDto dto, CancellationToken ct = default)
        {
            return await _courseMaterialRepo.CreateModuleAsync(dto, ct);
        }

        public async Task<bool> UpdateModuleAsync(int moduleId, UpdateModuleDto dto, CancellationToken ct = default)
        {
            return await _courseMaterialRepo.UpdateModuleAsync(moduleId, dto, ct);
        }

        public async Task<bool> DeleteModuleAsync(int moduleId, CancellationToken ct = default)
        {
            return await _courseMaterialRepo.DeleteModuleAsync(moduleId, ct);
        }

        // ========== Lessons ==========
        public async Task<LessonListItemDto?> GetLessonAsync(int lessonId, CancellationToken ct = default)
        {
            return await _courseMaterialRepo.GetLessonAsync(lessonId, ct);
        }

        public async Task<IReadOnlyList<LessonListItemDto>> GetLessonsByModuleAsync(int moduleId, CancellationToken ct = default)
        {
            return await _courseMaterialRepo.GetLessonsByModuleAsync(moduleId, ct);
        }

        public async Task<Lesson> CreateLessonAsync(CreateLessonDto dto, CancellationToken ct = default)
        {
            return await _courseMaterialRepo.CreateLessonAsync(dto, ct);
        }

        public async Task<bool> UpdateLessonAsync(int lessonId, UpdateLessonDto dto, CancellationToken ct = default)
        {
            return await _courseMaterialRepo.UpdateLessonAsync(lessonId, dto, ct);
        }

        public async Task<bool> DeleteLessonAsync(int lessonId, CancellationToken ct = default)
        {
            return await _courseMaterialRepo.DeleteModuleAsync(lessonId, ct);
        }

        public async Task<(string url, string relativePath)> UploadLessonBinaryAsync(
            int lessonId, IFormFile file, CancellationToken ct = default)
        {
            return await _courseMaterialRepo.UploadLessonBinaryAsync(lessonId, file, ct);
        }

        public async Task<bool> ClearLessonFileAsync(int lessonId, CancellationToken ct = default)
        {
            return await _courseMaterialRepo.DeleteModuleAsync(lessonId, ct);
        }

        public async Task<Lesson> CreateQuizLessonFromExcelAsync(CreateQuizLessonRequest req, CancellationToken ct = default)
        {
            return await _courseMaterialRepo.CreateQuizLessonFromExcelAsync(req, ct);
        }
    }
}
