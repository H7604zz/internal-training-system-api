using ClosedXML.Excel;
using InternalTrainingSystem.Core.Common.Constants;
using InternalTrainingSystem.Core.DB;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Repository.Interface;
using InternalTrainingSystem.Core.Services.Interface;
using InternalTrainingSystem.Core.Utils;
using Microsoft.EntityFrameworkCore;

namespace InternalTrainingSystem.Core.Repository.Implement
{
    public class CourseMaterialRepository : ICourseMaterialRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileStorage _storage;

        public CourseMaterialRepository(ApplicationDbContext context, IFileStorage storage)
        {
            _storage = storage;
            _context = context;
            _storage = storage;
        }

        // ========== Lessons ==========
        public async Task<(string url, string relativePath)> UploadLessonBinaryAsync(
    int lessonId, IFormFile file, CancellationToken ct = default)
        {
            var lesson = await _context.Lessons.FirstOrDefaultAsync(l => l.Id == lessonId, ct);
            if (lesson == null) throw new ArgumentException("Lesson not found.");

            
            if (lesson.Type is not (LessonType.Reading ))
                throw new InvalidOperationException(
                    "This lesson type does not support binary upload. Only Reading/File can upload binary data.");

            if (file == null || file.Length == 0) throw new ArgumentException("Empty file.");

            var ext = Path.GetExtension(file.FileName);
            var contentType = string.IsNullOrWhiteSpace(file.ContentType)
                ? "application/octet-stream"
                : file.ContentType;

            switch (lesson.Type)
            {
                case LessonType.Reading:
                    if (file.Length > LessonContentConstraints.MaxDocBytes)
                        throw new ArgumentException($"File too large. Max {LessonContentConstraints.MaxDocBytes / (1024 * 1024)} MB.");

                    if (!LessonContentConstraints.IsAllowedDoc(ext, contentType))
                        throw new ArgumentException("Only PDF/DOC/DOCX/PPTX/TXT are allowed.");
                    break;
            }

            var subFolder = $"uploads/lessons/{lessonId}";

            // ⬅️ Truyền kèm metadata để S3 set đúng Content-Type / Encoding / Disposition
            var meta = StorageObjectMetadata.ForUpload(file.FileName, contentType);
            var (url, relativePath) = await _storage.SaveAsync(file, subFolder, meta, ct);

            lesson.ContentUrl = url;
            lesson.FilePath = relativePath;
            lesson.MimeType = meta.ContentType;   // đã chuẩn hoá bên dưới
            lesson.SizeBytes = file.Length;

            await _context.SaveChangesAsync(ct);
            return (url, relativePath);
        }
        
        public Task<Lesson?> GetWithModuleAsync(int lessonId, CancellationToken ct = default) =>
            _context.Lessons.Include(l => l.Module)
                       .FirstOrDefaultAsync(l => l.Id == lessonId, ct);

        public Task<Lesson?> GetByIdAsync(int lessonId, CancellationToken ct = default) =>
            _context.Lessons.FirstOrDefaultAsync(l => l.Id == lessonId, ct);
    }
}
