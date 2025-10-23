using InternalTrainingSystem.Core.DB;
using InternalTrainingSystem.Core.DTOs;
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
            _storage = storage;
            _context = context;
        }

        private static void ValidateLessonPayload(CreateLessonDto dto)
        {
            switch (dto.Type)
            {
                case LessonType.Video:
                    if (string.IsNullOrWhiteSpace(dto.VideoUrl))
                        throw new ArgumentException("VideoUrl is required for Video lesson.");
                    break;
                case LessonType.Reading:
                    if (string.IsNullOrWhiteSpace(dto.ContentHtml))
                        throw new ArgumentException("ContentHtml is required for Reading lesson.");
                    break;
                case LessonType.File:
                    if (string.IsNullOrWhiteSpace(dto.FileUrl))
                        throw new ArgumentException("FileUrl is required for File lesson.");
                    break;
                case LessonType.Link:
                    if (string.IsNullOrWhiteSpace(dto.ExternalUrl))
                        throw new ArgumentException("ExternalUrl is required for Link lesson.");
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
                    if (string.IsNullOrWhiteSpace(dto.VideoUrl))
                        throw new ArgumentException("VideoUrl is required for Video lesson.");
                    break;
                case LessonType.Reading:
                    if (string.IsNullOrWhiteSpace(dto.ContentHtml))
                        throw new ArgumentException("ContentHtml is required for Reading lesson.");
                    break;
                case LessonType.File:
                    if (string.IsNullOrWhiteSpace(dto.FileUrl))
                        throw new ArgumentException("FileUrl is required for File lesson.");
                    break;
                case LessonType.Link:
                    if (string.IsNullOrWhiteSpace(dto.ExternalUrl))
                        throw new ArgumentException("ExternalUrl is required for Link lesson.");
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
                EstimatedMinutes = m.EstimatedMinutes,
                IsActive = m.IsActive,
                Lessons = m.Lessons
                    .OrderBy(l => l.OrderIndex)
                    .Select(l => new LessonListItemDto
                    {
                        Id = l.Id,
                        ModuleId = l.ModuleId,
                        Title = l.Title,
                        Type = l.Type,
                        OrderIndex = l.OrderIndex,
                        DurationMinutes = l.DurationMinutes,
                        IsPreview = l.IsPreview,
                        IsRequired = l.IsRequired
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
                EstimatedMinutes = m.EstimatedMinutes,
                IsActive = m.IsActive,
                Lessons = m.Lessons
                    .OrderBy(l => l.OrderIndex)
                    .Select(l => new LessonListItemDto
                    {
                        Id = l.Id,
                        ModuleId = l.ModuleId,
                        Title = l.Title,
                        Type = l.Type,
                        OrderIndex = l.OrderIndex,
                        DurationMinutes = l.DurationMinutes,
                        IsPreview = l.IsPreview,
                        IsRequired = l.IsRequired
                    }).ToList()
            }).ToList();
        }

        public async Task<CourseModule> CreateModuleAsync(CreateModuleDto dto, CancellationToken ct = default)
        {
            // Ensure course exists
            var courseExists = await _context.Courses.AnyAsync(c => c.CourseId == dto.CourseId, ct);
            if (!courseExists) throw new ArgumentException("Course not found.");

            var entity = new CourseModule
            {
                CourseId = dto.CourseId,
                Title = dto.Title.Trim(),
                Description = dto.Description,
                OrderIndex = dto.OrderIndex,
                EstimatedMinutes = dto.EstimatedMinutes,
                IsActive = dto.IsActive
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
            entity.EstimatedMinutes = dto.EstimatedMinutes;
            entity.IsActive = dto.IsActive;

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
                DurationMinutes = l.DurationMinutes,
                IsPreview = l.IsPreview,
                IsRequired = l.IsRequired
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
                DurationMinutes = l.DurationMinutes,
                IsPreview = l.IsPreview,
                IsRequired = l.IsRequired
            }).ToList();
        }

        public async Task<Lesson> CreateLessonAsync(CreateLessonDto dto, CancellationToken ct = default)
        {
            // Ensure module exists
            var module = await _context.CourseModules.Include(m => m.Course).FirstOrDefaultAsync(m => m.Id == dto.ModuleId, ct);
            if (module == null) throw new ArgumentException("Module not found.");

            // Validate payload by type
            ValidateLessonPayload(dto);

            // If quiz lesson, ensure quiz belongs to same course
            if (dto.Type == LessonType.Quiz && dto.QuizId.HasValue)
            {
                var quiz = await _context.Quizzes.AsNoTracking().FirstOrDefaultAsync(q => q.QuizId == dto.QuizId.Value, ct);
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
                DurationMinutes = dto.DurationMinutes,
                IsPreview = dto.IsPreview,
                IsRequired = dto.IsRequired,
                VideoUrl = dto.VideoUrl,
                ContentHtml = dto.ContentHtml,
                FileUrl = dto.FileUrl,
                ExternalUrl = dto.ExternalUrl,
                QuizId = dto.QuizId
            };

            _context.Lessons.Add(entity);
            await _context.SaveChangesAsync(ct);
            return entity;
        }

        public async Task<bool> UpdateLessonAsync(int lessonId, UpdateLessonDto dto, CancellationToken ct = default)
        {
            var entity = await _context.Lessons.Include(l => l.Module).ThenInclude(m => m.Course)
                .FirstOrDefaultAsync(l => l.Id == lessonId, ct);
            if (entity == null) return false;

            // Validate payload
            ValidateLessonPayload(dto);

            // Quiz must belong to same course
            if (dto.Type == LessonType.Quiz && dto.QuizId.HasValue)
            {
                var quiz = await _context.Quizzes.AsNoTracking().FirstOrDefaultAsync(q => q.QuizId == dto.QuizId.Value, ct);
                if (quiz == null) throw new ArgumentException("Quiz not found.");
                if (quiz.CourseId != entity.Module.CourseId)
                    throw new ArgumentException("Quiz must belong to the same Course as the Module.");
            }

            entity.Title = dto.Title.Trim();
            entity.Type = dto.Type;
            entity.OrderIndex = dto.OrderIndex;
            entity.DurationMinutes = dto.DurationMinutes;
            entity.IsPreview = dto.IsPreview;
            entity.IsRequired = dto.IsRequired;
            entity.VideoUrl = dto.VideoUrl;
            entity.ContentHtml = dto.ContentHtml;
            entity.FileUrl = dto.FileUrl;
            entity.ExternalUrl = dto.ExternalUrl;
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
        private static readonly HashSet<string> AllowedDocContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
    };

        private static readonly HashSet<string> AllowedDocExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".doc", ".docx"
    };

        private const long MaxFileBytes = 20 * 1024 * 1024; // 20 MB

        public async Task<(string url, string relativePath)> UploadLessonFileAsync(
            int lessonId, IFormFile file, CancellationToken ct = default)
        {
            var lesson = await _context.Lessons.FirstOrDefaultAsync(l => l.Id == lessonId, ct);
            if (lesson == null) throw new ArgumentException("Lesson not found.");
            if (lesson.Type != LessonType.File)
                throw new InvalidOperationException("Only lessons with Type=File can have a file uploaded.");

            if (file == null || file.Length == 0)
                throw new ArgumentException("Empty file.");

            if (file.Length > MaxFileBytes)
                throw new ArgumentException($"File too large. Max {MaxFileBytes / (1024 * 1024)} MB.");

            // Validate content type & extension
            var ext = Path.GetExtension(file.FileName);
            if (!AllowedDocExtensions.Contains(ext))
                throw new ArgumentException("Only .pdf, .doc, .docx are allowed.");

            var contentType = file.ContentType ?? "";
            if (!AllowedDocContentTypes.Contains(contentType))
                throw new ArgumentException("Invalid MIME type. Only PDF/DOC/DOCX are allowed.");

            // Save to /wwwroot/uploads/lessons/{lessonId}/
            var subFolder = $"uploads/lessons/{lessonId}";
            var (url, relativePath) = await _storage.SaveAsync(file, subFolder, ct);

            // If previously had a file, delete old (optional)
            if (!string.IsNullOrWhiteSpace(lesson.FileUrl))
            {
                // FileUrl là URL, cần tự lưu thêm relativePath vào DB để delete chuẩn.
                // Ở đây tối giản: nếu bạn muốn delete chính xác, hãy thêm trường Lesson.FilePath (relativePath) trong DB.
            }

            lesson.FileUrl = url; // lưu public URL; khuyến nghị thêm Lesson.FilePath để delete chính xác
            await _context.SaveChangesAsync(ct);

            return (url, relativePath);
        }

        public async Task<bool> ClearLessonFileAsync(int lessonId, CancellationToken ct = default)
        {
            var lesson = await _context.Lessons.FirstOrDefaultAsync(l => l.Id == lessonId, ct);
            if (lesson == null) return false;

            // Nếu bạn thêm cột Lesson.FilePath để delete chính xác:
            // if (!string.IsNullOrEmpty(lesson.FilePath))
            //     await _storage.DeleteAsync(lesson.FilePath, ct);

            lesson.FileUrl = null;
            await _context.SaveChangesAsync(ct);
            return true;
        }
    }
}
