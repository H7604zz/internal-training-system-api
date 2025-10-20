using InternalTrainingSystem.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace InternalTrainingSystem.Core.DTOs
{
    public class CourseListDto
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Duration { get; set; }
        public string Level { get; set; } = string.Empty;
        public string? Department { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? Status { get; set; }
        public List<DepartmentDto> Departments { get; set; } = new();
        public DateTime CreatedDate { get; set; }
    }

    public class CourseDetailDto
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Duration { get; set; }
        public string Level { get; set; } = string.Empty;
        public string? Department { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int? CategoryId { get; set; }
        public string? Prerequisites { get; set; }
        public string? Objectives { get; set; }
        public decimal? Price { get; set; }
        public int EnrollmentCount { get; set; }
        public double AverageRating { get; set; }
        public List<DepartmentDto> Departments { get; set; } = new();
    }

    public class GetCoursesByIdentifiersRequest
    {
        public List<string> Identifiers { get; set; } = new List<string>();
    }

    public class CreateCourseDto
    {
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

    public class UpdateCourseDto
    {
        [Required]
        public int CourseId { get; set; }

        [Required, StringLength(200)]
        public string CourseName { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        public int CourseCategoryId { get; set; }

        [Range(1, int.MaxValue)]
        public int Duration { get; set; }

        [Required, RegularExpression("Beginner|Intermediate|Advanced")]
        public string Level { get; set; } = "Beginner";

        public string? Status { get; set; } = null;

        public List<int>? Departments { get; set; } // danh sách ID phòng ban
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
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int CourseCategoryId { get; set; }
        public string CourseCategoryName { get; set; } = string.Empty;
        public int Duration { get; set; }
        public string Level { get; set; } = "Beginner";
        public string? Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<DepartmentDto> Departments { get; set; } = new();
    }

    public record ToggleStatusDto(string Status);
}
