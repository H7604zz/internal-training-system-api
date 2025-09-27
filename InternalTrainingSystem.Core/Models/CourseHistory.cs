using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
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

        [Column(TypeName = "decimal(5,2)")]
        public decimal? ProgressBefore { get; set; } // Progress before this action (0-100%)

        [Column(TypeName = "decimal(5,2)")]
        public decimal? ProgressAfter { get; set; } // Progress after this action (0-100%)

        public int? ScoreBefore { get; set; } // Score before action (for quiz-related actions)

        public int? ScoreAfter { get; set; } // Score after action (for quiz-related actions)

        [StringLength(100)]
        public string? AdditionalData { get; set; } // JSON string for extra data if needed

        public DateTime ActionDate { get; set; } = DateTime.UtcNow;

        [StringLength(100)]
        public string? DeviceInfo { get; set; } // Device/browser info

        [StringLength(45)]
        public string? IPAddress { get; set; }

        public TimeSpan? TimeSpent { get; set; } // Duration of activity session

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