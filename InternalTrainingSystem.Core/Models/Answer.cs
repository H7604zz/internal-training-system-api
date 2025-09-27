using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public class Answer
    {
        [Key]
        public int AnswerId { get; set; }

        [Required]
        [StringLength(500)]
        public string AnswerText { get; set; } = string.Empty;

        public bool IsCorrect { get; set; } = false;

        public int OrderIndex { get; set; }

        public bool IsActive { get; set; } = true;

        // Foreign Keys
        [Required]
        public int QuestionId { get; set; }

        // Navigation Properties
        [ForeignKey("QuestionId")]
        public virtual Question Question { get; set; } = null!;

        public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
    }
}