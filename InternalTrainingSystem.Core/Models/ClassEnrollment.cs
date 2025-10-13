using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternalTrainingSystem.Core.Models
{
    public class ClassEnrollment
    {
        [Key]
        public int ClassEnrollmentId { get; set; }

        [Required]
        public int ClassId { get; set; }

        [Required]
        public string StudentId { get; set; } = string.Empty;

        public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;

        [StringLength(20)]
        public string Status { get; set; } = "Enrolled"; // Enrolled, Completed, Dropped, Pending

        public DateTime? CompletionDate { get; set; }

        [Range(0, 100)]
        public decimal? FinalGrade { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedDate { get; set; }

        // Navigation Properties
        [ForeignKey("ClassId")]
        public virtual Class Class { get; set; } = null!;

        [ForeignKey("StudentId")]
        public virtual ApplicationUser Student { get; set; } = null!;
    }
}
