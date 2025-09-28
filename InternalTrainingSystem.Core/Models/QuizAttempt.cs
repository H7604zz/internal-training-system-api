using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternalTrainingSystem.Core.Models
{
    public class QuizAttempt
    {
        [Key]
        public int AttemptId { get; set; }

        public DateTime StartTime { get; set; } = DateTime.UtcNow;

        public DateTime? EndTime { get; set; }

        public int Score { get; set; } = 0;

        public int MaxScore { get; set; }

        public double Percentage { get; set; } = 0;

        [StringLength(20)]
        public string Status { get; set; } = "InProgress"; // InProgress, Completed, TimedOut

        public bool IsPassed { get; set; } = false;

        public int AttemptNumber { get; set; }

        // Foreign Keys
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int QuizId { get; set; }

        // Navigation Properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;

        [ForeignKey("QuizId")]
        public virtual Quiz Quiz { get; set; } = null!;

        public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
        public virtual ICollection<CourseHistory> CourseHistories { get; set; } = new List<CourseHistory>();
    }
}