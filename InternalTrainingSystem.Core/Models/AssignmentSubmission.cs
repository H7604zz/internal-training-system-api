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
        public int AttemptNumber { get; set; }
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        [StringLength(20)]
        public string Status { get; set; } = AssignmentSubmissionConstants.Status.Submitted;
        public int? Score { get; set; }
        [StringLength(1000)]
        public string? Feedback { get; set; }
        public bool IsLate { get; set; }
        [StringLength(255)]
        public string? OriginalFileName { get; set; }
        public string? FilePath { get; set; }
        public string? MimeType { get; set; }
        public long? SizeBytes { get; set; }
        public string? PublicUrl { get; set; }
        public bool IsMain { get; set; } = true;
        [ForeignKey(nameof(AssignmentId))]
        public Assignment Assignment { get; set; } = null!;
        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; } = null!;
    }
}
