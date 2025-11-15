using InternalTrainingSystem.Core.Configuration.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternalTrainingSystem.Core.Models
{
    public class Question
    {
        [Key]
        public int QuestionId { get; set; }

        [Required]
        [StringLength(1000)]
        public string QuestionText { get; set; } = string.Empty;

        [StringLength(20)]
        public string QuestionType { get; set; } = QuizConstants.QuestionTypes.MultipleChoice; // MultipleChoice, TrueFalse, Essay

        public int Points { get; set; } = 1;

        public int OrderIndex { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Foreign Keys
        [Required]
        public int QuizId { get; set; }

        // Navigation Properties
        [ForeignKey("QuizId")]
        public virtual Quiz Quiz { get; set; } = null!;

        public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();
        public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
    }
}