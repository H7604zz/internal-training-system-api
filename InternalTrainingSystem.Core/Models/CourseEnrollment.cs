using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public class CourseEnrollment
    {
        [Key]
        public int EnrollmentId { get; set; }

        public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;

        public DateTime? CompletionDate { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Enrolled"; // Enrolled, InProgress, Completed, Dropped

        public int? Score { get; set; }

        [StringLength(10)]
        public string? Grade { get; set; }

        public int Progress { get; set; } = 0; // 0-100%

        public DateTime? LastAccessedDate { get; set; }

        // Foreign Keys
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int CourseId { get; set; }

        // Navigation Properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; } = null!;

        public virtual ICollection<CourseHistory> CourseHistories { get; set; } = new List<CourseHistory>();
    }
}