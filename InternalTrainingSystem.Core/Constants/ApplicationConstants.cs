namespace InternalTrainingSystem.Core.Constants
{
    public static class CourseConstants
    {
        public static class Levels
        {
            public const string Beginner = "Beginner";
            public const string Intermediate = "Intermediate";
            public const string Advanced = "Advanced";
        }

        public static class Status
        {
            public const string Pending = "Pending";
            public const string Approve = "Approved";
            public const string Reject = "Rejected";
            public const string Draft = "Draft";
        }
    }

    public static class ClassConstants
    {
        public static class Status
        {
            public const string Created = "Created";
            public const string Completed = "Completed";
            public const string Cancelled = "Cancelled";
            public const string Scheduled = "Scheduled";
        }
    }

    public static class ClassSwapConstants
    {
        public const string Pending = "Pending";
        public const string Approved = "Approved";
        public const string Cancelled = "Cancelled";
        public const string Rejected = "Rejected";
    }

    public static class EnrollmentConstants
    {
        public static class Status
        {
            public const string NotEnrolled = "NotEnrolled";
            public const string Enrolled = "Enrolled";
            public const string InProgress = "InProgress";
            public const string Completed = "Completed";
            public const string Dropped = "Dropped";
        }
    }

    public static class QuizConstants
    {
        public static class Status
        {
            public const string InProgress = "InProgress";
            public const string Completed = "Completed";
            public const string TimedOut = "TimedOut";
        }

        public static class QuestionTypes
        {
            public const string MultipleChoice = "MultipleChoice";
            public const string TrueFalse = "TrueFalse";
            public const string Essay = "Essay";
        }
    }

    public static class ScheduleConstants
    {
        public static class Status
        {
            public const string Scheduled = "Scheduled";
            public const string InProgress = "InProgress";
            public const string Completed = "Completed";
            public const string Cancelled = "Cancelled"; 
            public const string Rescheduled = "Rescheduled";
        }

        public static class ParticipantStatus
        {
            public const string Registered = "Registered";
            public const string Attended = "Attended";
            public const string NoShow = "NoShow";
            public const string Cancelled = "Cancelled";
        }
    }

    public static class CourseHistoryConstants
    {
        public static class Actions
        {
            public const string Enrolled = "Enrolled";
            public const string Started = "Started";
            public const string Paused = "Paused";
            public const string Resumed = "Resumed";
            public const string Completed = "Completed";
            public const string Dropped = "Dropped";
            public const string QuizStarted = "QuizStarted";
            public const string QuizCompleted = "QuizCompleted";
            public const string QuizPassed = "QuizPassed";
            public const string QuizFailed = "QuizFailed";
            public const string QuizRetaken = "QuizRetaken";
            public const string CertificateIssued = "CertificateIssued";
            public const string ScheduleRegistered = "ScheduleRegistered";
            public const string ScheduleAttended = "ScheduleAttended";
            public const string ScheduleCancelled = "ScheduleCancelled";
            public const string ProgressUpdated = "ProgressUpdated";
            public const string MaterialAccessed = "MaterialAccessed";
            public const string MaterialDownloaded = "MaterialDownloaded";
            public const string FeedbackSubmitted = "FeedbackSubmitted";
        }
    }

    public static class AttendanceConstants
    {
        public static class Status
        {
            public const string Present = "Present";
            public const string Absent = "Absent";
            public const string NotYet = "NotYet";
        }
    }

    public static class UserRoles
    {
        public const string Staff = "Staff"; // Nhân viên tham gia đào tạo
        public const string DirectManager = "DirectManager"; // Quản lý trực tiếp nhân viên
        public const string BoardOfDirectors = "BoardOfDirectors"; // Ban giám đốc
        public const string Administrator = "Administrator"; // Admin hệ thống
        public const string Mentor = "Mentor"; // Người hướng dẫn/giảng viên
        public const string TrainingDepartment = "TrainingDepartment"; // Phòng đào tạo
        public const string HR = "HR"; // Phòng nhân sự
        public const string System = "System"; // Hệ thống/AI chatbot

        // ✅ Thêm mảng chứa tất cả role
        public static readonly string[] All =
    {
        Staff,
        DirectManager,
        BoardOfDirectors,
        Administrator,
        Mentor,
        TrainingDepartment,
        HR,
        System
    };
    }

    public static class LessonContentConstraints
    {
        // Docs
        public static readonly ISet<string> AllowedDocExtensions =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".pdf", ".doc", ".docx", ".txt", ".pptx" };

        public static readonly ISet<string> AllowedDocContentTypes =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "application/pdf",
                "application/msword",
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "text/plain", 
                "application/vnd.openxmlformats-officedocument.presentationml.presentation" 
            };

        // Videos
        public static readonly ISet<string> AllowedVideoExtensions =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".mp4", ".mov", ".m4v", ".webm" };

        public static readonly ISet<string> AllowedVideoContentTypes =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "video/mp4",
                "video/quicktime",
                "video/x-m4v",
                "video/webm"
            };

        // Size limits
        public const long MaxDocBytes = 20L * 1024 * 1024;  // 20MB
        public const long MaxVideoBytes = 500L * 1024 * 1024;  // 500MB

        // Helper checks (tiện dùng trong service)
        public static bool IsAllowedDoc(string ext, string contentType) =>
            AllowedDocExtensions.Contains(ext) && AllowedDocContentTypes.Contains(contentType);

        public static bool IsAllowedVideo(string ext, string contentType) =>
            AllowedVideoExtensions.Contains(ext) && AllowedVideoContentTypes.Contains(contentType);
    }
    public static class AssignmentSubmissionConstants
    {
        public static class Status
        {
            public const string Submitted = "Submitted";   // staff đã nộp
            public const string Returned = "Returned";     // trả bài yêu cầu sửa
            public const string Graded = "Graded";         // đã chấm
            public const string Rejected = "Rejected";     // từ chối bài nộp (sai format, gian lận…)
        }
    }
}
