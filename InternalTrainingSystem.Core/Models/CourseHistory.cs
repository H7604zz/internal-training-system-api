using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternalTrainingSystem.Core.Models
{
    public class CourseHistory
    {
        [Key]
        public int HistoryId { get; set; }

        [Required]
        public CourseAction Action { get; set; }
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
    public enum CourseAction
    {
        Enrolled = 1,
        Started = 2,
        Paused = 3,
        Resumed = 4,
        Completed = 5,
        Dropped = 6,

        QuizStarted = 10,
        QuizCompleted = 11,
        QuizPassed = 12,
        QuizFailed = 13,
        QuizRetaken = 14,

        CertificateIssued = 20,

        ScheduleRegistered = 30,
        ScheduleAttended = 31,
        ScheduleCancelled = 32,

        ProgressUpdated = 40,
        MaterialAccessed = 41,
        MaterialDownloaded = 42,

        FeedbackSubmitted = 50,

        // ?? Thêm nhóm hành ??ng duy?t khóa h?c
        CourseSubmittedForApproval = 60, // Khi ng??i t?o g?i lên ch? duy?t
        CourseApproved = 61,             // Khi Ban giám ??c duy?t
        CourseRejected = 62,             // Khi b? t? ch?i
        CourseReSubmitted = 63,          // Khi ng??i t?o ch?nh l?i và g?i l?i
        CourseDeletedByManagement = 64   // Khi BOD xóa khóa h?c ?ã duy?t
    }
}