using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using InternalTrainingSystem.Core.Constants;

namespace InternalTrainingSystem.Core.Models
{
    public class AssignmentSubmission
    {
        [Key] public int SubmissionId { get; set; }

        [Required] public int AssignmentId { get; set; }
        [Required] public string UserId { get; set; } = string.Empty;

        public int? EnrollmentId { get; set; } // nếu muốn ràng vào lần ghi danh
        public int AttemptNumber { get; set; } // 1..MaxSubmissions

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow; 
        [StringLength(20)]
        public string Status { get; set; } = AssignmentSubmissionConstants.Status.Submitted;

        // chấm điểm/nhận xét (giảng viên/TD)
        public int? Score { get; set; }
        [StringLength(10)] public string? Grade { get; set; }
        [StringLength(1000)] public string? Feedback { get; set; }

        public bool IsLate { get; set; } = false;

        [ForeignKey(nameof(AssignmentId))] public Assignment Assignment { get; set; } = null!;
        [ForeignKey(nameof(UserId))] public ApplicationUser User { get; set; } = null!;
        [ForeignKey(nameof(EnrollmentId))] public CourseEnrollment? Enrollment { get; set; }

        public ICollection<SubmissionFile> Files { get; set; } = new List<SubmissionFile>();
    }
}
