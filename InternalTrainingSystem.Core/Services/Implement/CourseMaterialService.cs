using InternalTrainingSystem.Core.Constants;
using InternalTrainingSystem.Core.DB;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Enums;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Services.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client.Extensions.Msal;

namespace InternalTrainingSystem.Core.Services.Implement
{
    public class CourseMaterialService : ICourseMaterialService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileStorage _storage;

        public CourseMaterialService(ApplicationDbContext context, IFileStorage storage)
        {
            _context = context;
            _storage = storage;
        }

        private static void ValidateLessonPayload(CreateLessonDto dto)
        {
            switch (dto.Type)
            {
                case LessonType.Video:
                case LessonType.File:
                case LessonType.Link:
                    if (string.IsNullOrWhiteSpace(dto.ContentUrl))
                        throw new ArgumentException("ContentUrl is required for Video/File/Link lesson.");
                    break;

                case LessonType.Reading:
                    if (string.IsNullOrWhiteSpace(dto.ContentHtml))
                        throw new ArgumentException("ContentHtml is required for Reading lesson.");
                    break;

                case LessonType.Quiz:
                    if (!dto.QuizId.HasValue)
                        throw new ArgumentException("QuizId is required for Quiz lesson.");
                    break;
            }
        }

        private static void ValidateLessonPayload(UpdateLessonDto dto)
        {
            switch (dto.Type)
            {
                case LessonType.Video:
                case LessonType.File:
                case LessonType.Link:
                    if (string.IsNullOrWhiteSpace(dto.ContentUrl))
                        throw new ArgumentException("ContentUrl is required for Video/File/Link lesson.");
                    break;

                case LessonType.Reading:
                    if (string.IsNullOrWhiteSpace(dto.ContentHtml))
                        throw new ArgumentException("ContentHtml is required for Reading lesson.");
                    break;

                case LessonType.Quiz:
                    if (!dto.QuizId.HasValue)
                        throw new ArgumentException("QuizId is required for Quiz lesson.");
                    break;
            }
        }

        // ========== Modules ==========
        public async Task<ModuleDetailDto?> GetModuleAsync(int moduleId, CancellationToken ct = default)
        {
            var m = await _context.CourseModules
                .Include(x => x.Lessons)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == moduleId, ct);

            if (m == null) return null;

            return new ModuleDetailDto
            {
                Id = m.Id,
                CourseId = m.CourseId,
                Title = m.Title,
                Description = m.Description,
                OrderIndex = m.OrderIndex,
                Lessons = m.Lessons
                    .OrderBy(l => l.OrderIndex)
                    .Select(l => new LessonListItemDto
                    {
                        Id = l.Id,
                        ModuleId = l.ModuleId,
                        Title = l.Title,
                        Type = l.Type,
                        OrderIndex = l.OrderIndex,
                        ContentUrl = l.ContentUrl
                    }).ToList()
            };
        }

        public async Task<IReadOnlyList<ModuleDetailDto>> GetModulesByCourseAsync(int courseId, CancellationToken ct = default)
        {
            var list = await _context.CourseModules
                .Where(x => x.CourseId == courseId)
                .OrderBy(x => x.OrderIndex)
                .Include(x => x.Lessons)
                .AsNoTracking()
                .ToListAsync(ct);

            return list.Select(m => new ModuleDetailDto
            {
                Id = m.Id,
                CourseId = m.CourseId,
                Title = m.Title,
                Description = m.Description,
                OrderIndex = m.OrderIndex,
                Lessons = m.Lessons
                    .OrderBy(l => l.OrderIndex)
                    .Select(l => new LessonListItemDto
                    {
                        Id = l.Id,
                        ModuleId = l.ModuleId,
                        Title = l.Title,
                        Type = l.Type,
                        OrderIndex = l.OrderIndex,
                        ContentUrl = l.ContentUrl
                    }).ToList()
            }).ToList();
        }

        public async Task<CourseModule> CreateModuleAsync(CreateModuleDto dto, CancellationToken ct = default)
        {
            var courseExists = await _context.Courses.AnyAsync(c => c.CourseId == dto.CourseId, ct);
            if (!courseExists) throw new ArgumentException("Course not found.");

            var entity = new CourseModule
            {
                CourseId = dto.CourseId,
                Title = dto.Title.Trim(),
                Description = dto.Description,
                OrderIndex = dto.OrderIndex
            };

            _context.CourseModules.Add(entity);
            await _context.SaveChangesAsync(ct);
            return entity;
        }

        public async Task<bool> UpdateModuleAsync(int moduleId, UpdateModuleDto dto, CancellationToken ct = default)
        {
            var entity = await _context.CourseModules.FirstOrDefaultAsync(m => m.Id == moduleId, ct);
            if (entity == null) return false;

            entity.Title = dto.Title.Trim();
            entity.Description = dto.Description;
            entity.OrderIndex = dto.OrderIndex;

            await _context.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> DeleteModuleAsync(int moduleId, CancellationToken ct = default)
        {
            var entity = await _context.CourseModules.FirstOrDefaultAsync(m => m.Id == moduleId, ct);
            if (entity == null) return false;

            _context.CourseModules.Remove(entity);
            await _context.SaveChangesAsync(ct);
            return true;
        }

        // ========== Lessons ==========
        public async Task<LessonListItemDto?> GetLessonAsync(int lessonId, CancellationToken ct = default)
        {
            var l = await _context.Lessons.AsNoTracking().FirstOrDefaultAsync(x => x.Id == lessonId, ct);
            if (l == null) return null;

            return new LessonListItemDto
            {
                Id = l.Id,
                ModuleId = l.ModuleId,
                Title = l.Title,
                Type = l.Type,
                OrderIndex = l.OrderIndex,
                ContentUrl = l.ContentUrl
            };
        }

        public async Task<IReadOnlyList<LessonListItemDto>> GetLessonsByModuleAsync(int moduleId, CancellationToken ct = default)
        {
            var list = await _context.Lessons
                .Where(l => l.ModuleId == moduleId)
                .OrderBy(l => l.OrderIndex)
                .AsNoTracking()
                .ToListAsync(ct);

            return list.Select(l => new LessonListItemDto
            {
                Id = l.Id,
                ModuleId = l.ModuleId,
                Title = l.Title,
                Type = l.Type,
                OrderIndex = l.OrderIndex,
                ContentUrl = l.ContentUrl
            }).ToList();
        }

        public async Task<Lesson> CreateLessonAsync(CreateLessonDto dto, CancellationToken ct = default)
        {
            var module = await _context.CourseModules
                .Include(m => m.Course)
                .FirstOrDefaultAsync(m => m.Id == dto.ModuleId, ct);
            if (module == null) throw new ArgumentException("Module not found.");

            ValidateLessonPayload(dto);

            if (dto.Type == LessonType.Quiz && dto.QuizId.HasValue)
            {
                var quiz = await _context.Quizzes.AsNoTracking()
                    .FirstOrDefaultAsync(q => q.QuizId == dto.QuizId.Value, ct);
                if (quiz == null) throw new ArgumentException("Quiz not found.");
                if (quiz.CourseId != module.CourseId)
                    throw new ArgumentException("Quiz must belong to the same Course as the Module.");
            }

            var entity = new Lesson
            {
                ModuleId = dto.ModuleId,
                Title = dto.Title.Trim(),
                Type = dto.Type,
                OrderIndex = dto.OrderIndex,
                ContentUrl = dto.ContentUrl,
                ContentHtml = dto.ContentHtml,
                QuizId = dto.QuizId
            };

            _context.Lessons.Add(entity);
            await _context.SaveChangesAsync(ct);
            return entity;
        }

        public async Task<bool> UpdateLessonAsync(int lessonId, UpdateLessonDto dto, CancellationToken ct = default)
        {
            var entity = await _context.Lessons
                .Include(l => l.Module).ThenInclude(m => m.Course)
                .FirstOrDefaultAsync(l => l.Id == lessonId, ct);
            if (entity == null) return false;

            ValidateLessonPayload(dto);

            if (dto.Type == LessonType.Quiz && dto.QuizId.HasValue)
            {
                var quiz = await _context.Quizzes.AsNoTracking()
                    .FirstOrDefaultAsync(q => q.QuizId == dto.QuizId.Value, ct);
                if (quiz == null) throw new ArgumentException("Quiz not found.");
                if (quiz.CourseId != entity.Module.CourseId)
                    throw new ArgumentException("Quiz must belong to the same Course as the Module.");
            }

            entity.Title = dto.Title.Trim();
            entity.Type = dto.Type;
            entity.OrderIndex = dto.OrderIndex;
            entity.ContentUrl = dto.ContentUrl;
            entity.ContentHtml = dto.ContentHtml;
            entity.QuizId = dto.QuizId;

            await _context.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> DeleteLessonAsync(int lessonId, CancellationToken ct = default)
        {
            var entity = await _context.Lessons.FirstOrDefaultAsync(l => l.Id == lessonId, ct);
            if (entity == null) return false;
            _context.Lessons.Remove(entity);
            await _context.SaveChangesAsync(ct);
            return true;
        }

        public async Task<(string url, string relativePath)> UploadLessonBinaryAsync(
    int lessonId, IFormFile file, CancellationToken ct = default)
        {
            var lesson = await _context.Lessons.FirstOrDefaultAsync(l => l.Id == lessonId, ct);
            if (lesson == null) throw new ArgumentException("Lesson not found.");

            if (lesson.Type is not (LessonType.File or LessonType.Video))
                throw new InvalidOperationException("Only lessons with Type=File or Type=Video can have a binary upload.");

            if (file == null || file.Length == 0)
                throw new ArgumentException("Empty file.");

            var ext = Path.GetExtension(file.FileName);
            var contentType = file.ContentType ?? "";

            if (lesson.Type == LessonType.File)
            {
                if (file.Length > LessonContentConstraints.MaxDocBytes)
                    throw new ArgumentException($"File too large. Max {LessonContentConstraints.MaxDocBytes / (1024 * 1024)} MB.");
                if (!LessonContentConstraints.IsAllowedDoc(ext, contentType))
                    throw new ArgumentException("Only PDF/DOC/DOCX are allowed.");
            }
            else // Video
            {
                if (file.Length > LessonContentConstraints.MaxVideoBytes)
                    throw new ArgumentException($"Video too large. Max {LessonContentConstraints.MaxVideoBytes / (1024 * 1024)} MB.");
                if (!LessonContentConstraints.IsAllowedVideo(ext, contentType))
                    throw new ArgumentException("Only MP4/MOV/M4V/WEBM are allowed.");
            }

            var subFolder = $"uploads/lessons/{lessonId}";
            var (url, relativePath) = await _storage.SaveAsync(file, subFolder, ct);

            lesson.ContentUrl = url;
            lesson.FilePath = relativePath;
            lesson.MimeType = contentType;
            lesson.SizeBytes = file.Length;

            await _context.SaveChangesAsync(ct);
            return (url, relativePath);
        }


        public async Task<bool> ClearLessonFileAsync(int lessonId, CancellationToken ct = default)
        {
            var lesson = await _context.Lessons.FirstOrDefaultAsync(l => l.Id == lessonId, ct);
            if (lesson == null) return false;

            if (!string.IsNullOrEmpty(lesson.FilePath))
                await _storage.DeleteAsync(lesson.FilePath, ct);

            lesson.ContentUrl = null;
            lesson.FilePath = null;
            lesson.MimeType = null;
            lesson.SizeBytes = null;

            await _context.SaveChangesAsync(ct);
            return true;
        }
    }
}
