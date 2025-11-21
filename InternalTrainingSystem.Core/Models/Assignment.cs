using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace InternalTrainingSystem.Core.Models
{
    public class Assignment
    {
        [Key]
        public int AssignmentId { get; set; }

        [Required]
        public int ClassId { get; set; }          // FK -> Class

        public int? ScheduleId { get; set; }      // optional: 1 buổi cụ thể

        [Required, StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Description { get; set; }

        public DateTime? StartAt { get; set; }
        public DateTime? DueAt { get; set; }
        public DateTime? CloseAt { get; set; }

        public bool AllowLateSubmit { get; set; } = false;
        public int MaxSubmissions { get; set; } = 1;
        public int? MaxScore { get; set; } = 10;

        public string? AttachmentUrl { get; set; }
        public string? AttachmentFilePath { get; set; }
        public string? AttachmentMimeType { get; set; }
        public long? AttachmentSizeBytes { get; set; }

        [ForeignKey(nameof(ClassId))]
        public Class Class { get; set; } = null!;

        [ForeignKey(nameof(ScheduleId))]
        public Schedule? Schedule { get; set; }

        public ICollection<AssignmentSubmission> Submissions { get; set; } = new List<AssignmentSubmission>();
    }
}
