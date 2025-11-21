using System.ComponentModel.DataAnnotations;

namespace InternalTrainingSystem.Core.DTOs
{
    public class AssignmentDto
    {
        public int AssignmentId { get; set; }
        public int ClassId { get; set; }
        public int? ScheduleId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? StartAt { get; set; }
        public DateTime? DueAt { get; set; }
        public DateTime? CloseAt { get; set; }
        public bool AllowLateSubmit { get; set; }
        public int MaxSubmissions { get; set; }
        public int? MaxScore { get; set; }
        public string? AttachmentUrl { get; set; }
    }

    public class CreateAssignmentForm
    {
        public int ClassId { get; set; }
        public int? ScheduleId { get; set; }

        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }

        public DateTime? StartAt { get; set; }
        public DateTime? DueAt { get; set; }
        public DateTime? CloseAt { get; set; }

        public bool AllowLateSubmit { get; set; }
        public int MaxSubmissions { get; set; } = 1;
        public int? MaxScore { get; set; } = 10;

        public IFormFile? File { get; set; }
    }
    public class UpdateAssignmentForm
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }

        public DateTime? StartAt { get; set; }
        public DateTime? DueAt { get; set; }
        public DateTime? CloseAt { get; set; }

        public bool AllowLateSubmit { get; set; }
        public int MaxSubmissions { get; set; }
        public int? MaxScore { get; set; }

        // optional: nộp file mới để replace file cũ
        public IFormFile? File { get; set; }
    }
    public class SubmissionFileDto
    {
        public int FileId { get; set; }
        public string OriginalFileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string? MimeType { get; set; }
        public long? SizeBytes { get; set; }
        public string? PublicUrl { get; set; }
        public bool IsMain { get; set; }
    }

    public class AssignmentSubmissionSummaryDto
    {
        public int SubmissionId { get; set; }
        public string UserId { get; set; } = null!;
        public string UserFullName { get; set; } = string.Empty;

        public int AttemptNumber { get; set; }
        public DateTime SubmittedAt { get; set; }
        public bool IsLate { get; set; }

        public string Status { get; set; } = string.Empty;
        public int? Score { get; set; }
        public string? Grade { get; set; }
    }

    public class AssignmentSubmissionDetailDto
    {
        public int SubmissionId { get; set; }
        public int AssignmentId { get; set; }
        public string UserId { get; set; } = null!;
        public string UserFullName { get; set; } = string.Empty;

        public int AttemptNumber { get; set; }
        public DateTime SubmittedAt { get; set; }
        public bool IsLate { get; set; }

        public string Status { get; set; } = string.Empty;
        public int? Score { get; set; }
        public string? Grade { get; set; }
        public string? Feedback { get; set; }

        public List<SubmissionFileDto> Files { get; set; } = new();
    }

    public class CreateSubmissionRequest
    {
        public string? Note { get; set; }
    }

    public class GradeSubmissionDto
    {
        public int? Score { get; set; }
        public string? Grade { get; set; }
        public string? Feedback { get; set; }
        public string? Status { get; set; } // e.g. "Graded", "Returned"
    }
    public class SubmitAssignmentForm
    {
        // Note (hoặc comment) – map sang CreateSubmissionRequest
        public string? Note { get; set; }

        // File duy nhất
        public IFormFile File { get; set; } = null!;
    }
}
