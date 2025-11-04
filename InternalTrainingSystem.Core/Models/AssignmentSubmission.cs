using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using InternalTrainingSystem.Core.Constants;

namespace InternalTrainingSystem.Core.Models
{
    public class AssignmentSubmission
    {
        [Key]
        public int SubmissionId { get; set; }

        [Required]
        public int AssignmentId { get; set; }   // FK -> Assignment

        [Required]
        public string UserId { get; set; } = null!; // FK -> ApplicationUser

        public int? EnrollmentId { get; set; }  // optional FK -> CourseEnrollment

        public int AttemptNumber { get; set; }
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        [StringLength(20)]
        public string Status { get; set; } = AssignmentSubmissionConstants.Status.Submitted;

        public int? Score { get; set; }
        public string? Grade { get; set; }
        [StringLength(1000)]
        public string? Feedback { get; set; }
        public bool IsLate { get; set; }

        [ForeignKey(nameof(AssignmentId))]
        public Assignment Assignment { get; set; } = null!;

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; } = null!;

        [ForeignKey(nameof(EnrollmentId))]
        public CourseEnrollment? Enrollment { get; set; }

        public ICollection<SubmissionFile> Files { get; set; } = new List<SubmissionFile>();
    }
}
