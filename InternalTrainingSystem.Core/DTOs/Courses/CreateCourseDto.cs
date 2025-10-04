using System.ComponentModel.DataAnnotations;

namespace InternalTrainingSystem.Core.DTOs.Courses
{
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
}
