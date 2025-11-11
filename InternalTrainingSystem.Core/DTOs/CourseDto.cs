using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Constants;
using System.ComponentModel.DataAnnotations;

namespace InternalTrainingSystem.Core.DTOs
{
    public class CourseDetailDto
    {
        public int CourseId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? CategoryName { get; set; }
        public int Duration { get; set; }
        public string Level { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool IsOnline { get; set; }
        public bool IsMandatory { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public List<DepartmentListDto> Departments { get; set; } = new();
        public List<ModuleDetailDto> Modules { get; set; } = new();
    }

    public class CreateCourseDto
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

        [Required(ErrorMessage = "Thời lượng là bắt buộc.")]
        [Range(1, int.MaxValue, ErrorMessage = "Thời lượng phải lớn hơn 0.")]
        public int Duration { get; set; }

        [RegularExpression("Beginner|Intermediate|Advanced",
            ErrorMessage = "Cấp độ chỉ được phép là 'Beginner', 'Intermediate' hoặc 'Advanced'.")]
        public string Level { get; set; } = "Beginner";

        public String Status { get; set; } = "Pending";

        public List<int>? Departments { get; set; } // Danh sách ID phòng ban
    }

    

    public sealed class UpdateCourseRejectDto
    {
        public string CourseName { get; set; } = default!;
        public string? Description { get; set; }
        public int Duration { get; set; }
        public string Level { get; set; } = default!;
        public int CourseCategoryId { get; set; }
        public List<int> DepartmentIds { get; set; } = new();
    }

    public class CourseSearchRequest
    {
        public string? Q { get; set; }

        public string? Category { get; set; }

        public List<string>? Categories { get; set; }

        public int? CategoryId { get; set; }

        public bool? IsActive { get; set; }
        public string? Level { get; set; }
        public string? Department { get; set; }
        public int? DurationFrom { get; set; }
        public int? DurationTo { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public string? Sort { get; set; } = "-CreatedDate";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class CourseListItemDto
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string? Code { get; set; }
        public string? Description { get; set; }
        public int Duration { get; set; }
        public string Level { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsOnline { get; set; }
        public bool IsMandatory { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? Status { get; set; } = CourseConstants.Status.Pending; // Course approval status: Pending, Approved, Rejected, Draft
        public List<DepartmentListDto> Departments { get; set; } = new();
        public string? CreatedBy { get; set; } = string.Empty;
        public DateTime? UpdatedDate { get; set; }
        public string? UpdatedBy { get; set; } = string.Empty;
    }

    public record ToggleStatusDto(string Status);

    public class GetAllCoursesRequest
    {
        public string? Search { get; set; }
        public string? Status { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    // Dùng khi submit form multipart/form-data (giống CreateCourseFormDto)
    public class UpdateCourseFormDto
    {
        // JSON string của UpdateCourseMetadataDto
        [Required]
        public string Metadata { get; set; } = string.Empty;

        // Danh sách file upload kèm theo (để tham chiếu bằng index)
        public IList<IFormFile> LessonFiles { get; set; } = new List<IFormFile>();
    }

    public class UpdateCourseMetadataDto : IValidatableObject
    {
        [Required, StringLength(200)]
        public string CourseName { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        public int CourseCategoryId { get; set; }

        [Range(0, int.MaxValue)]
        public int Duration { get; set; }

        [Required, StringLength(20)]
        public string Level { get; set; } = "Beginner";

        public bool IsOnline { get; set; } = true;
        public bool IsMandatory { get; set; } = false;

        // Departments được gán cho Course
        public List<int> Departments { get; set; } = new();

        // Danh sách module để upsert
        [MinLength(1, ErrorMessage = "Course must have at least one module.")]
        public List<UpdateModuleSpecDto> Modules { get; set; } = new();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Modules == null || Modules.Count == 0)
                yield return new ValidationResult("Modules is required.", new[] { nameof(Modules) });
        }
    }

    public class UpdateModuleSpecDto : IValidatableObject
    {
        // null => module mới
        public int? ModuleId { get; set; }

        [Required, StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Range(0, int.MaxValue)]
        public int OrderIndex { get; set; }

        [MinLength(1, ErrorMessage = "Module must have at least one lesson.")]
        public List<UpdateLessonSpecDto> Lessons { get; set; } = new();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Lessons == null || Lessons.Count == 0)
                yield return new ValidationResult("Lessons is required.", new[] { nameof(Lessons) });
        }
    }

    public class UpdateLessonSpecDto
    {
        // null => lesson mới
        public int? LessonId { get; set; }

        [Required, StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        public LessonType Type { get; set; }

        [Range(0, int.MaxValue)]
        public int OrderIndex { get; set; }

        // Với Video
        public string? ContentUrl { get; set; }
        public string? AttachmentUrl { get; set; }

        // Với Quiz (Excel)
        public bool IsQuizExcel { get; set; } = false;
        public string? QuizTitle { get; set; }
        public int? QuizTimeLimit { get; set; }

        [Range(1, 20)]
        public int? QuizMaxAttempts { get; set; }

        [Range(0, 100)]
        public int? QuizPassingScore { get; set; }

        // File indices trong UpdateCourseFormDto.LessonFiles
        public int? MainFileIndex { get; set; }
    }

    public sealed class UpdatePendingCourseStatusRequest
    {
        public string NewStatus { get; init; } = default!; // "Approve" | "Reject"
        public string? RejectReason { get; init; }
    }

    public class StatisticsCourseDto
    {
        public int ClassCount { get; set; }
        public int StudentCount { get; set; }
    }

}
