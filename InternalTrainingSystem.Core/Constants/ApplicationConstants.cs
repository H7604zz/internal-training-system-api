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
            public const string Active = "Active";
            public const string Inactive = "Inactive";
            public const string Pending = "Pending";
            public const string Approve = "Approve";
            public const string Reject = "Reject";
            public const string Deleted = "Deleted";
        }
    }

    public static class ClassConstants
    {
        public static class Status
        {
            public const string Active = "Active";
            public const string Completed = "Completed";
            public const string Cancelled = "Cancelled";
            public const string Scheduled = "Scheduled";
        }
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
            public const string Excused = "Excused";
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
    }
}
