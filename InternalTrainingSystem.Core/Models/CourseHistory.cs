using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternalTrainingSystem.Core.Models
{
    public class CourseHistory
    {
        [Key]
        public int HistoryId { get; set; }

        [Required, StringLength(50)]
        public string Action { get; set; } = string.Empty;
        // Enrolled, Started, ProgressUpdated, MaterialAccessed, MaterialDownloaded,
        // QuizStarted, QuizCompleted, QuizPassed, QuizFailed, Completed, CertificateIssued, ...

        [StringLength(500)]
        public string? Description { get; set; }

        public DateTime ActionDate { get; set; } = DateTime.Now;
        [Required]
        public string UserId { get; set; } = string.Empty;
        [Required]
        public int CourseId { get; set; }
        public int? EnrollmentId { get; set; }            
        public int? QuizId { get; set; }           
        public int? QuizAttemptId { get; set; }    
        public int? ScheduleId { get; set; }       

        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser User { get; set; } = null!;

        [ForeignKey(nameof(CourseId))]
        public virtual Course Course { get; set; } = null!;

        [ForeignKey(nameof(EnrollmentId))]
        public virtual CourseEnrollment? Enrollment { get; set; }

        [ForeignKey(nameof(QuizId))]
        public virtual Quiz? Quiz { get; set; }

        [ForeignKey(nameof(QuizAttemptId))]
        public virtual QuizAttempt? QuizAttempt { get; set; }

        [ForeignKey(nameof(ScheduleId))]
        public virtual Schedule? Schedule { get; set; }
    }
}