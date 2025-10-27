using ClosedXML.Excel;
using InternalTrainingSystem.Core.Constants;
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
                    if (string.IsNullOrWhiteSpace(dto.ContentHtml) &&
                string.IsNullOrWhiteSpace(dto.ContentUrl))
                        throw new ArgumentException("Reading requires ContentHtml or an uploaded file (ContentUrl).");
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
                    if (string.IsNullOrWhiteSpace(dto.ContentHtml) &&
                string.IsNullOrWhiteSpace(dto.ContentUrl))
                throw new ArgumentException("Reading requires ContentHtml or an uploaded file (ContentUrl).");
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

            if (lesson.Type is not (LessonType.File or LessonType.Video or LessonType.Reading))
                throw new InvalidOperationException("Only lessons with Type=File, Video, or Reading can have a binary upload.");

            if (file == null || file.Length == 0)
                throw new ArgumentException("Empty file.");

            var ext = Path.GetExtension(file.FileName);
            var contentType = file.ContentType ?? "";

            if (lesson.Type == LessonType.File || lesson.Type == LessonType.Reading)
            {
                if (file.Length > LessonContentConstraints.MaxDocBytes)
                    throw new ArgumentException($"File too large. Max {LessonContentConstraints.MaxDocBytes / (1024 * 1024)} MB.");

                if (!LessonContentConstraints.IsAllowedDoc(ext, contentType))
                    throw new ArgumentException("Only PDF/DOC/DOCX are allowed.");
            }
            else if (lesson.Type == LessonType.Video)
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
        public async Task<Lesson> CreateQuizLessonFromExcelAsync(CreateQuizLessonRequest req, CancellationToken ct = default)
        {
            // 1. Kiểm tra module tồn tại & lấy CourseId
            var module = await _context.CourseModules
                .Include(m => m.Course)
                .FirstOrDefaultAsync(m => m.Id == req.ModuleId, ct);

            if (module == null)
                throw new ArgumentException("Module not found.");

            if (req.ExcelFile == null || req.ExcelFile.Length == 0)
                throw new ArgumentException("Excel file is required.");

            var ext = Path.GetExtension(req.ExcelFile.FileName).ToLowerInvariant();
            if (ext != ".xlsx" && ext != ".xls")
                throw new ArgumentException("Only .xlsx or .xls is allowed for quiz import.");

            // 2. Tạo Quiz rỗng trước
            var quiz = new Quiz
            {
                // giả sử Quiz có các field như:
                // QuizId (identity)
                Title = string.IsNullOrWhiteSpace(req.QuizTitle) ? req.Title : req.QuizTitle,
                CourseId = module.CourseId,
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            };

            _context.Quizzes.Add(quiz);
            await _context.SaveChangesAsync(ct); // để có QuizId

            // 3. Đọc file Excel và build câu hỏi + đáp án
            using (var stream = req.ExcelFile.OpenReadStream())
            using (var workbook = new XLWorkbook(stream))
            {
                // Lấy sheet đầu tiên
                var ws = workbook.Worksheets.First();

                // Giả định hàng đầu tiên là header
                // QuestionText | OptionA | OptionB | OptionC | OptionD | CorrectOption | Score
                var firstDataRow = 2;
                var lastRow = ws.LastRowUsed().RowNumber();

                for (int row = firstDataRow; row <= lastRow; row++)
                {
                    var questionText = ws.Cell(row, 1).GetString().Trim();
                    var optA = ws.Cell(row, 2).GetString().Trim();
                    var optB = ws.Cell(row, 3).GetString().Trim();
                    var optC = ws.Cell(row, 4).GetString().Trim();
                    var optD = ws.Cell(row, 5).GetString().Trim();
                    var correctOpt = ws.Cell(row, 6).GetString().Trim().ToUpperInvariant(); // "A"/"B"/"C"/"D"
                    var scoreVal = ws.Cell(row, 7).TryGetValue<double>(out var pts) ? pts : 1.0;

                    if (string.IsNullOrWhiteSpace(questionText))
                        continue; // bỏ qua dòng trống

                    // 3.1 tạo Question
                    var question = new Question
                    {
                        // QuestionId (identity)
                        QuizId = quiz.QuizId,
                        QuestionText = questionText,
                        Points = (int)Math.Round(pts)
                    };
                    _context.Questions.Add(question);
                    await _context.SaveChangesAsync(ct); // để có QuestionId

                    // 3.2 tạo các Answer
                    // Helper local func
                    async Task AddAnswer(string text, bool isCorrect)
                    {
                        if (string.IsNullOrWhiteSpace(text)) return;
                        var ans = new Answer
                        {
                            QuestionId = question.QuestionId,
                            AnswerText = text,
                            IsCorrect = isCorrect
                        };
                        _context.Answers.Add(ans);
                        await _context.SaveChangesAsync(ct);
                    }

                    await AddAnswer(optA, correctOpt == "A");
                    await AddAnswer(optB, correctOpt == "B");
                    await AddAnswer(optC, correctOpt == "C");
                    await AddAnswer(optD, correctOpt == "D");
                }
            }

            // 4. Tạo Lesson type = Quiz, link tới quiz vừa tạo
            var lesson = new Lesson
            {
                ModuleId = req.ModuleId,
                Title = string.IsNullOrWhiteSpace(req.Title) ? quiz.Title : req.Title,
                Type = LessonType.Quiz,
                OrderIndex = req.OrderIndex,
                QuizId = quiz.QuizId,

                // Quiz lesson không cần ContentUrl / ContentHtml
                ContentUrl = null,
                ContentHtml = null
            };

            _context.Lessons.Add(lesson);
            await _context.SaveChangesAsync(ct);

            return lesson;
        }
    }
}
