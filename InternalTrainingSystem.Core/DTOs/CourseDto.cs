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
        public string CategoryName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class CourseDetailDto
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Duration { get; set; }
        public string Level { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int? CategoryId { get; set; }
        public string? Prerequisites { get; set; }
        public string? Objectives { get; set; }
        public decimal? Price { get; set; }
        public int EnrollmentCount { get; set; }
        public double AverageRating { get; set; }
    }

    public class GetCoursesByIdentifiersRequest
    {
        public List<string> Identifiers { get; set; } = new List<string>();
    }
    
    public class CreateCourseDto
    {
        [Required, StringLength(200)]
        public string CourseName { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        public int CourseCategoryId { get; set; }

        [Range(0, int.MaxValue)]
        public int Duration { get; set; }

        [Required, RegularExpression("Beginner|Intermediate|Advanced")]
        public string Level { get; set; } = "Beginner";
    }
    
    public class CourseSearchRequest
    {
        public string? Q { get; set; }

        public string? Category { get; set; }

        public List<string>? Categories { get; set; }

        public int? CategoryId { get; set; }

        public bool? IsActive { get; set; }
        public string? Level { get; set; }
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
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
