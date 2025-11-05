using System.ComponentModel.DataAnnotations;

namespace InternalTrainingSystem.Core.Models
{
    public class Lesson
    {
        public int Id { get; set; }
        public int ModuleId { get; set; }
        [Required, StringLength(200)]
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public LessonType Type { get; set; }
        public int OrderIndex { get; set; }  // Thứ tự lesson trong module
        public string? ContentUrl { get; set; }    // Video/File/Link
        public string? FilePath { get; set; }      // storage key (S3/Azure)
        public string? MimeType { get; set; }        
        public long? SizeBytes { get; set; }
        public string? AttachmentUrl { get; set; }
        public int? QuizId { get; set; }            // Type = Quiz
        public CourseModule Module { get; set; } = null!;
    }

    public enum LessonType
    {
        Video = 1,
        Reading = 2,
        Quiz = 3
    }
}
