using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using InternalTrainingSystem.Core.Common.Constants;

namespace InternalTrainingSystem.Core.Models
{
    public class AssignmentSubmission
    {
        [Key]
        public int SubmissionId { get; set; }
        [Required]
        public int AssignmentId { get; set; }
        [Required]
        public string UserId { get; set; } = null!;
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        [StringLength(20)]
        public string Status { get; set; } = AssignmentSubmissionConstants.Status.Submitted;
        [StringLength(255)]
        public string? FilePath { get; set; }
        public string? MimeType { get; set; }
        public long? SizeBytes { get; set; }
        public string? PublicUrl { get; set; }
        [ForeignKey(nameof(AssignmentId))]
        public Assignment Assignment { get; set; } = null!;
        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; } = null!;
    }
}
