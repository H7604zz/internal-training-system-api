using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternalTrainingSystem.Core.Models
{
    public class CourseHistory
    {
        [Key]
        public int HistoryId { get; set; }

        [Required]
        [StringLength(50)]
        public string Action { get; set; } = string.Empty; // Enrolled, Started, Paused, Resumed, Completed, Dropped, QuizTaken, QuizPassed, QuizFailed, CertificateIssued

        [StringLength(500)]
        public string? Description { get; set; }

        public DateTime ActionDate { get; set; } = DateTime.UtcNow;

        // Foreign Keys
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int CourseId { get; set; }

        public int? EnrollmentId { get; set; } // Reference to CourseEnrollment

        public int? QuizAttemptId { get; set; } // Reference to QuizAttempt if action is quiz-related

        public int? ScheduleId { get; set; } // Reference to Schedule if action is schedule-related

        // Navigation Properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; } = null!;

        [ForeignKey("EnrollmentId")]
        public virtual CourseEnrollment? Enrollment { get; set; }

        [ForeignKey("QuizAttemptId")]
        public virtual QuizAttempt? QuizAttempt { get; set; }

        [ForeignKey("ScheduleId")]
        public virtual Schedule? Schedule { get; set; }
    }
}