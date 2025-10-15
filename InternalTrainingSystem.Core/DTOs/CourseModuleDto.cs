using InternalTrainingSystem.Core.Models;

namespace InternalTrainingSystem.Core.DTOs
{
    public class CreateModuleDto
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int OrderIndex { get; set; } = 1;
        public int? EstimatedMinutes { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateModuleDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int OrderIndex { get; set; } = 1;
        public int? EstimatedMinutes { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class ModuleDetailDto
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int OrderIndex { get; set; }
        public int? EstimatedMinutes { get; set; }
        public bool IsActive { get; set; }

        public List<LessonListItemDto> Lessons { get; set; } = new();
    }

    // Models/DTOs/Lesson DTOs
    public class CreateLessonDto
    {
        public int ModuleId { get; set; }
        public string Title { get; set; } = string.Empty;
        public LessonType Type { get; set; }
        public int OrderIndex { get; set; } = 1;
        public int? DurationMinutes { get; set; }
        public bool IsPreview { get; set; } = false;
        public bool IsRequired { get; set; } = true;

        public string? VideoUrl { get; set; }
        public string? ContentHtml { get; set; }
        public string? FileUrl { get; set; }
        public string? ExternalUrl { get; set; }
        public int? QuizId { get; set; }
    }

    public class UpdateLessonDto
    {
        public string Title { get; set; } = string.Empty;
        public LessonType Type { get; set; }
        public int OrderIndex { get; set; } = 1;
        public int? DurationMinutes { get; set; }
        public bool IsPreview { get; set; } = false;
        public bool IsRequired { get; set; } = true;

        public string? VideoUrl { get; set; }
        public string? ContentHtml { get; set; }
        public string? FileUrl { get; set; }
        public string? ExternalUrl { get; set; }
        public int? QuizId { get; set; }
    }

    public class LessonListItemDto
    {
        public int Id { get; set; }
        public int ModuleId { get; set; }
        public string Title { get; set; } = string.Empty;
        public LessonType Type { get; set; }
        public int OrderIndex { get; set; }
        public int? DurationMinutes { get; set; }
        public bool IsPreview { get; set; }
        public bool IsRequired { get; set; }
    }

}
