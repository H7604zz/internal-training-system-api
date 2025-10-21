using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace InternalTrainingSystem.Core.Models
{
    public class LessonProgress
    {
        [Required]
        public string UserId { get; set; } = null!;

        [Required]
        public int LessonId { get; set; }

        public bool IsDone { get; set; } = false;
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; } = null!;

        [ForeignKey(nameof(LessonId))]
        public Lesson Lesson { get; set; } = null!;
    }
}
