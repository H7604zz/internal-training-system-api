using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternalTrainingSystem.Core.Models
{
    public class UserAnswer
    {
        [Key]
        public int UserAnswerId { get; set; }

        [StringLength(1000)]
        public string? AnswerText { get; set; } // For essay questions

        public bool IsCorrect { get; set; } = false;

        public int Points { get; set; } = 0;

        public DateTime AnsweredAt { get; set; } = DateTime.UtcNow;

        // Foreign Keys
        [Required]
        public int AttemptId { get; set; }

        [Required]
        public int QuestionId { get; set; }

        public int? AnswerId { get; set; } // For multiple choice questions

        // Navigation Properties
        [ForeignKey("AttemptId")]
        public virtual QuizAttempt QuizAttempt { get; set; } = null!;

        [ForeignKey("QuestionId")]
        public virtual Question Question { get; set; } = null!;

        [ForeignKey("AnswerId")]
        public virtual Answer? Answer { get; set; }
    }
}