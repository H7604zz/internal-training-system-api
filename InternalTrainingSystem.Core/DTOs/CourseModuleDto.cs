using InternalTrainingSystem.Core.Enums;

namespace InternalTrainingSystem.Core.DTOs
{
    // ===== Module DTOs =====
    public class CreateModuleDto
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int OrderIndex { get; set; } = 1;
    }

    public class UpdateModuleDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int OrderIndex { get; set; } = 1;
    }

    public class ModuleDetailDto
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int OrderIndex { get; set; }
        public List<LessonListItemDto> Lessons { get; set; } = new();
    }

    // ===== Lesson DTOs =====
    public class CreateLessonDto
    {
        public int ModuleId { get; set; }
        public string Title { get; set; } = string.Empty;
        public LessonType Type { get; set; }
        public int OrderIndex { get; set; } = 1;
        public string? ContentUrl { get; set; }   // Video/File/Link
        public string? ContentHtml { get; set; }  // Reading
        public int? QuizId { get; set; }          // Quiz
    }

    public class UpdateLessonDto
    {
        public string Title { get; set; } = string.Empty;
        public LessonType Type { get; set; }
        public int OrderIndex { get; set; } = 1;

        public string? ContentUrl { get; set; }
        public string? ContentHtml { get; set; }
        public int? QuizId { get; set; }
    }

    public class LessonListItemDto
    {
        public int Id { get; set; }
        public int ModuleId { get; set; }
        public string Title { get; set; } = string.Empty;
        public LessonType Type { get; set; }
        public int OrderIndex { get; set; }

        public string? ContentUrl { get; set; }
    }

}
