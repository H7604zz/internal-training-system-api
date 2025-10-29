
using System.ComponentModel.DataAnnotations;
using InternalTrainingSystem.Core.Models;
using Microsoft.AspNetCore.Mvc;

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
    public class CreateQuizLessonRequest
    {
        public int ModuleId { get; set; }
        public string Title { get; set; } = string.Empty; 
        public int OrderIndex { get; set; } = 1;
        public string QuizTitle { get; set; } = string.Empty;
        // File Excel upload
        public IFormFile ExcelFile { get; set; } = null!;
    }
    // Lesson full course
    public class NewLessonSpecDto
    {
        [Required(ErrorMessage = "Tiêu đề bài học là bắt buộc.")]
        [StringLength(200, ErrorMessage = "Tiêu đề bài học không được vượt quá 200 ký tự.")]
        public string Title { get; set; } = string.Empty;
        [Range(1, int.MaxValue, ErrorMessage = "OrderIndex của lesson phải >= 1.")]
        public int OrderIndex { get; set; } = 1;

        // 1 = Video, 2 = File, 3 = Reading, 4 = Link, 5 = Quiz 
        [Required(ErrorMessage = "Loại bài học (Type) là bắt buộc.")]
        public LessonType Type { get; set; }

        public string? ContentUrl { get; set; }

        public string? ContentHtml { get; set; }

        public bool UploadBinary { get; set; } = false;

        public string? QuizTitle { get; set; }
        public bool IsQuizExcel { get; set; } = false;
    }

    // Module full course
    public class NewModuleSpecDto
    {
        [Required(ErrorMessage = "Tiêu đề module là bắt buộc.")]
        [StringLength(200, ErrorMessage = "Tiêu đề module không được vượt quá 200 ký tự.")]
        public string Title { get; set; } = string.Empty;
        [StringLength(1000, ErrorMessage = "Mô tả module không được vượt quá 1000 ký tự.")]
        public string? Description { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "OrderIndex của module phải >= 1.")]
        public int OrderIndex { get; set; } = 1;

        [Required(ErrorMessage = "Module phải có ít nhất 1 bài học (lesson).")]
        [MinLength(1, ErrorMessage = "Module phải có ít nhất 1 bài học (lesson).")]
        public List<NewLessonSpecDto> Lessons { get; set; } = new();
    }

    // Metadata Create Full Course 
    public class CreateCourseMetadataDto
    {
        [Required(ErrorMessage = "Mã khóa học là bắt buộc.")]
        [StringLength(50, ErrorMessage = "Mã khóa học không được vượt quá 50 ký tự.")]
        public string CourseCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên khóa học là bắt buộc.")]
        [StringLength(200, ErrorMessage = "Tên khóa học không được vượt quá 200 ký tự.")]
        public string CourseName { get; set; } = string.Empty;
        [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Danh mục khóa học là bắt buộc.")]
        public int CourseCategoryId { get; set; }

        [Required]
        public int Duration { get; set; }

        [RegularExpression("Beginner|Intermediate|Advanced",
            ErrorMessage = "Cấp độ chỉ được phép là 'Beginner', 'Intermediate' hoặc 'Advanced'.")]
        public string Level { get; set; } = "Beginner";
        public bool IsOnline { get; set; } = true;

        public bool IsMandatory { get; set; } = false;

        public List<int> Departments { get; set; } = new();

        [Required]
        public List<NewModuleSpecDto> Modules { get; set; } = new();
    }

    public class CreateCourseFormDto
    {
        [FromForm(Name = "metadata")]
        public string Metadata { get; set; } = string.Empty;

        [FromForm(Name = "lessonFiles")]
        public List<IFormFile> LessonFiles { get; set; } = new();
    }


}
