using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace InternalTrainingSystem.Core.Models
{
    public class SubmissionFile
    {
        [Key] public int FileId { get; set; }

        [Required] public int SubmissionId { get; set; }

        [Required, StringLength(255)]
        public string OriginalFileName { get; set; } = string.Empty;

        [Required] public string FilePath { get; set; } = string.Empty; // storage key
        public string? MimeType { get; set; }
        public long? SizeBytes { get; set; }

        public string? PublicUrl { get; set; } // nếu phát link public/presigned
        public bool IsMain { get; set; } = true; // đánh dấu file chính (nếu nhiều file)

        [ForeignKey(nameof(SubmissionId))]
        public AssignmentSubmission Submission { get; set; } = null!;
    }
}
