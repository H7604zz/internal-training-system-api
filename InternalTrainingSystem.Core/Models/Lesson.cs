using System.ComponentModel.DataAnnotations;

namespace InternalTrainingSystem.Core.Models
{
    public class Lesson
    {
        public int Id { get; set; }

        public int ModuleId { get; set; }

        [Required, StringLength(200)]
        public string Title { get; set; } = string.Empty;

        public LessonType Type { get; set; }

        public int OrderIndex { get; set; }  // Thứ tự lesson trong module
        public int? DurationMinutes { get; set; }  // ước lượng thời lượng
        public bool IsPreview { get; set; } = false;
        public bool IsRequired { get; set; } = true;

        // Nội dung/nguồn dữ liệu (tùy theo Type)
        public string? VideoUrl { get; set; }       // Type = Video
        public string? ContentHtml { get; set; }    // Type = Reading (lưu html/markdown-rendered)
        public string? FileUrl { get; set; }        // Type = File (đường dẫn lưu trữ)
        public string? ExternalUrl { get; set; }    // Type = Link

        // Nếu là bài Quiz cuối module, gắn vào Quiz hiện có (đã liên kết Course)
        public int? QuizId { get; set; }            // Type = Quiz

        public CourseModule Module { get; set; } = null!;
    }
}
