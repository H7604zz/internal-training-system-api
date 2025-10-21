using InternalTrainingSystem.Core.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternalTrainingSystem.Core.Models
{
    public class Class
    {
        [Key]
        public int ClassId { get; set; }

        [Required]
        [StringLength(200)]
        public string ClassName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required]
        public string MentorId { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [Range(1, 100)]
        public int MaxStudents { get; set; } = 30;

        [StringLength(20)]
        public string Status { get; set; } = ClassConstants.Status.Active; // Active, Completed, Cancelled, Scheduled

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedDate { get; set; }

        public string? CreatedById { get; set; }

        // Navigation Properties
        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; } = null!;

        [ForeignKey("MentorId")]
        public virtual ApplicationUser Mentor { get; set; } = null!;

        [ForeignKey("CreatedById")]
        public virtual ApplicationUser CreatedBy { get; set; } = null!;

        public virtual ICollection<ClassEnrollment> ClassEnrollments { get; set; } = new List<ClassEnrollment>();
        public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
    }
}
