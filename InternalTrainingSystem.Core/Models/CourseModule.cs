using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace InternalTrainingSystem.Core.Models
{
    public class CourseModule
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required, StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        public int OrderIndex { get; set; }  // Thứ tự module trong course
        public int? EstimatedMinutes { get; set; }
        public bool IsActive { get; set; } = true;

        [ForeignKey(nameof(CourseId))]
        public Course Course { get; set; } = null!;

        public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
    }
}
